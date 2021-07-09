using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class TeleportUnit : IApplyUnit
    {
        public TeleportUnit(PlayGround toPlayGround,
            TwoDPoint toPos, size[] allowSizes)
        {
            InWhichMapInteractive = null;
            FromPlayGround = null;
            ToPlayGround = toPlayGround;
            ToPos = toPos;
            AllowSizes = allowSizes;
        }

        public void PutInPlayGround(PlayGround fromPlayGround)
        {
            FromPlayGround = fromPlayGround;
        }

        public IMapInteractable PutInteractable(TwoDPoint pos, bool isActive)
        {
            return new ApplyDevice(this, pos, isActive);
        }

        public IMapInteractable? InWhichMapInteractive { get; set; }

        public PlayGround? FromPlayGround { get; set; }
        public PlayGround ToPlayGround { get; }


        public TwoDPoint ToPos { get; }

        public size[] AllowSizes { get; }

        public TwoDPoint? GetThisPos()
        {
            return InWhichMapInteractive?.GetAnchor();
        }

        public bool CanInterActOneBy(CharacterStatus characterStatus)
        {
            return CanInterActTwoBy(characterStatus);
        }

        public bool CanInterActTwoBy(CharacterStatus characterStatus)
        {
            var bodySize = characterStatus.CharacterBody.GetSize();
            var contains = AllowSizes.Contains(bodySize);
            return contains;
        }

        public ImmutableArray<IActResult> ActWhichChar(CharacterStatus characterStatus, MapInteract interactive)
        {
            return interactive switch
            {
                MapInteract.GetInfoCall => new IActResult[]
                    {new TelePortMsg(ToPlayGround.MgId, ToPos)}.ToImmutableArray(),
                MapInteract.BuyOrApplyCall => new IActResult[]
                    {new TelePortMsg(ToPlayGround.MgId, ToPos)}.ToImmutableArray(),
                _ => ImmutableArray<IActResult>.Empty
            };
        }

        public float GetMaxR()
        {
            var max = AllowSizes.Max(x => CommonConfig.Configs.bodys.TryGetValue(x, out var b) ? b.rad : 0);
            return max;
        }
    }
}