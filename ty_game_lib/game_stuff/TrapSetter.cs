using System;
using System.IO;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class TrapSetter
    {
        public TrapSetter(bool canBeSee, int? failChanceStack, BodySize bodySize, uint callTrapTick,
            uint? maxLifeTimeTick, IHitMedia trapMedia, uint trickDelayTick, int? trickStack, IHitMedia? launchMedia,
            base_attr_id? baseAttrId)
        {
            CanBeSee = canBeSee;
            FailChanceStack = failChanceStack;
            BodySize = bodySize;
            CallTrapTick = callTrapTick;
            MaxLifeTimeTick = maxLifeTimeTick;
            TrapMedia = trapMedia;
            BaseAttrId = baseAttrId;

            LaunchMedia = launchMedia;
            TrickDelayTick = trickDelayTick;
            TrickStack = trickStack;
        }

        public static TrapSetter GenById(string id)
        {
            if (LocalConfig.Configs.traps.TryGetValue(id, out var trap))
            {
                return new TrapSetter(trap);
            }

            throw new DirectoryNotFoundException($"not such id {id}");
        }

        private TrapSetter(trap trap)
        {
            CanBeSee = trap.CanBeSee;
            FailChanceStack = trap.FailChance == 0 ? (int?) null : trap.FailChance;
            BodySize = LocalConfig.GetBodySize(trap.BodyId);
            CallTrapTick = CommonConfig.GetTickByTime(trap.CallTrapRoundTime);
            TrapMedia = LocalConfig.GenHitMedia(trap.TrapMedia);
            BaseAttrId = trap.AttrId;
            MaxLifeTimeTick = trap.MaxLifeTime == 0f ? (uint?) null : CommonConfig.GetTickByTime(trap.MaxLifeTime);

            var firstOrDefault = trap.LauchMedia.FirstOrDefault();
            if (firstOrDefault == null)
            {
                LaunchMedia = null;
                TrickStack = null;
                TrickDelayTick = 0;
                return;
            }

            var genHitMedia = LocalConfig.GenHitMedia(firstOrDefault);
            LaunchMedia = genHitMedia;
            TrickStack = (int?) trap.TrickStack;
            TrickDelayTick = CommonConfig.GetTickByTime(trap.TrickDelayTime);
        }

        public Trap GenATrap(CharacterStatus characterStatus, TwoDPoint pos)
        {
            var id = characterStatus.GetId() * LocalConfig.UpMaxTrap + characterStatus.Traps.Count;

            if (BaseAttrId == null)
                return new Trap(characterStatus, null, CanBeSee, pos, id, BodySize,
                    CallTrapTick,
                    MaxLifeTimeTick, 0,
                    TrapMedia, TrickDelayTick, 0, TrickStack, LaunchMedia, FailChanceStack,
                    characterStatus.TrapAtkMulti);
            var (baseSurvivalStatus, _) = GameTools.GenStatusByAttr(GameTools.GenBaseAttrById(BaseAttrId.Value));
            var trap = new Trap(characterStatus, baseSurvivalStatus, CanBeSee, pos, id, BodySize,
                CallTrapTick,
                MaxLifeTimeTick, 0,
                TrapMedia, TrickDelayTick, 0, TrickStack, LaunchMedia, FailChanceStack,
                characterStatus.TrapAtkMulti);
            return trap;
        }

        private base_attr_id? BaseAttrId { get; }
        private bool CanBeSee { get; }
        private int? FailChanceStack { get; }
        private BodySize BodySize { get; }
        private uint CallTrapTick { get; }
        private uint? MaxLifeTimeTick { get; }
        public IHitMedia TrapMedia { get; }
        private uint TrickDelayTick { get; }
        private int? TrickStack { get; }
        public IHitMedia? LaunchMedia { get; }
    }
}