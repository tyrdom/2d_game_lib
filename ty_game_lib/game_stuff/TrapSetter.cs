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
            int? baseAttrId)
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
            return CommonConfig.Configs.traps.TryGetValue(id, out var trap)
                ? new TrapSetter(trap)
                : throw new KeyNotFoundException($"not such id {id}");
        }

        private TrapSetter(trap trap)
        {
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

        public Trap GenATrap(CharacterStatus characterStatus, TwoDPoint pos ,int mapInstanceId)
        {
            var id = characterStatus.GetId() * CommonConfig.OtherConfig.up_trap_max + characterStatus.Traps.Count;

            if (BaseAttrId == null)
                return new Trap(characterStatus, null, CanBeSee, pos, id, BodySize,
                    CallTrapTick,
                    MaxLifeTimeTick, 0,
                    TrapMedia, TrickDelayTick, 0, TrickStack, LaunchMedia, FailChanceStack,
                    characterStatus.TrapAtkMulti,mapInstanceId);
            var (baseSurvivalStatus, _) = GameTools.GenStatusByAttr(GameTools.GenBaseAttrById(BaseAttrId.Value));
            var trap = new Trap(characterStatus, baseSurvivalStatus, CanBeSee, pos, id, BodySize,
                CallTrapTick,
                MaxLifeTimeTick, 0,
                TrapMedia, TrickDelayTick, 0, TrickStack, LaunchMedia, FailChanceStack,
                characterStatus.TrapAtkMulti,mapInstanceId);
            return trap;
        }

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