using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class TrapSetter
    {
        public TrapSetter(bool canBeSee, int? failChanceStack, size bodySize, uint callTrapTick,
            uint? maxLifeTimeTick, IHitMedia trapMedia, uint trickDelayTick, int? trickStack, IHitMedia? launchMedia,
            int? baseAttrId, trap_id trapId)
        {
            CanBeSee = canBeSee;
            FailChanceStack = failChanceStack;
            BodySize = bodySize;
            CallTrapTick = callTrapTick;
            MaxLifeTimeTick = maxLifeTimeTick;
            TrapMedia = trapMedia;
            BaseAttrId = baseAttrId;
            TrapId = trapId;
            LaunchMedia = launchMedia;
            TrickDelayTick = trickDelayTick;
            TrickStack = trickStack;
        }

        public static TrapSetter GenById(string id)
        {
            var trapId = (trap_id) Enum.Parse(typeof(trap_id), id, true);
            return GenById(trapId);
        }

        private static TrapSetter GenById(trap_id id)
        {
            return CommonConfig.Configs.traps.TryGetValue(id, out var trap)
                ? new TrapSetter(trap)
                : throw new KeyNotFoundException($"not such id {id}");
        }

        private TrapSetter(trap trap)
        {
            TrapId = trap.id;
            CanBeSee = trap.CanBeSee;
            FailChanceStack = trap.FailChance == 0 ? (int?) null : trap.FailChance;
            BodySize = trap.BodyId;
            CallTrapTick = (trap.CallTrapRoundTime);
            TrapMedia = StuffLocalConfig.GenHitMedia(trap.TrapMedia);
            BaseAttrId = trap.AttrId;
            MaxLifeTimeTick = trap.MaxLifeTime == 0f ? (uint?) null : (trap.MaxLifeTime);

            var firstOrDefault = trap.LauchMedia.FirstOrDefault();
            if (firstOrDefault == null)
            {
                LaunchMedia = null;
                TrickStack = null;
                TrickDelayTick = 0;
                return;
            }

            var genHitMedia = StuffLocalConfig.GenHitMedia(firstOrDefault);
            LaunchMedia = genHitMedia;
            TrickStack = (int?) trap.TrickStack;
            TrickDelayTick = trap.TrickDelayTime;
        }

        public Trap GenATrap(CharacterStatus characterStatus, TwoDPoint pos, int mapInstanceId)
        {
            if (BaseAttrId == null)
                return new Trap(characterStatus, null, CanBeSee, pos, BodySize,
                    CallTrapTick,
                    MaxLifeTimeTick, 0,
                    TrapMedia, TrickDelayTick, 0, TrickStack, LaunchMedia, FailChanceStack,
                    characterStatus.TrapAtkMulti, mapInstanceId, TrapId);
            var (baseSurvivalStatus, _) = GameTools.GenStatusByAttr(GameTools.GenBaseAttrById(BaseAttrId.Value));
            var trap = new Trap(characterStatus, baseSurvivalStatus, CanBeSee, pos, BodySize,
                CallTrapTick,
                MaxLifeTimeTick, 0,
                TrapMedia, TrickDelayTick, 0, TrickStack, LaunchMedia, FailChanceStack,
                characterStatus.TrapAtkMulti, mapInstanceId, TrapId);
            return trap;
        }

        private trap_id TrapId { get; }
        private int? BaseAttrId { get; }
        private bool CanBeSee { get; }
        private int? FailChanceStack { get; }
        private size BodySize { get; }
        private uint CallTrapTick { get; }
        private uint? MaxLifeTimeTick { get; }
        public IHitMedia TrapMedia { get; }
        private uint TrickDelayTick { get; }
        private int? TrickStack { get; }
        public IHitMedia? LaunchMedia { get; }
    }
}