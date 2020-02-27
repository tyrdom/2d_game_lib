using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class Bullet
    {
        public TwoDPoint Pos;
        public TwoDVector Aim;
        public Dictionary<BodySize, BulletBox> SizeToBulletCollision;
        public CharacterStatus Master;
        public AntiActBuffConfig SuccessAntiActBuffConfigToOpponent;
        private AntiActBuffConfig FailActBuffConfigToSelf;
        public int PauseToCaster;
        public int PauseToOpponent;
        public DamageBuffConfig[] DamageBuffConfigs;

        public Bullet(TwoDPoint pos, TwoDVector aim, Dictionary<BodySize, BulletBox> sizeToBulletCollision, CharacterStatus master, AntiActBuffConfig successAntiActBuffConfigToOpponent, AntiActBuffConfig failActBuffConfigToSelf, int pauseToCaster, int pauseToOpponent, DamageBuffConfig[] damageBuffConfigs)
        {
            Pos = pos;
            Aim = aim;
            SizeToBulletCollision = sizeToBulletCollision;
            Master = master;
            SuccessAntiActBuffConfigToOpponent = successAntiActBuffConfigToOpponent;
            FailActBuffConfigToSelf = failActBuffConfigToSelf;
            PauseToCaster = pauseToCaster;
            PauseToOpponent = pauseToOpponent;
            DamageBuffConfigs = damageBuffConfigs;
        }
        
        
    }
}