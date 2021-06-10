using System;
using System.Linq;
using game_config;

namespace game_stuff
{
    public class AttackStatus
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
            Console.Out.WriteLine(
                $"{MainAttack} cause {mainDamage} and {ShardedAttack} cause {shardedAttack}~{shardedNum}");
#endif

            return damage;
        }


        private AttackStatus(uint baseAttack, uint bsn, float backStabAdd)
        {
            ShardedAttack = (uint) (baseAttack * CommonConfig.OtherConfig.ShardedAttackMulti);
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

        public override string ToString()
        {
            return $"MA:{MainAttack} SN:{ShardedNum} BS:{BackStabAdd}";
        }

        public void PassiveEffectChangeAtk(float[] passiveTrait,
            AttackStatus baseAtkStatus)
        {
            if (!passiveTrait.Any())
            {
                return;
            }

            BackStabAdd = baseAtkStatus.BackStabAdd + passiveTrait[0];
            MainAttack = baseAtkStatus.MainAttack + (uint) (passiveTrait[1] * baseAtkStatus.MainAttack);
            ShardedNum = baseAtkStatus.ShardedNum + (uint) passiveTrait[2];
#if DEBUG
            Console.Out.WriteLine($"now Atk attr is {this}");
#endif
        }
    }
}