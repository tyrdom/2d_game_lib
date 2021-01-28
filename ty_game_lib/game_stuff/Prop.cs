using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class Prop : ICharAct, ISaleStuff, ICanDrop, ICanPutInMapInteractable
    {
        public static Prop GenById(int id)
        {
            if (CommonConfig.Configs.props.TryGetValue(id, out var prop)
            )
            {
                return new Prop(prop);
            }

            throw new DirectoryNotFoundException($"not such id {id}");
        }

        private Prop(prop prop)
        {
            var dictionary = prop.LaunchTimeToEffectM.ToDictionary(p => CommonConfig.GetTickByTime(p.Key),
                p => LocalConfig.GenMedia(p.Value));
            PropBullets = dictionary.ToImmutableDictionary();
            RecyclePropStack = LocalConfig.StandardPropRecycleStack;
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
                : new TwoDVector(CommonConfig.NumPerSecToTickPerSec(firstOrDefault.x),
                    CommonConfig.NumPerSecToTickPerSec(firstOrDefault.y));
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

        public IMapInteractable? InWhichMapInteractive { get; set; }


        public IMapInteractable DropAsIMapInteractable(TwoDPoint pos)
        {
            return CanPutInMapInteractableStandard.GenIMapInteractable(pos, this);
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

        public (bool canUse, float? CondParam) BotUse(bot_use_cond enemyOnSight, int characterStatusNowPropPoint)
        {
            return enemyOnSight == BotUseCond && characterStatusNowPropPoint >= PropPointCost
                ? (true, CondParam)
                : (false, null);
        }
    }
}