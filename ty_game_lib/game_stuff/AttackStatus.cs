using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using game_config;

namespace game_stuff
{
    public class AttackStatus
    {
        private float BackStabAdd { get; set; }
        private uint MainAttack { get; set; }
        private uint ShardedAttack { get; }
        private uint ShardedNum { get; set; }

        public Damage GenDamage(float damageMulti, bool isBackStab)
        {
            var dm = damageMulti;
            if (isBackStab)
            {
                dm *= 1 + BackStabAdd;
            }

            var mainDamage = (uint) (MainAttack * dm);
            var shardedDamage = (uint) (ShardedAttack * dm);
            var damage = new Damage(ShardedNum, mainDamage, shardedDamage);
            return damage;
        }


        private AttackStatus(uint baseAttack, uint bsn, float backStabAdd)
        {
            ShardedAttack = (uint) (baseAttack * TempConfig.ShardedAttackMulti);
            ShardedNum = bsn;
            BackStabAdd = backStabAdd;
            MainAttack = baseAttack;
        }

        public static AttackStatus StandardAttackStatus()
        {
            return new AttackStatus(100, 0, 0.5f);
        }


        public static AttackStatus GenByConfig(base_attribute baseAttribute)
        {
            var baseAttributeAtk = baseAttribute.Atk;
            var baseAttributeShardedNum = baseAttribute.ShardedNum;
            var baseAttributeBackStabAdd = baseAttribute.BackStabAdd;

            return new AttackStatus(baseAttributeAtk, baseAttributeShardedNum, baseAttributeBackStabAdd);
        }

        public void PassiveEffectChangeAtk(Vector<float> passiveTrait,
            AttackStatus baseAtkStatus)
        {
            MainAttack = (uint) (baseAtkStatus.MainAttack * (1 + passiveTrait[0]));
            ShardedNum = (uint) (baseAtkStatus.ShardedNum + passiveTrait[1]);
            BackStabAdd += baseAtkStatus.BackStabAdd + passiveTrait[2];
        }
    }
}