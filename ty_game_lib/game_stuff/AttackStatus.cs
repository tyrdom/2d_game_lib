using System;
using System.Numerics;
using game_config;

namespace game_stuff
{
    public struct AttackStatus
    {
        private float BackStabAdd { get; set; }
        private uint MainAttack { get; set; }
        private uint ShardedAttack { get; }
        private uint ShardedNum { get; set; }

        public Damage GenDamage(float damageMulti, bool isBackStab, float buffMulti = 1f)
        {
            var bk = isBackStab ? 1f + BackStabAdd : 1f;
            var mainDamage = (uint) (MainAttack * damageMulti * bk * buffMulti);
            var shardedAttack = (uint) (ShardedAttack * damageMulti);
            var shardedNum = (uint) (ShardedNum * buffMulti * bk);
            var damage = new Damage(shardedNum, mainDamage, shardedAttack);

#if DEBUG
            
             Console.Out.WriteLine($"{MainAttack} cause {mainDamage} and {ShardedAttack} cause {shardedAttack}~{shardedNum}");
#endif
           
            return damage;
        }


        private AttackStatus(uint baseAttack, uint bsn, float backStabAdd)
        {
            ShardedAttack = (uint) (baseAttack *  CommonConfig.OtherConfig.ShardedAttackMulti);
            ShardedNum = bsn;
            BackStabAdd = backStabAdd;
            MainAttack = baseAttack;
        }


        public static AttackStatus GenByConfig(base_attribute baseAttribute)
        {
            var baseAttributeAtk = baseAttribute.Atk;
            var baseAttributeShardedNum = baseAttribute.ShardedNum;
            var baseAttributeBackStabAdd = baseAttribute.BackStabAdd;

            return new AttackStatus(baseAttributeAtk, baseAttributeShardedNum, baseAttributeBackStabAdd);
        }

        public void PassiveEffectChangeAtk(float[] passiveTrait,
            AttackStatus baseAtkStatus)
        {
            MainAttack = (uint) (baseAtkStatus.MainAttack * (1 + passiveTrait[0]));
            ShardedNum = (uint) (baseAtkStatus.ShardedNum + passiveTrait[1]);
            BackStabAdd += baseAtkStatus.BackStabAdd + passiveTrait[2];
        }
    }
}