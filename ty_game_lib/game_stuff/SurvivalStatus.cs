using System;
using System.Collections.Generic;
using System.Linq;
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

        public HashSet<SurvivalChangeMark> SurvivalChangeMarks { get; }

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
            SurvivalChangeMarks = new HashSet<SurvivalChangeMark>();
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
                SurvivalChangeMarks.Add(SurvivalChangeMark.ShieldChange);
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
                SurvivalChangeMarks.Add(SurvivalChangeMark.ArmorChange);
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

            SurvivalChangeMarks.Add(SurvivalChangeMark.HpChange);
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
            if (NowHp >= MaxHp)
            {
                return;
            }

            SurvivalChangeMarks.Add(SurvivalChangeMark.HpChange);

            NowHp = (uint) MathTools.Min(NowHp + regenerationHealMulti * MaxHp * healMulti, MaxHp);
        }

        private void FixArmor(float regenerationFixMulti, float fixMulti)
        {
            if (NowArmor >= MaxArmor)
            {
                return;
            }

            SurvivalChangeMarks.Add(SurvivalChangeMark.ArmorChange);
            NowArmor = (uint) MathTools.Min(NowArmor + regenerationFixMulti * MaxArmor * fixMulti, MaxArmor);
        }

        private void ChargeShield(float regenerationShieldMulti, float chargeMulti)
        {
            SurvivalChangeMarks.Add(SurvivalChangeMark.ShieldChange);
            var maxShield = (uint) (regenerationShieldMulti * MaxShield * chargeMulti);
            NowShield += maxShield;
        }


        public bool GoATickAndCheckAlive()
        {
            if (IsDead()) return false;


            if (NowShield < MaxShield && NowDelayTick == 0)
            {
                SurvivalChangeMarks.Add(SurvivalChangeMark.ShieldChange);
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


        public void SurvivalPassiveEffectChange(float[] v,
            SurvivalStatus baseSurvivalStatus)

        {
            // if (v.Length < 6)
            // {
            //     var aggregate = v.Aggregate("", (s, x) => s + x + "|");
            //     throw new Exception($"no good array : {aggregate}:: {v.Length}");
            // }

            var lossHp = MaxHp - NowHp;
            MaxHp = (uint) MathTools.Max(1, baseSurvivalStatus.MaxHp * (1 + v[0]));

            NowHp = MathTools.Max(1, MaxHp - lossHp);

            var lossAr = MaxArmor - NowArmor;
            MaxArmor = (uint) MathTools.Max(0, baseSurvivalStatus.MaxArmor * (1 + v[1]));
            NowArmor = MathTools.Max(0, MaxArmor - lossAr);
            ArmorDefence = (uint) MathTools.Max(0, baseSurvivalStatus.ArmorDefence * (1 + v[2]));
            MaxShield = (uint) MathTools.Max(0, baseSurvivalStatus.MaxShield * (1 + v[3]));
            ShieldRecover = (uint) (baseSurvivalStatus.ShieldRecover * (1 + v[4]));
            ShieldInstability = (uint) (baseSurvivalStatus.ShieldInstability * (1 + v[5]));
            SurvivalChangeMarks.Add(SurvivalChangeMark.MaxValueChange);
        }

        public float GenShortStatus()
        {
            var nowShield = (NowHp + NowArmor + NowShield) / (float) (MaxArmor + MaxShield + MaxHp);
            return nowShield;
        }

        public void AbsorbDamage(uint total, uint hit, AbsorbStatus absorbStatus, uint damageShardedDamage)
        {
            HpAbs(absorbStatus.HpAbs, total);
            ArmorAbs(absorbStatus.ArmorAbs, total, hit, damageShardedDamage);
            ShieldAbs(absorbStatus.ShieldAbs, total, hit);
        }

        private void ShieldAbs(float absorbStatusShieldAbs, uint damage, uint hit)
        {
            var shieldInstability = damage + hit * ShieldInstability;
            NowShield = (uint) (NowShield + shieldInstability * absorbStatusShieldAbs);
            SurvivalChangeMarks.Add(SurvivalChangeMark.ShieldChange);
        }

        private void ArmorAbs(float absorbStatusArmorAbs, uint damage, uint hit, uint damageShardedDamage)
        {
            var shieldInstability = Math.Max(0, (damage - hit * Math.Min(ArmorDefence, damageShardedDamage)));
            NowArmor = (uint) (NowArmor + shieldInstability * absorbStatusArmorAbs);
            SurvivalChangeMarks.Add(SurvivalChangeMark.ArmorChange);
        }

        private void HpAbs(float absorbStatusHpAbs, uint damage)
        {
            NowHp = (uint) (NowHp + damage * absorbStatusHpAbs);
            SurvivalChangeMarks.Add(SurvivalChangeMark.HpChange);
        }

        public NewSurvivalStatus GenNewMsg()
        {
            return new NewSurvivalStatus(NowHp, NowArmor, NowShield, MaxHp, MaxArmor, MaxShield);
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

        public IEnumerable<ICharEvent> GenSurvivalEvents()
        {
            return SurvivalChangeMarks.Select(x =>
            {
                ICharEvent charEvent = x switch
                {
                    SurvivalChangeMark.ShieldChange => new ShieldChange(NowShield),
                    SurvivalChangeMark.ArmorChange => new ArmorChange(NowArmor),
                    SurvivalChangeMark.HpChange => new HpChange(NowHp),
                    SurvivalChangeMark.MaxValueChange => new SurvivalMaxValueChange(MaxHp, MaxArmor, MaxShield),
                    _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
                };
                return charEvent;
            });
        }
    }

    public class SurvivalMaxValueChange : ICharEvent
    {
        public SurvivalMaxValueChange(uint maxHp, uint maxArmor, uint maxShield)
        {
            MaxHp = maxHp;
            MaxArmor = maxArmor;
            MaxShield = maxShield;
        }

        public uint MaxShield { get; }

        public uint MaxArmor { get; }

        public uint MaxHp { get; }
    }

    public class NewSurvivalStatus : ICharEvent
    {
        public NewSurvivalStatus(uint nowHp, uint nowArmor, uint nowShield, uint maxHp, uint maxArmor, uint maxShield)
        {
            NowHp = nowHp;
            NowArmor = nowArmor;
            NowShield = nowShield;
            MaxHp = maxHp;
            MaxArmor = maxArmor;
            MaxShield = maxShield;
        }

        public uint MaxShield { get; }

        public uint MaxArmor { get; }

        public uint MaxHp { get; }


        public uint NowShield { get; }

        public uint NowArmor { get; }

        public uint NowHp { get; }
    }

    public class HpChange : ICharEvent
    {
        public HpChange(uint nowHp)
        {
            NowHp = nowHp;
        }

        public uint NowHp { get; }
    }

    public class ShieldChange : ICharEvent
    {
        public ShieldChange(uint nowShield)
        {
            NowShield = nowShield;
        }

        public uint NowShield { get; }
    }

    public class ArmorChange : ICharEvent
    {
        public ArmorChange(uint nowArmor)
        {
            NowArmor = nowArmor;
        }

        public uint NowArmor { get; }
    }

    public enum SurvivalChangeMark
    {
        ShieldChange,
        ArmorChange,
        HpChange,
        MaxValueChange
    }
}