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
            TrickDelayTick = trickDelayTick;
            TrickStack = trickStack;
            LaunchMedia = launchMedia;
            BaseAttrId = baseAttrId;
        }

        public Trap GenATrap(CharacterStatus characterStatus, TwoDPoint pos)
        {
            var id = characterStatus.GetId() * TempConfig.UpMaxTrap + characterStatus.Traps.Count;

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