using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class Bullet
    {
        public TwoDPoint Anchor;
        public TwoDVector Aim;
        public Dictionary<BodySize, AabbBoxShape> SizeToBulletCollision;
        public CharacterStatus Master;
        public AntiActBuffConfig SuccessAntiActBuffConfigToOpponent;
        
        public DamageBuffConfig[] DamageBuffConfigs;
        
    }
}