using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class TrapSetter
    {
        public TrapSetter(bool canBeSee, int? failChance, BodySize bodySize, uint callTrapTick,
            uint? maxLifeTimeTick, IHitMedia trapMedia, uint trickDelayTick, int? trickStack, IHitMedia? launchMedia,
            base_attr_id? baseAttrId)
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
            BaseAttrId = baseAttrId;
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
        private uint CallTrapTick { get; }
        private uint? MaxLifeTimeTick { get; }
        public IHitMedia TrapMedia { get; }
        private uint TrickDelayTick { get; }
        private int? TrickStack { get; }
        public IHitMedia? LaunchMedia { get; }
    }
}