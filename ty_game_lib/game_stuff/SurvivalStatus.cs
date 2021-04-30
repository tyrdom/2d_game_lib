using System;
using System.Numerics;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class SurvivalStatus
    {
        public uint MaxHp { get; set; }
        public uint NowHp { get; private set; }

        public void SetHp(int hp)
        {
            NowHp = (uint) hp;
        }

        public void SetArmor(int armor)
        {
            NowArmor = (uint) armor;
        }

        public uint NowArmor { get; private set; }
        public uint MaxArmor { get; set; }

        public uint ArmorDefence { get; set; }
        public uint NowShield { get; private set; }
        public uint MaxShield { get; set; }


        private uint NowDelayTick { get; set; }

        private uint ShieldDelayTick { get; }
        private uint ShieldInstability { get; set; }
        private uint ShieldRecover { get; set; }


        private SurvivalStatus(uint maxHp, uint nowHp, uint nowArmor, uint maxArmor, uint nowShield, uint maxShield,
            uint shieldDelayTick, uint armorDefence, uint shieldRecover, uint shieldInstability)
        {
            MaxHp = maxHp;
            NowHp = nowHp;
            NowArmor = nowArmor;
            MaxArmor = maxArmor;
            NowShield = nowShield;
            MaxShield = maxShield;
            ShieldDelayTick = shieldDelayTick;
            ArmorDefence = armorDefence;
            ShieldRecover = shieldRecover;
            ShieldInstability = shieldInstability;
            NowDelayTick = 0;
        }

        public static SurvivalStatus StartTestSurvivalStatus()
        {
            return new SurvivalStatus(1000, 1000, 0, 0, 0, 0, 5, 1, 0, 0);
        }

        public override string ToString()
        {
            return $"HP: {NowHp}/{MaxHp} AM: {NowArmor}/{MaxArmor} SD: {NowShield}/{MaxShield}r{NowDelayTick}";
        }

        public bool IsDead()
        {
            return NowHp <= 0;
        }

        public void TakeDamage(Damage damage)
        {
            NowDelayTick = ShieldDelayTick;
            TakeOneDamage(damage.MainDamage);
#if DEBUG
            Console.Out.WriteLine($"n shield {NowShield}  delay :{NowDelayTick} {GetHashCode()}");
#endif
            if (damage.ShardedNum > 0)
            {
                TakeMultiDamage(damage.ShardedDamage, damage.ShardedNum);
            }
        }

        private void TakeMultiDamage(uint damage, uint times)
        {
            var restTime = (int) times;
            if (NowShield > 0)
            {
                var shieldInstability = damage + ShieldInstability;
                if (shieldInstability <= 0)
                {
                    return;
                }

                var nowShield = (int) NowShield - (int) (shieldInstability * restTime);
                if (nowShield >= 0)
                {
                    NowShield = (uint) nowShield;
                    return;
                }


                var lossTimes = (int) (NowShield / shieldInstability);
                NowShield = 0;
                restTime -= lossTimes;
            }

            if (NowArmor > 0)
            {
                var defence = (int) (damage - ArmorDefence);
                if (defence <= 0)
                {
                    return;
                }

                var nowArmor = (int) NowArmor - defence * restTime;

                if (nowArmor >= 0)
                {
                    NowArmor = (uint) nowArmor;
                    return;
                }

                var armorDefence = (int) NowArmor / defence;
                NowArmor = 0;
                restTime -= armorDefence;
            }

            if (restTime * damage >= NowHp)
            {
                NowHp = 0;
                return;
            }

            NowHp -= damage * (uint) restTime;
        }


        private void TakeOneDamage(uint damage)
        {
            var rest = (int) damage;

            if (NowShield > 0)
            {
                var nowShield = (int) NowShield - (int) ShieldInstability - rest;

                if (nowShield > 0)
                {
                    NowShield = (uint) nowShield;


                    return;
                }

                NowShield = 0;
                rest = -nowShield;
            }

            if (NowArmor > 0)
            {
                var nowArmor = (int) NowArmor - (int) MathTools.Max(0, rest - (int) ArmorDefence);

                if (nowArmor > 0)
                {
                    NowArmor = (uint) nowArmor;
                    return;
                }

                {
                    NowArmor = 0;
                    rest = -nowArmor;
                }
            }

            if ((uint) rest >= NowHp)
            {
                NowHp = 0;
                return;
            }

            NowHp -= (uint) rest;
        }

        public void GetRegen(Regeneration regeneration, RegenEffectStatus regenEffectStatus)
        {
            GetHeal(regeneration.HealMulti, regenEffectStatus.HealEffect);
            FixArmor(regeneration.FixMulti, regenEffectStatus.FixEffect);
            ChargeShield(regeneration.ShieldMulti, regenEffectStatus.ChargeEffect);
        }

        private void GetHeal(float regenerationHealMulti, float healMulti)
        {
            NowHp = (uint) MathTools.Min(NowHp + regenerationHealMulti * MaxHp * healMulti, MaxHp);
        }

        private void FixArmor(float regenerationFixMulti, float fixMulti)
        {
            NowArmor = (uint) MathTools.Min(NowArmor + regenerationFixMulti * MaxArmor * fixMulti, MaxArmor);
        }

        private void ChargeShield(float regenerationShieldMulti, float chargeMulti)
        {
            var maxShield = (uint) (regenerationShieldMulti * MaxShield * chargeMulti);
            NowShield += maxShield;
        }


        public bool GoATickAndCheckAlive()
        {
            if (IsDead()) return false;


            if (NowShield < MaxShield && NowDelayTick == 0)
            {
                NowShield = Math.Min(NowShield + ShieldRecover, MaxShield);
            }

            else if (NowDelayTick > 0)
            {
                NowDelayTick--;
            }
#if DEBUG
            // Console.Out.WriteLine($"now shield {NowShield} d {NowDelayTick} this {this.GetHashCode()}");
#endif
            return true;
        }


        public static SurvivalStatus GenByConfig(base_attribute baseAttribute, float multi = 1f)
        {
            var baseAttributeMaxHp = (uint) (baseAttribute.MaxHP * multi);
            var baseAttributeMaxArmor = (uint) (baseAttribute.MaxArmor * multi);
            var baseAttributeArmorDefence = baseAttribute.ArmorDefence;
            var baseAttributeMaxShield = (uint) (baseAttribute.MaxShield * multi);
            var baseAttributeShieldRecover = (uint) Math.Round(baseAttribute.ShieldRecover);
            var baseAttributeShieldInstability = baseAttribute.ShieldInstability;
            var baseAttributeShieldDelayTime = baseAttribute.ShieldDelayTime;

            return new SurvivalStatus(baseAttributeMaxHp, baseAttributeMaxHp, baseAttributeMaxArmor,
                baseAttributeMaxArmor, baseAttributeMaxShield, baseAttributeMaxShield,
                baseAttributeShieldDelayTime, baseAttributeArmorDefence, baseAttributeShieldRecover,
                baseAttributeShieldInstability);
        }


        public void SurvivalPassiveEffectChange(Vector<float> v,
            SurvivalStatus baseSurvivalStatus)

        {
            var lossHp = MaxHp - NowHp;
            MaxHp = (uint) (baseSurvivalStatus.MaxHp * (1 + v[0]));

            NowHp = MaxHp <= lossHp ? 1 : MaxHp - lossHp;

            var lossAr = MaxArmor - NowArmor;
            MaxArmor = (uint) (baseSurvivalStatus.MaxArmor * (1 + v[1]));
            NowArmor = MaxArmor < lossAr ? 0 : MaxArmor - lossAr;
            ArmorDefence = (uint) (baseSurvivalStatus.ArmorDefence * (1 + v[2]));
            MaxShield = (uint) (baseSurvivalStatus.MaxShield * (1 + v[3]));
            ShieldRecover = (uint) (baseSurvivalStatus.ShieldRecover * (1 + v[4]));
            ShieldInstability = (uint) (baseSurvivalStatus.ShieldInstability * (1 + v[5]));
        }

        public float GenShortStatus()
        {
            var nowShield = (NowHp + NowArmor + NowShield) / (float) (MaxArmor + MaxShield + MaxHp);
            return nowShield;
        }

        public void AbsorbDamage(uint damage, uint hit, AbsorbStatus absorbStatus, uint damageShardedDamage)
        {
            HpAbs(absorbStatus.HpAbs, damage);
            ArmorAbs(absorbStatus.ArmorAbs, damage, hit, damageShardedDamage);
            ShieldAbs(absorbStatus.ShieldAbs, damage, hit);
        }

        private void ShieldAbs(float absorbStatusShieldAbs, uint damage, uint hit)
        {
            var shieldInstability = damage + hit * ShieldInstability;
            NowShield = (uint) (NowShield + shieldInstability * absorbStatusShieldAbs);
        }

        private void ArmorAbs(float absorbStatusArmorAbs, uint damage, uint hit, uint damageShardedDamage)
        {
            var shieldInstability = damage - hit * Math.Min(ArmorDefence, damageShardedDamage);
            NowShield = (uint) (NowShield + shieldInstability * absorbStatusArmorAbs);
        }

        private void HpAbs(float absorbStatusHpAbs, uint damage)
        {
            NowHp = (uint) (NowShield + damage * absorbStatusHpAbs);
        }

        public void Full()
        {
            NowArmor = MaxArmor;
            NowHp = MaxHp;
        }

        public float ArmorPercent()
        {
            return (float) NowArmor / MaxArmor;
        }

        public float HpPercent()
        {
            return (float) NowHp / MaxHp;
        }

        public float ShieldPercent()
        {
            return (float) NowShield / MaxShield;
        }
    }
}