using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class BulletConfig
    {
        //     private TwoDVector LocalAnchor;

        public Dictionary<BodySize, BulletBox> RawBulletBoxes;
        private AntiActBuffConfig SuccessActBuffConfigToOpponent;

        private AntiActBuffConfig FailActBuffConfigToSelf;
        private DamageBuffConfig[] DamageBuffConfigs;
        private int PauseToCaster;
        private int PauseToOpponent;


        public Bullet GenBullet(TwoDPoint casterPos, TwoDVector casterAim, CharacterStatus caster)
        {
            // var twoDPoint = casterPos.Move(LocalAnchor.ClockwiseTurn(casterAim));

            var dictionary = RawBulletBoxes.ToDictionary(pair => pair.Key,
                pair => pair.Value.GenBulletboxByRawBox(casterPos, casterAim));


            return new Bullet(casterPos, casterAim, dictionary, caster,
                SuccessActBuffConfigToOpponent,
                FailActBuffConfigToSelf, PauseToCaster, PauseToOpponent, DamageBuffConfigs);
        }
    }
}