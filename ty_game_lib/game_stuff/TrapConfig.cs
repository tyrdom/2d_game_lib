namespace game_stuff
{
    public class TrapConfig
    {
        public TrapConfig(bool canBeSee, int? failChance, BodySize bodySize, uint callTrapTick,
            uint? maxLifeTimeTick, IHitMedia trapMedia, uint trickDelayTick, int? trickStack, IHitMedia? launchMedia,
            float? survivalStatusMulti)
        {
            Owner = null;
            CanBeSee = canBeSee;
            FailChance = failChance;
            BodySize = bodySize;
            CallTrapTick = callTrapTick;
            MaxLifeTimeTick = maxLifeTimeTick;
            TrapMedia = trapMedia;
            TrickDelayTick = trickDelayTick;
            TrickStack = trickStack;
            LaunchMedia = launchMedia;
            SurvivalStatusMulti = survivalStatusMulti;
        }

        public void PickBySomeOne(CharacterStatus characterStatus)
        {
            Owner = characterStatus;
        }

        private CharacterStatus? Owner { get; set; }


        private float? SurvivalStatusMulti { get; }

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