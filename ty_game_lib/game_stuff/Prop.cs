using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Prop : ICharAct, ISaleStuff, ICanPickDrop, ICanPutInMapInteractable, IPutInCage
    {
        public static Prop GenById(int id)
        {
            if (CommonConfig.Configs.props.TryGetValue(id, out var prop)
            )
            {
                return new Prop(prop);
            }

            throw new KeyNotFoundException($"not such id {id}");
        }

        private Prop(prop prop)
        {
            var dictionary = prop.LaunchTimeToEffectM.ToDictionary(p => CommonConfig.GetTickByTime(p.Key),
                p => StuffLocalConfig.GenMedia(p.Value));
            PropBullets = dictionary.ToImmutableDictionary();
            RecyclePropStack = CommonConfig.OtherConfig.standard_recycle_prop_stack;
            PropPointCost = prop.PropPointCost;
            MoveMulti = prop.MoveSpeedMulti;
            PId = prop.id;
            TotalTick = CommonConfig.GetTickByTime(prop.PropMustTime);
            LockAim = prop.LockAim;
            BotUseCond = prop.BotUseCondType;
            CondParam = prop.BotUseCondParam.FirstOrDefault();
            var tickByTime = CommonConfig.GetTickByTime(prop.MoveAddStartTime);
            StartAddSpeedTick = tickByTime == 0 ? (uint?) null : tickByTime;
            var firstOrDefault = prop.MoveAdds.FirstOrDefault();
            AddSpeed = firstOrDefault == null
                ? null
                : new TwoDVector(CommonConfig.ValuePerSecToValuePerTick(firstOrDefault.x),
                    CommonConfig.ValuePerSecToValuePerTick(firstOrDefault.y));
        }

        public int PId { get; }
        public int PropPointCost { get; }
        public uint TotalTick { get; }
        private float MoveMulti { get; }
        private ImmutableDictionary<uint, IEffectMedia> PropBullets { get; }

        public int RecyclePropStack { get; }

        //bot使用
        public bot_use_cond BotUseCond { get; }

        public float? CondParam { get; }

        //强制位移类专用
        public bool LockAim { get; }

        private uint? StartAddSpeedTick { get; }

        private TwoDVector? AddSpeed { get; }

        public (ITwoDTwoP? move, IEffectMedia? bullet, bool snipeOff, ICanPutInMapInteractable? getFromCage, MapInteract
            interactive) GoATick(TwoDPoint getPos,
                TwoDVector sightAim,
                TwoDVector? rawMoveVector, TwoDVector? limitV)
        {
            var b = NowOnTick == 0;
            NowOnTick++;
            var bullet = PropBullets.TryGetValue(NowOnTick, out var aBullet) ? aBullet : null;
            var twoDVector = rawMoveVector?.Multi(MoveMulti);

            if (AddSpeed == null || !(NowOnTick > StartAddSpeedTick))
                return (twoDVector, bullet, b, null, MapInteract.PickCall);

            var antiClockwiseTurn = AddSpeed.AntiClockwiseTurn(sightAim);
            if (twoDVector != null)
            {
                antiClockwiseTurn.Add(twoDVector);
            }

            return (antiClockwiseTurn, bullet, b, null, MapInteract.PickCall);
        }

        public int NowTough { get; set; }
        public uint NowOnTick { get; set; }

        public SkillPeriod InWhichPeriod()
        {
            return NowOnTick < TotalTick ? SkillPeriod.Casting : SkillPeriod.End;
        }

        public int? ComboInputRes()
        {
            return null;
        }

        public bool Launch(int nowStack)
        {
            if (nowStack < PropPointCost) return false;
            NowOnTick = 0;
            return true;
        }

        public IMapInteractable PutInteractable(TwoDPoint pos, bool isActive)
        {
            return CanPutInMapInteractableStandard.PutInMapInteractable(pos, this);
        }

        public IMapInteractable? InWhichMapInteractive { get; set; }


        public IMapInteractable DropAsIMapInteractable(TwoDPoint pos)
        {
            return CanPutInMapInteractableStandard.PutInMapInteractable(pos, this);
        }

        public bool CanInterActOneBy(CharacterStatus characterStatus)
        {
            return true;
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            return true;
        }

        public IActResult? ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            switch (interactive)
            {
                case MapInteract.RecycleCall:
                    characterStatus.RecycleAProp(this);
                    return null;
                case MapInteract.PickCall:
                    var pickAProp = characterStatus.PickAProp(this);
                    return pickAProp == null
                        ? (IActResult?) null
                        : new DropThings(new List<IMapInteractable> {pickAProp});
                default:
                    throw new ArgumentOutOfRangeException(nameof(interactive), interactive, null);
            }
        }

        public int GetId()
        {
            return PId;
        }

        public int GetNum()
        {
            return 1;
        }

        public void Sign(CharacterStatus characterStatus)
        {
            foreach (var keyValuePair in PropBullets)
            {
                keyValuePair.Value.Sign(characterStatus);
            }
        }

        public bool BotUseWhenSeeEnemy(CharacterStatus characterStatusNowPropPoint)
        {
            return BotUseCond == bot_use_cond.EnemyOnSight && characterStatusNowPropPoint.NowPropPoint >= PropPointCost
                ;
        }

        public bool CheckAppStatusToBotPropUse(CharacterStatus botBodyCharacterStatus)
        {
            var b = botBodyCharacterStatus.NowPropPoint >= PropPointCost;
            return b && BotUseCond switch
            {
                bot_use_cond.ArmorBlowPercent => botBodyCharacterStatus.SurvivalStatus.ArmorPercent() < CondParam,
                bot_use_cond.ShieldBlowPercent =>
                    botBodyCharacterStatus.SurvivalStatus.ShieldPercent() < CondParam,
                bot_use_cond.HpBlowPercent => botBodyCharacterStatus.SurvivalStatus.HpPercent() < CondParam,
                _ => false
            };
        }

        public bool CanUseWhenPatrol(CharacterStatus botBodyCharacterStatus)
        {
            return BotUseCond == bot_use_cond.OnPatrolRandom && botBodyCharacterStatus.NowPropPoint >= PropPointCost;
        }
    }
}