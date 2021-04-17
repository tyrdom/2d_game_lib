using System;
using System.Collections.Generic;
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

        private ICanBeHit? LockBody { get; set; }


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

        public static SimpleBot GenById(int npcId, CharacterBody body, Random random, PathTop pathTop)
        {
            return CommonConfig.Configs.battle_npcs.TryGetValue(npcId, out var battleNpc)
                ? GenByConfig(battleNpc, body, random, pathTop)
                : throw new KeyNotFoundException($"not found id :: {npcId}");
        }

        public static SimpleBot GenByConfig(battle_npc battleNpc, CharacterBody body, Random random,
            PathTop pathTop)
        {
            var polyCount = pathTop.GetPolyCount();
            var next = random.Next((int) (polyCount * BotLocalConfig.PatrolMin),
                (int) (polyCount * BotLocalConfig.PatrolMax + 1));
            var twoDPoints = pathTop.GetPatrolPts(random, next);
            var battleNpcActWeight = battleNpc.ActWeight;
            var weight = battleNpcActWeight.FirstOrDefault(x => x.op == botOp.none)?.weight ?? 0;
            var valueTuples = battleNpcActWeight.Where(x => x.op != botOp.none)
                .Select(x => (x.weight, CovOp(x.op)));
            var firstSkillCtrl = new FirstSkillCtrl(valueTuples, weight,
                CommonConfig.GetIntTickByTime(battleNpc.DoNotMinMaxTime.item2),
                CommonConfig.GetIntTickByTime(battleNpc.DoNotMinMaxTime.item1),
                CommonConfig.GetIntTickByTime(battleNpc.ActShowDelayTime));

            return new SimpleBot(body, random, twoDPoints, firstSkillCtrl, battleNpc.MaxCombo);
        }

        private static SkillAction CovOp(botOp botOp)
        {
            return botOp switch
            {
                botOp.op1 => SkillAction.Op1,
                botOp.op2 => SkillAction.Op2,
                botOp.none => SkillAction.Op1,
                _ => throw new ArgumentOutOfRangeException(nameof(botOp), botOp, null)
            };
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
        }


        private Operate? SeeATargetAction(ICanBeHit canBeHit)
        {
            LockBody = canBeHit;
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

        private void ReturnOnPatrol(PathTop pathTop)
        {
            BotStatus = BotStatus.OnPatrol;
            var twoDPoint = PatrolCtrl.GetNowPt();
            var twoDPoints = pathTop.FindGoPts(BotBody.GetAnchor(), twoDPoint);
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

        public BotOpAndThink BotSimpleGoATick(IEnumerable<ICanBeEnemy> perceivable, PathTop pathTop)
        {
            var canBeEnemies = perceivable.ToList();
            var canBeHits = canBeEnemies.OfType<ICanBeHit>()
                .Where(x => x.GetTeam() != BotBody.Team);

            var beHit = canBeHits.Aggregate((ICanBeHit?) null, (s, x) =>
            {
                if (s == null)
                {
                    return x;
                }

                var distance1 = BotBody.GetAnchor().GetDistance(s.GetAnchor());
                var distance2 = BotBody.GetAnchor().GetDistance(x.GetAnchor());
                var canBeHit = distance1 > distance2 ? x : s;
                return canBeHit;
            });
            // var canBeHitPts = canBeHits
            //     .Select(x => x.GetAnchor()).ToList();
            var radarSees = canBeEnemies.OfType<RadarSee>().Select(x => x.GetAnchor()).ToList();

            if (InVehicle)
            {
                if (CheckVehicle() != null) return new BotOpAndThink(new Operate(specialAction: CheckVehicle()));
            }

            if (CheckStun())
            {
                ComboCtrl.ComboLoss();
                return new BotOpAndThink();
            }

            var botBodyCharacterStatus = BotBody.CharacterStatus;

            var characterStatusNowWeapon = botBodyCharacterStatus.NowWeapon;
            var weapon = botBodyCharacterStatus.Weapons[characterStatusNowWeapon];

            switch (BotStatus)
            {
                case BotStatus.OnPatrol:
                    if (beHit != null)
                    {
                        return new BotOpAndThink(SeeATargetAction(beHit));
                    }

                    if (radarSees.Any())
                    {
                        var twoDPoint = NearPt(radarSees);
                        var twoDPoints = pathTop.FindGoPts(BotBody.GetAnchor(), twoDPoint);
                        PathPoints.Clear();
                        PathPoints.AddRange(twoDPoints);
                        HavePeered = false;
                        BotStatus = BotStatus.GoMaybe;
                        return new BotOpAndThink();
                    }

                    var characterStatusIsBeHitBySomeOne = botBodyCharacterStatus.IsBeHitBySomeOne;
                    if (characterStatusIsBeHitBySomeOne != null)
                    {
                        PathPoints.Clear();
                        var twoDPoint = BotBody.GetAnchor().Move(characterStatusIsBeHitBySomeOne);
                        PathPoints.Add(twoDPoint);
                        BotStatus = BotStatus.GoMaybe;
                        return new BotOpAndThink();
                    }

                    var doPatrol = DoPatrol();
                    return new BotOpAndThink(doPatrol);
                case BotStatus.GoMaybe:
                    if (beHit != null)
                    {
                        HavePeered = true;
                        return new BotOpAndThink(SeeATargetAction(beHit));
                    }

                    var goPathDirection = GoPathDirection();
                    if (goPathDirection == null)
                    {
                        ReturnOnPatrol(pathTop);
                    }

                    if (HavePeered || PathPoints.Count != 1)
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
                    var characterStatusProp = botBodyCharacterStatus.Prop;
                    if (characterStatusProp != null &&
                        characterStatusProp.CheckAppStatusToBotPropUse(botBodyCharacterStatus))
                    {
                        var botSimpleGoATick = new Operate(specialAction: SpecialAction.UseProp);
                        return new BotOpAndThink(botSimpleGoATick);
                    }

                    if (beHit != null)
                    {
                        var goATick = FirstSkillCtrl.GoATick(Random);
                        var twoDPoint = beHit.GetAnchor();

                        var distance = twoDPoint.GetDistance(BotBody.GetAnchor());
                        var (inRange, needSwitch) = CheckWeaponAndAmmo(distance);
                        if (needSwitch)
                        {
                            return new BotOpAndThink(new Operate(skillAction: SkillAction.Switch), goATick);
                        }

                        if (inRange)
                        {
                            var skillAction = FirstSkillCtrl.GetAction(Random);
                            if (skillAction != null && CheckSkillSnipeNeed(skillAction.Value))
                            {
                                return new BotOpAndThink(
                                    new Operate(snipeAction: GetSnipeActBySkillAct(skillAction)));
                            }

                            BotStatus = BotStatus.EngageAct;
                            return new BotOpAndThink(new Operate(skillAction: skillAction), goATick);
                        }

                        var operate = new Operate(move: MoveToPt(twoDPoint));
                        return new BotOpAndThink(operate, goATick);
                    }

                    if (LockBody != null)
                    {
                        var closeEnough = LockBody.GetAnchor().GetDistance(this.BotBody.GetAnchor()) >
                                          BotLocalConfig.MaxTraceDistance;
                        if (closeEnough)
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
                        if (CheckStartCombo())
                        {
                            ComboCtrl.ComboTurnOn();
                        }

                        return new BotOpAndThink();
                    }


                    if (ComboCtrl.CanCombo())
                    {
                        var comboAction = FirstSkillCtrl.GetComboAction(Random);
                        var checkSkillSnipeNeed = CheckSkillSnipeNeed(comboAction);
                        if (checkSkillSnipeNeed)
                        {
                            return new BotOpAndThink(
                                new Operate(snipeAction: GetSnipeActBySkillAct(comboAction)));
                        }

                        ComboCtrl.ActACombo();
                        return new BotOpAndThink(new Operate(skillAction: comboAction));
                    }

                    BotStatus = BotStatus.TargetApproach;
                    return new BotOpAndThink();
                default:
                    throw new ArgumentOutOfRangeException();
            }
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


        private bool CheckStartCombo()
        {
            return BotBody.CharacterStatus.IsHitSome;
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
                return new Operate(move: goPathDirection);
            }

            var canUseWhenPatrol = BotBody.CharacterStatus.Prop?.CanUseWhenPatrol(BotBody.CharacterStatus);
            if (canUseWhenPatrol.HasValue && canUseWhenPatrol.Value)
            {
                return new Operate(specialAction: SpecialAction.UseProp);
            }

            var next = Random.Next(PatrolCtrl.GetPtNum());
            var twoDPoints = PatrolCtrl.NextPt(next);
            PathPoints.AddRange(twoDPoints);
            return new Operate();
        }

        private bool CloseEnough(TwoDPoint twoDPoint)
        {
            var distance = twoDPoint.GetDistance(BotBody.GetMoveVectorLine());
#if DEBUG
            Console.Out.WriteLine($"distance to pt{twoDPoint}  {BotBody.GetMoveVectorLine()} is {distance}");
#endif
            return distance < BotLocalConfig.CloseEnoughDistance;
        }

        private TwoDVector? GoPathDirection()
        {
            while (true)
            {
                var firstOrDefault = PathPoints.FirstOrDefault();

                if (firstOrDefault == null) return null;

                if (!CloseEnough(firstOrDefault))
                {
                    return MoveToPt(firstOrDefault);
                }
#if DEBUG
                Console.Out.WriteLine("close enough pts rv ");
#endif
                PathPoints.RemoveAt(0);
            }
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
            if (characterStatusNowWeapon != longestWeapon.weaponIndex) return (false, true);
            var enumerable = valueTuples.Where(a => a.range >= distance).ToArray();
            if (!enumerable.Any())
            {
                return (false, false);
            }

            var okWeapon = enumerable.First();
            return characterStatusNowWeapon == okWeapon.weaponIndex ? (true, false) : (false, true);
        }

        public TwoDPoint GetStartPt()
        {
            var twoDPoint = PatrolCtrl.GetNowPt();
            return twoDPoint;
        }
    }
}