using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using cov_path_navi;
using game_config;
using game_stuff;

namespace game_bot
{
    public enum BotStatus
    {
        OnPatrol,
        TargetApproach,
        EngageAct,
        GoMaybe
    }

    public class SimpleBot
    {
        private bool HavePeered { get; set; }

        public CharacterBody BotBody { get; }

        public PatrolCtrl PatrolCtrl { get; }

        public FirstSkillCtrl FirstSkillCtrl { get; }

        public ComboCtrl ComboCtrl { get; }
        private BotStatus BotStatus { get; set; }
        public List<(float range, int maxAmmoUse, int weaponIndex)> RangeToWeapon { get; set; }


        private List<TwoDPoint> PathPoints { get; }
        private Random Random { get; }
        private bool InVehicle { get; }

        private ICanBeAndNeedHit? LockBody { get; set; }
        private int NowLockTraceTick { get; set; }
        private HashSet<ICanBeAndNeedHit> Traps { get; }

        private static List<(float min, int i, int Key)> GetRangeAmmoWeapon(Dictionary<int, Weapon> weapons,
            size bodySize)
        {
            var valueTuples = (from keyValuePair in weapons
                let weapon = keyValuePair.Value
                let min = weapon.BotRanges
                let i = weapon.SkillGroups[bodySize].Values
                    .Max(skills => skills.Values.Max(skill => skill.AmmoCost))
                select (min, i, keyValuePair.Key)).ToList();

            valueTuples.Sort((x, y) => -x.Item1.CompareTo(y.Item1));
            return valueTuples;
        }

        public static SimpleBot GenById(int botId, CharacterBody body, Random random, PathTop? pathTop)
        {
            return CommonConfig.Configs.battle_bots.TryGetValue(botId, out var battleNpc)
                ? GenByConfig(battleNpc, body, random, pathTop)
                : throw new KeyNotFoundException($"not found id :: {botId}");
        }

        private static SimpleBot GenByConfig(battle_bot battleBot, CharacterBody body, Random random,
            PathTop? pathTop)
        {
            var polyCount = pathTop?.GetPolyCount() ?? 0;
            var next = random.Next((int) (polyCount * BotLocalConfig.PatrolMin),
                (int) (polyCount * BotLocalConfig.PatrolMax + 1));
            var twoDPoints = pathTop?.GetPatrolPts(random, next) ?? new List<TwoDPoint>();
            var battleNpcActWeight = battleBot.ActWeight;
            var weight = battleNpcActWeight.FirstOrDefault(x => x.op == botOp.none)?.weight ?? 0;
            var valueTuples = battleNpcActWeight.Where(x => x.op != botOp.none)
                .Select(x => (x.weight, BehaviorTreeFunc.CovOp(x.op)));
            var firstSkillCtrl = new FirstSkillCtrl(valueTuples, weight,
                (int) (battleBot.DoNotMinMaxTime.item2),
                (int) battleBot.DoNotMinMaxTime.item1,
                (int) battleBot.ActShowDelayTime);

            return new SimpleBot(body, random, twoDPoints, firstSkillCtrl, battleBot.MaxCombo);
        }

     

        private SimpleBot(CharacterBody botBody, Random random, List<TwoDPoint> patrolPts,
            FirstSkillCtrl firstSkillCtrl,
            int comboMax)
        {
            var valueTuples = GetRangeAmmoWeapon(botBody.CharacterStatus.GetWeapons(), botBody.GetSize());

            BotBody = botBody;
            Random = random;
            FirstSkillCtrl = firstSkillCtrl;
            LockBody = null;
            InVehicle = botBody.CharacterStatus.NowVehicle != null;
            ComboCtrl = new ComboCtrl(comboMax);
            BotStatus = BotStatus.OnPatrol;
            PatrolCtrl = new PatrolCtrl(patrolPts);
            PathPoints = new List<TwoDPoint>();
            RangeToWeapon = valueTuples;
            HavePeered = true;
            NowLockTraceTick = 0;
            Traps = new HashSet<ICanBeAndNeedHit>();
        }


