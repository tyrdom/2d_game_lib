using System;
using System.Collections.Generic;
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

        public void PassiveEffectChangeAtk(IEnumerable<AtkAboutPassiveEffect> passiveTrait,
            AttackStatus baseAtkStatus)
        {
            var atkAboutPassives = passiveTrait.ToList();
            var (main, sn, bs) = atkAboutPassives.Aggregate((0f, 0f, 0f), (s, x) =>
                (s.Item1 + x.MainAtkMultiAdd, s.Item2 + x.ShardedNumAdd,
                    s.Item3 + x.BackStabAdd));

            MainAttack = (uint) (baseAtkStatus.MainAttack * (1 + main));
            ShardedNum = (uint) (baseAtkStatus.ShardedNum + sn);
            BackStabAdd += baseAtkStatus.BackStabAdd + bs;
        }
    }
}