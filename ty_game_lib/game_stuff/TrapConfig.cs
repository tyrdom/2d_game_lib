using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class TrapConfig
    {
        public TrapConfig(bool canBeSee, int? failChance, BodySize bodySize, uint callTrapTick,
            uint? maxLifeTimeTick, IHitMedia trapMedia, uint trickDelayTick, int? trickStack, IHitMedia? launchMedia,
            float? survivalStatusMulti)
        {
            CanBeSee = canBeSee;
            FailChance = failChance;
            BodySize = bodySize;
            CallTrapTick = callTrapTick;
            MaxLifeTimeTick = maxLifeTimeTick;
            TrapMedia = trapMedia;
            TrickDelayTick = trickDelayTick;
            TrickStack = trickStack;
            LaunchMedia = launchMedia;
        }

        public Trap GenATrap(CharacterStatus characterStatus, TwoDPoint pos)
        {
            var id = characterStatus.GetId() * TempConfig.UpMaxTrap + characterStatus.Traps.Count;

            var genStatusByAttr = BaseAttrId != null
                ? GameTools.GenStatusByAttr(GameTools.GenBaseAttrById(BaseAttrId.Value)).baseSurvivalStatus
                : null;
            var trap = new Trap(characterStatus, genStatusByAttr, CanBeSee, pos, id, BodySize, CallTrapTick,
                MaxLifeTimeTick, 0,
                TrapMedia, TrickDelayTick, 0, TrickStack, LaunchMedia, FailChance);
            return trap;
        }

        private base_attr_id? BaseAttrId { get; }


        private bool CanBeSee { get; }

        private int? FailChance { get; }
        private BodySize BodySize { get; }

        public uint CallTrapTick { get; }

        public uint? MaxLifeTimeTick { get; }


        public IHitMedia TrapMedia { get; }

        public uint TrickDelayTick { get; }


        public int? TrickStack { get; }

        public IHitMedia? LaunchMedia { get; }
    }
}