        private Operate? SeeATargetAction(ICanBeAndNeedHit canBeAndNeedHit)
        {
            LockBody = canBeAndNeedHit;
            NowLockTraceTick = (int) BotLocalConfig.LockTraceTickTime;
            BotStatus = BotStatus.TargetApproach;
            var botBodyCharacterStatus = BotBody.CharacterStatus;
            var valueTuple = botBodyCharacterStatus.Prop?.BotUseWhenSeeEnemy(
                botBodyCharacterStatus);
            if (valueTuple.HasValue && valueTuple.Value)
            {
                return new Operate(specialAction: SpecialAction.UseProp);
            }

            return null;
        }

        private void ReturnOnPatrol(PathTop? pathTop)
        {
            BotStatus = BotStatus.OnPatrol;
            var twoDPoint = PatrolCtrl.GetNowPt();
            if (twoDPoint == null) return;
            var twoDPoints = pathTop?.FindGoPts(BotBody.GetAnchor(), twoDPoint) ?? new TwoDPoint[] { };
            PathPoints.AddRange(twoDPoints);
        }

        private TwoDVector? MoveToPt(TwoDPoint pt)
        {
            var twoDVector = new TwoDVector(BotBody.GetAnchor(), pt).GetUnit2();
#if DEBUG
            Console.Out.WriteLine($" {BotBody.GetAnchor()} go direction {twoDVector}");
#endif
            return twoDVector;
        }

