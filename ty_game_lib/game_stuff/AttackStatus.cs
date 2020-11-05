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


        private static AttackStatus GenByConfig(base_attr_id baseAttrId)
        {
            if (!TempConfig.Configs.base_attributes.TryGetValue(baseAttrId, out var baseAttribute))
                throw new ArgumentException($"not such attr{baseAttrId}");
            var baseAttributeAtk = baseAttribute.Atk;
            var baseAttributeShardedNum = baseAttribute.ShardedNum;
            var baseAttributeBackStabAdd = baseAttribute.BackStabAdd;

            return new AttackStatus(baseAttributeAtk, baseAttributeShardedNum, baseAttributeBackStabAdd);
        }

        public void PassiveEffectChangeAtk(IEnumerable<AtkAboutPassiveEffect> passiveTrait,
            base_attr_id baseAttrId)
        {
            var baseAtkStatus = GenByConfig(baseAttrId);
            var atkAboutPassives = passiveTrait.ToList();
            var (main, sn, bs) = atkAboutPassives.Aggregate((0f, 0f, 0f), (s, x) =>
                (s.Item1 + x.MainAtkMultiAddPerLevel, s.Item2 + x.ShardedNumAddPerLevel,
                    s.Item3 + x.BackStabAddPerLevel));

            MainAttack = (uint) (baseAtkStatus.MainAttack * (1 + main));
            ShardedNum = (uint) (baseAtkStatus.ShardedNum + sn);
            BackStabAdd += baseAtkStatus.BackStabAdd + bs;
        }
    }
}