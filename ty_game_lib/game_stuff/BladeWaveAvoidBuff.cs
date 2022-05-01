using System;
using game_config;

namespace game_stuff
{
    public class BladeWaveAvoidBuff
    {
        public BladeWaveAvoidBuff(int casterId, bullet_id bulletId, int restTick)
        {
            CasterId = casterId;
            BulletId = bulletId;
            RestTick = restTick;
        }

        private int CasterId { get; }

        private bullet_id BulletId { get; }
        private int RestTick { get; set; }

        public int GenKey()
        {
            var genKey = GenKey(CasterId, BulletId);
            return genKey;
        }

        public static int GenKey(int castId, bullet_id bulletId)
        {
            var length = Enum.GetValues(typeof(bullet_id)).Length;
            var casterId = castId * length + (int)bulletId;
            return casterId;
        }

        public bool IsFinish()
        {
            return RestTick <= 0;
        }

        public void GoATick()
        {
            RestTick--;
        }
    }
}