        public BotOpAndThink BotSimpleGoATick(PlayerTickSense perceivable,
            ImmutableHashSet<IHitMsg> immutableHashSet, PathTop? pathTop)
        {
            var notMoveCanBeAndNeedSews = perceivable.AppearNotMove.OfType<ICanBeAndNeedHit>()
                .Where(x => x.GetTeam() != BotBody.Team);
            var moveCanBeAndNeedSews = perceivable.VanishNotMove.OfType<ICanBeAndNeedHit>();
            Traps.UnionWith(notMoveCanBeAndNeedSews);
            Traps.ExceptWith(moveCanBeAndNeedSews);
            var canBeEnemies = perceivable.OnChangingBodyAndRadarSee.ToList();
            var canBeHits = canBeEnemies.OfType<ICanBeAndNeedHit>()
                .Where(x => x.GetTeam() != BotBody.Team);

            var targets = canBeHits.Aggregate((ICanBeAndNeedHit?) null,
                CanBeAndNeedHit);
            if (targets == null)
            {
                var accumulate = Traps.Aggregate((ICanBeAndNeedHit?) null, CanBeAndNeedHit);
                targets = accumulate;
            }

            // var canBeHitPts = canBeHits
            //     .Select(x => x.GetAnchor()).ToList();
            var radarSees = canBeEnemies.OfType<RadarSee>().Select(x => x.GetAnchor()).ToList();

            if (InVehicle)
            {
                if (CheckVehicle() != null) return new BotOpAndThink(new Operate(specialAction: CheckVehicle()));
            }

            var botBodyCharacterStatus = BotBody.CharacterStatus;
            var charEvents = botBodyCharacterStatus.CharEvents;
#if DEBUG
            Console.Out.WriteLine($"char Events Count {charEvents.Count}");
#endif
            var characterStatusIsBeHitBySomeOne =
                charEvents.OfType<HitMark>().FirstOrDefault()?.HitDirV;
            if (characterStatusIsBeHitBySomeOne != null && BotStatus == BotStatus.OnPatrol)
            {
#if DEBUG
                Console.Out.WriteLine($"hit by someOne not in sight? {characterStatusIsBeHitBySomeOne}");
#endif
                PathPoints.Clear();
                var twoDPoint = BotBody.GetAnchor().Move(characterStatusIsBeHitBySomeOne.GetUnit().Multi(2f));
                PathPoints.Add(twoDPoint);
                HavePeered = false;
                BotStatus = BotStatus.GoMaybe;
                return new BotOpAndThink();
            }

            if (CheckStun())
            {
                ComboCtrl.ComboLoss();
                return new BotOpAndThink();
            }


            var characterStatusNowWeapon = botBodyCharacterStatus.NowWeapon;
            var weapon = botBodyCharacterStatus.Weapons[characterStatusNowWeapon];

            switch (BotStatus)
            {
                case BotStatus.OnPatrol:
                    if (targets != null)
                    {
                        var seeATargetAction = SeeATargetAction(targets);
                        return new BotOpAndThink(seeATargetAction);
                    }

                    if (radarSees.Any())
                    {
                        var twoDPoint = NearPt(radarSees);
                        var twoDPoints = pathTop?.FindGoPts(BotBody.GetAnchor(), twoDPoint) ?? new TwoDPoint[] { };
                        PathPoints.Clear();
                        PathPoints.AddRange(twoDPoints);
                        HavePeered = false;
                        BotStatus = BotStatus.GoMaybe;
                        return new BotOpAndThink();
                    }

                    var doPatrol = DoPatrol();
                    return new BotOpAndThink(doPatrol);
                case BotStatus.GoMaybe:
                    if (targets != null)
                    {
                        HavePeered = true;
                        return new BotOpAndThink(SeeATargetAction(targets));
                    }

                    var goPathDirection = GoPathDirection();
                    if (goPathDirection == null)
                    {
                        HavePeered = true;
                        ReturnOnPatrol(pathTop);
                    }

                    if (HavePeered || PathPoints.Count < 1)
                        return new BotOpAndThink(new Operate(move: goPathDirection));
                    var distance1 = PathPoints.First().GetDistance(BotBody.GetAnchor());
                    var (inRange1, needSwitch1) = CheckWeaponAndAmmo(distance1);
                    if (needSwitch1)
                    {
                        return new BotOpAndThink(new Operate(skillAction: SkillAction.Switch));
                    }

                    if (!inRange1) return new BotOpAndThink(new Operate(move: goPathDirection));
                    var zoomStepScopes = weapon
                        .ZoomStepScopes;
                    if (botBodyCharacterStatus.GetNowSnipeStep() >= zoomStepScopes.Length)
                    {
                        HavePeered = true;
                        return new BotOpAndThink(new Operate(move: goPathDirection,
                            snipeAction: SnipeAction.SnipeOff));
                    }

                    if (!zoomStepScopes.Any()) return new BotOpAndThink(new Operate(move: goPathDirection));
                    var keyValuePair = weapon.Snipes.Aggregate((KeyValuePair<SnipeAction, Snipe>?) null,
                        (s, x) =>
                            !s.HasValue
                                ? x
                                : x.Value.MaxStep > s.Value.Value.MaxStep
                                    ? x
                                    : s);
                    if (keyValuePair == null) return new BotOpAndThink(new Operate(move: goPathDirection));
                    var snipeAction = keyValuePair.Value.Key;
                    return new BotOpAndThink(new Operate(move: goPathDirection,
                        snipeAction: snipeAction));

                case BotStatus.TargetApproach:
#if DEBUG
                    Console.Out.WriteLine("TargetApproach");
#endif
                    var characterStatusProp = botBodyCharacterStatus.Prop;
                    if (characterStatusProp != null &&
                        characterStatusProp.CheckAppStatusToBotPropUse(botBodyCharacterStatus))
                    {
                        var botSimpleGoATick = new Operate(specialAction: SpecialAction.UseProp);
                        return new BotOpAndThink(botSimpleGoATick);
                    }

                    if (targets != null)
                    {
                        var goATick = FirstSkillCtrl.GoATick(Random);
                        var twoDPoint = targets.GetAnchor();

                        var distance = twoDPoint.GetDistance(BotBody.GetAnchor());
                        var (inRange, needSwitch) = CheckWeaponAndAmmo(distance);
                        if (needSwitch)
                        {
                            return new BotOpAndThink(new Operate(skillAction: SkillAction.Switch), goATick);
                        }

                        var twoDVector = MoveToPt(twoDPoint);
                        if (inRange)
                        {
                            var skillAction = FirstSkillCtrl.GetAction();
                            if (skillAction != null && CheckSkillSnipeNeed(skillAction.Value))
                            {
                                return new BotOpAndThink(
                                    new Operate(snipeAction: GetSnipeActBySkillAct(skillAction)));
                            }

                            BotStatus = BotStatus.EngageAct;

                            return new BotOpAndThink(new Operate(aim: twoDVector, skillAction: skillAction), goATick);
                        }

                        var operate = new Operate(move: twoDVector);
                        return new BotOpAndThink(operate, goATick);
                    }

                    if (LockBody != null)
                    {
                        NowLockTraceTick--;
                        var distance = LockBody.GetAnchor().GetDistance(this.BotBody.GetAnchor());
                        var closeEnough = distance > BotLocalConfig.MaxTraceDistance;
                        if (closeEnough || NowLockTraceTick <= 0)
                        {
                            LockBody = null;
                            ReturnOnPatrol(pathTop);
                            return new BotOpAndThink();
                        }

                        var twoDVector = MoveToPt(LockBody.GetAnchor());
                        var operate = new Operate(move: twoDVector);
                        return new BotOpAndThink(operate);
                    }

                    ReturnOnPatrol(pathTop);
                    return new BotOpAndThink();
                case BotStatus.EngageAct:

                    if (!ActFin())
                    {
#if DEBUG
                        Console.Out.WriteLine("now acting");
#endif
                        if (CheckStartCombo(immutableHashSet))
                        {
                            ComboCtrl.ComboTurnOn(Random);
                        }

                        return new BotOpAndThink();
                    }


                    if (ComboCtrl.CanCombo())
                    {
                        var comboAction = FirstSkillCtrl.GetComboAction(Random);
#if DEBUG
                        Console.Out.WriteLine($"rand skill is {comboAction}");
#endif
                        var checkSkillSnipeNeed = CheckSkillSnipeNeed(comboAction);
                        if (checkSkillSnipeNeed)
                        {
                            return new BotOpAndThink(
                                new Operate(snipeAction: GetSnipeActBySkillAct(comboAction)));
                        }

                        ComboCtrl.ActACombo();
                        var canBeAndNeedHit = LockBody == null
                            ? null
                            : BotBody.GetAnchor().GenVector(LockBody.GetAnchor()).GetUnit2();
                        return new BotOpAndThink(new Operate(aim: canBeAndNeedHit, skillAction: comboAction));
                    }

                    BotStatus = BotStatus.TargetApproach;
                    return new BotOpAndThink();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ICanBeAndNeedHit CanBeAndNeedHit(ICanBeAndNeedHit? s, ICanBeAndNeedHit x)
        {
            if (s == null)
            {
                return x;
            }

            var distance1 = BotBody.GetAnchor().GetDistance(s.GetAnchor());
            var distance2 = BotBody.GetAnchor().GetDistance(x.GetAnchor());
            var canBeHit = distance1 > distance2 ? x : s;
            return canBeHit;
        }

        private bool CheckSkillSnipeNeed(SkillAction comboAction)
        {
            var botBodyCharacterStatus = BotBody.CharacterStatus;
            var characterStatusWeapons = botBodyCharacterStatus.Weapons;
            var nowWeapon = botBodyCharacterStatus.NowWeapon;
            var size = BotBody.GetSize();
            var immutableDictionary = characterStatusWeapons[nowWeapon].SkillGroups[size];
            return immutableDictionary.TryGetValue(comboAction, out var skills) &&
                   skills.Max(x => x.Value.SnipeStepNeed) >
                   botBodyCharacterStatus.GetNowSnipeStep();
        }

        private static SnipeAction? GetSnipeActBySkillAct(SkillAction? skillAction)
        {
            return skillAction switch
            {
                SkillAction.Op1 => SnipeAction.SnipeOn1,
                SkillAction.Op2 => SnipeAction.SnipeOn2,
                SkillAction.Op3 => SnipeAction.SnipeOn3,
                SkillAction.Switch => SnipeAction.SnipeOff,
                SkillAction.CatchTrick => SnipeAction.SnipeOff,
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(skillAction), skillAction, null)
            };
        }


        private static bool CheckStartCombo(ImmutableHashSet<IHitMsg> immutableHashSet)
        {
            return immutableHashSet.Any();
        }

        private bool CheckStun()
        {
            return BotBody.CharacterStatus.StunBuff != null;
        }

        private bool ActFin()
        {
            return BotBody.CharacterStatus.NowCastAct?.ComboInputRes() == null;
        }


        private TwoDPoint NearPt(List<TwoDPoint> pts)
        {
            pts.Sort((a, b) =>
                BotBody.GetAnchor().GetDistance(a)
                    .CompareTo(BotBody.GetAnchor().GetDistance(b)));
            var nearPt = pts.First();
            return nearPt;
        }

        public SpecialAction? CheckVehicle()
        {
            var characterStatusNowVehicle = BotBody.CharacterStatus.NowVehicle;
            if (!(characterStatusNowVehicle is {IsDestroyOn: true})) return null;
            var rangeAmmoWeapon = GetRangeAmmoWeapon(BotBody.CharacterStatus.Weapons, BotBody.BodySize);
            RangeToWeapon = rangeAmmoWeapon;
            return SpecialAction.OutVehicle;
        }

        private Operate DoPatrol()
        {
            if (PathPoints.Any())
            {
                var goPathDirection = GoPathDirection()?.Multi(BotLocalConfig.PatrolSlowMulti
                );
                if (goPathDirection != null) return new Operate(move: goPathDirection);
                var canUseWhenPatrol = BotBody.CharacterStatus.Prop?.CanUseWhenPatrol(BotBody.CharacterStatus, Random);
                if (canUseWhenPatrol.HasValue && canUseWhenPatrol.Value)
                {
                    return new Operate(specialAction: SpecialAction.UseProp);
                }
            }


            var next = Random.Next(PatrolCtrl.GetPtNum());
            var twoDPoints = PatrolCtrl.NextPt(next);
            PathPoints.AddRange(twoDPoints);
            return new Operate();
        }

        private bool CloseEnough(TwoDPoint twoDPoint)
        {
            var twoDVectorLine = BotBody.GetMoveVectorLine();
            var distance = twoDPoint.GetLineDistance(twoDVectorLine);
#if DEBUG
            Console.Out.WriteLine($"distance to pt{twoDPoint}  {BotBody.GetMoveVectorLine()} is {distance}");
#endif
            return distance < BotLocalConfig.CloseEnoughDistance;
        }

        private TwoDVector? GoPathDirection()
        {
            var firstOrDefault = PathPoints.FirstOrDefault();

            if (firstOrDefault == null) return null;

            var closeEnough = CloseEnough(firstOrDefault);
            if (!closeEnough) return MoveToPt(firstOrDefault);
#if DEBUG
                Console.Out.WriteLine("close enough pts rv ");
#endif
            PathPoints.RemoveAt(0);
            return null;
        }

        private (bool inRange, bool needSwitch) CheckWeaponAndAmmo(float distance)
        {
            var botBodyCharacterStatus = BotBody.CharacterStatus;
            var valueTuples = RangeToWeapon.Where(x => x.maxAmmoUse <= botBodyCharacterStatus.NowAmmo).ToArray();
            if (!valueTuples.Any())
            {
                return (false, false);
            }

            var longestWeapon = valueTuples.First();
            var characterStatusNowWeapon = botBodyCharacterStatus.NowWeapon;
            var b = botBodyCharacterStatus.NowCastAct == null;

            if (characterStatusNowWeapon != longestWeapon.weaponIndex) return (false, b);
            var enumerable = valueTuples.Where(a => a.range >= distance).ToArray();
            if (!enumerable.Any())
            {
                return (false, false);
            }

            var okWeapon = enumerable.First();
            return characterStatusNowWeapon == okWeapon.weaponIndex ? (true, false) : (false, b);
        }

        public TwoDPoint? GetStartPt()
        {
            var twoDPoint = PatrolCtrl.GetNowPt();
            return twoDPoint;
        }
    }
}