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
        public CharacterBody BotBody { get; }


        public PatrolCtrl PatrolCtrl { get; }

        public FirstSkillCtrl FirstSkillCtrl { get; }

        public ComboCtrl ComboCtrl { get; }
        private BotStatus BotStatus { get; set; }
        public List<(float range, int maxAmmoUse, int weaponIndex)> RangeToWeapon { get; set; }

        private int? MyPoly { get; set; }
        public int NowWeapon { get; set; }
        private List<TwoDPoint> PathPoints { get; }
        private Random Random { get; }

        private TwoDPoint? TargetRecordPos { get; set; }

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

        public SimpleBot(CharacterBody botBody, Random random, List<TwoDPoint> patrolPts, FirstSkillCtrl firstSkillCtrl,
            int comboMax)
        {
            var valueTuples = GetRangeAmmoWeapon(botBody.CharacterStatus.GetWeapons(), botBody.GetSize());

            BotBody = botBody;
            Random = random;
            FirstSkillCtrl = firstSkillCtrl;
            ComboCtrl = new ComboCtrl(comboMax);
            BotStatus = BotStatus.OnPatrol;
            PatrolCtrl = new PatrolCtrl(patrolPts);
            PathPoints = new List<TwoDPoint>();
            RangeToWeapon = valueTuples;
            NowWeapon = 0;
            MyPoly = null;
            TargetRecordPos = null;
        }


        private Operate? SeeATargetAction(List<TwoDPoint> canBeHitPts)
        {
            var twoDPoint = NearPt(canBeHitPts);
            TargetRecordPos = twoDPoint;
            BotStatus = BotStatus.TargetApproach;
            var botBodyCharacterStatus = BotBody.CharacterStatus;
            var valueTuple = botBodyCharacterStatus.Prop?.BotUse(bot_use_cond.EnemyOnSight,
                botBodyCharacterStatus.NowPropPoint);
            if (valueTuple.HasValue && valueTuple.Value.canUse)
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
            return new TwoDVector(BotBody.GetAnchor(), pt).GetUnit2();
        }

        public BotOpAndThink BotSimpleTick(IEnumerable<ICanBeEnemy> perceivable, PathTop pathTop)
        {
            var canBeEnemies = perceivable.ToList();
            var canBeHitPts = canBeEnemies.OfType<ICanBeHit>()
                .Where(x => x.GetTeam() != BotBody.Team)
                .Select(x => x.GetAnchor()).ToList();
            var radarSees = canBeEnemies.OfType<RadarSee>().Select(x => x.GetAnchor()).ToList();
            switch (BotStatus)
            {
                case BotStatus.OnPatrol:
                    if (canBeHitPts.Any())
                    {
                        return new BotOpAndThink(SeeATargetAction(canBeHitPts));
                    }

                    if (radarSees.Any())
                    {
                        var twoDPoint = NearPt(radarSees);
                        var twoDPoints = pathTop.FindGoPts(BotBody.GetAnchor(), twoDPoint);
                        PathPoints.Clear();
                        PathPoints.AddRange(twoDPoints);
                        BotStatus = BotStatus.GoMaybe;
                        return new BotOpAndThink();
                    }

                    var doPatrol = DoPatrol();
                    return new BotOpAndThink(doPatrol);
                case BotStatus.GoMaybe:
                    if (canBeHitPts.Any())
                    {
                        return new BotOpAndThink(SeeATargetAction(canBeHitPts));
                    }

                    var goPathDirection = GoPathDirection();
                    if (goPathDirection == null)
                    {
                        ReturnOnPatrol(pathTop);
                    }

                    return new BotOpAndThink();
                case BotStatus.TargetApproach:

                    if (canBeHitPts.Any())
                    {
                        var goATick = FirstSkillCtrl.GoATick(Random);
                        var twoDPoint = NearPt(canBeHitPts);
                        TargetRecordPos = twoDPoint;
                        var distance = twoDPoint.GetDistance(BotBody.GetAnchor());
                        var (inRange, needSwitch) = CheckWeaponAndAmmo(distance);
                        if (needSwitch)
                        {
                            return new BotOpAndThink(new Operate(skillAction: SkillAction.Switch), goATick);
                        }

                        if (inRange)
                        {
                            var skillAction = FirstSkillCtrl.GetAction(Random);
                            BotStatus = BotStatus.EngageAct;
                            return new BotOpAndThink(new Operate(skillAction: skillAction), goATick);
                        }

                        var operate = new Operate(move: MoveToPt(twoDPoint));
                        return new BotOpAndThink(operate, goATick);
                    }

                    if (TargetRecordPos != null)
                    {
                        var closeEnough = CloseEnough(TargetRecordPos);
                        if (closeEnough)
                        {
                            TargetRecordPos = null;
                            ReturnOnPatrol(pathTop);
                            return new BotOpAndThink();
                        }

                        var twoDVector = MoveToPt(TargetRecordPos);
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
                            ComboCtrl.ComboOn = true;
                        }

                        return new BotOpAndThink();
                    }

                    if (CheckStun())
                    {
                        ComboCtrl.ComboOn = false;
                        return new BotOpAndThink();
                    }

                    if (ComboCtrl.ComboOn)
                    {
                        var comboAction = FirstSkillCtrl.GetComboAction(Random);
                        return new BotOpAndThink(new Operate(skillAction: comboAction));
                    }

                    BotStatus = BotStatus.TargetApproach;
                    return new BotOpAndThink();
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            if (characterStatusNowVehicle == null || !characterStatusNowVehicle.IsDestroyOn) return null;
            var rangeAmmoWeapon = GetRangeAmmoWeapon(BotBody.CharacterStatus.Weapons, BotBody.BodySize);
            RangeToWeapon = rangeAmmoWeapon;
            return SpecialAction.OutVehicle;
        }

        private Operate DoPatrol()
        {
            if (PathPoints.Any())
            {
                var goPathDirection = GoPathDirection()?.Multi(0.5f);
                return new Operate(move: goPathDirection);
            }

            var next = Random.Next(PatrolCtrl.GetPtNum());
            var twoDPoints = PatrolCtrl.NextPt(next);
            PathPoints.AddRange(twoDPoints);
            return new Operate();
        }

        private bool CloseEnough(TwoDPoint twoDPoint)
        {
            return twoDPoint.GetDistance(BotBody.GetMoveVectorLine()) < 0.3f;
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
    }
}