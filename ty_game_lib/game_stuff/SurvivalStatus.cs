using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class SurvivalStatus
    {
        public uint MaxHp { get; private set; }
        public uint NowHp { get; private set; }

        public void SetHp(int hp)
        {
            NowHp = (uint)hp;
        }

        public void SetArmor(int armor)
        {
            NowArmor = (uint)armor;
        }

        public uint NowArmor { get; private set; }
        public uint MaxArmor { get; set; }

        public uint ArmorDefence { get; set; }
        public uint NowShield { get; private set; }
        public uint MaxShield { get; set; }


        private uint NowDelayTick { get; set; }

        private uint ShieldDelayTick { get; set; }
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
            return $"生命: {NowHp}/{MaxHp} 装甲: {NowArmor}/{MaxArmor} 护盾: {NowShield}/{MaxShield}延迟{NowDelayTick}";
        }

        public string GetDetails()
        {
            return $"装甲防御:{ArmorDefence} 护盾不稳定:{ShieldInstability} 护盾恢复:{ShieldRecover}";
        }

        public void SurvivalValueDetail(out bool shieldExist, out bool armorExist, out float hpLoss,
            out float armorLoss, out bool hpFull, out bool armorFull, out bool shieldFull, out float extraShield)
        {
            shieldExist = NowShield > 0;
            armorExist = NowArmor > 0;
            hpLoss = NowHp == 1 ? 1f : MathTools.Max(0, 1f - HpPercent());
            armorLoss = MathTools.Max(0, 1 - ArmorPercent());
            hpFull
                = NowHp >= MaxHp;
            armorFull = NowArmor >= MaxArmor;
            shieldFull = NowShield >= MaxShield;
            extraShield = shieldFull && MaxShield > 0
                ? (float)NowShield / MaxShield
                  - 1f
                : 0;
        }

        public bool IsDead()
        {
            return NowHp <= 0;
        }

        public int[] TakeDamage(Damage damage, out bool shieldBreak, out bool armorBreak)
        {
            var ints = new[]
                {
                    -1, -1
                }
                ;
            NowDelayTick = ShieldDelayTick;

            var takeOneDamage = TakeOneDamage(damage.MainDamage, out shieldBreak, out armorBreak);
            ints[0] = takeOneDamage;
#if DEBUG
            // Console.Out.WriteLine($"n shield {NowShield}  delay :{NowDelayTick} {GetHashCode()}");
#endif
            if (damage.ShardedNum <= 0)
            {
                return ints;
            }

            var takeMultiDamage = TakeMultiDamage(damage.ShardedDamage, damage.ShardedNum, out var shieldBreak2,
                out var armorBreak2);
            ints[1] = takeMultiDamage;
            shieldBreak = shieldBreak2 || shieldBreak;
            armorBreak = armorBreak2 || armorBreak;
            return ints;
        }

        private int TakeMultiDamage(uint damage, uint times, out bool shieldBreak, out bool armorBreak)
        {
            var restTime = (int)times;
            shieldBreak = false;
            armorBreak = false;
            var harm = 0;
            if (NowShield > 0)
            {
                var shieldInstability = damage + ShieldInstability;

                var i = (int)shieldInstability;
                var instability = i * restTime;
                var nowShield = (int)NowShield - instability;
                if (nowShield >= 0)
                {
                    NowShield = (uint)nowShield;
                    return instability;
                }

                shieldBreak = true;
                harm += (int)NowShield;
                var lossTimes = (int)(NowShield / shieldInstability);
                NowShield = 0;
                restTime -= lossTimes;
            }


            if (NowArmor > 0)
            {
                var defence = (int)damage - (int)ArmorDefence;
                if (defence <= 0)
                {
                    return harm;
                }

                var intPtr = defence * restTime;
                var nowArmor = (int)NowArmor - intPtr;

                if (nowArmor >= 0)
                {
                    NowArmor = (uint)nowArmor;
                    harm += intPtr;
                    return harm;
                }

                armorBreak = true;
                var armorDefence = (int)NowArmor / defence;
                harm += (int)NowArmor;
                NowArmor = 0;
                restTime -= armorDefence;
            }


            var time = restTime * (int)damage;
            harm += time;
            if (time >= NowHp)
            {
                NowHp = 0;
                return harm;
            }

            var nowHp = damage * (uint)restTime;
            NowHp -= nowHp;
            return harm;
        }


        private int TakeOneDamage(uint damage, out bool shieldBreak, out bool armorBreak)
        {
            var rest = (int)damage;
            shieldBreak = false;
            armorBreak = false;
            var harm = 0;
            if (NowShield > 0)
            {
                var shieldInstability = (int)ShieldInstability + rest;
                var nowShield = (int)NowShield - shieldInstability;
                SurvivalChangeMarks.Add(SurvivalChangeMark.ShieldChange);
                if (nowShield > 0)
                {
                    NowShield = (uint)nowShield;

                    return shieldInstability;
                }

                harm += (int)NowShield;
                shieldBreak = true;
                NowShield = 0;
                rest = -nowShield;
            }


            if (NowArmor > 0)
            {
                var max = (int)MathTools.Max(0, rest - ArmorDefence);
                var nowArmor = (int)NowArmor - max;
                SurvivalChangeMarks.Add(SurvivalChangeMark.ArmorChange);
                if (nowArmor > 0)
                {
                    NowArmor = (uint)nowArmor;
                    return harm + max;
                }

                harm += (int)NowArmor;
                armorBreak = true;
                NowArmor = 0;
                rest = -nowArmor;
            }

            SurvivalChangeMarks.Add(SurvivalChangeMark.HpChange);
            if (rest >= NowHp)
            {
                NowHp = 0;
                return harm + rest;
            }

            NowHp -= (uint)rest;
            return harm + rest;
        }

        public void GetRegen(Regeneration regeneration, RegenEffectStatus regenEffectStatus, out int heal,
            out int fixArmor, out int chargeShield)
        {
            heal = GetHeal(regeneration.HealMulti, regenEffectStatus.HealEffect);
            fixArmor = FixArmor(regeneration.FixMulti, regenEffectStatus.FixEffect);
            chargeShield = ChargeShield(regeneration.ShieldMulti, regenEffectStatus.ChargeEffect,
                regenEffectStatus.ExtraChargeMulti);
        }

        private int GetHeal(float regenerationHealMulti, float healMulti)
        {
            if (NowHp >= MaxHp)
            {
                return 0;
            }

            SurvivalChangeMarks.Add(SurvivalChangeMark.HpChange);
            var lastHp = NowHp;
            NowHp = (uint)MathTools.Min(NowHp + regenerationHealMulti * MaxHp * healMulti, MaxHp);
            return (int)NowHp - (int)lastHp;
        }

        private int FixArmor(float regenerationFixMulti, float fixMulti)
        {
            if (NowArmor >= MaxArmor)
            {
                return 0;
            }

            SurvivalChangeMarks.Add(SurvivalChangeMark.ArmorChange);
            var lastArmor = NowArmor;
            NowArmor = (uint)MathTools.Min(NowArmor + regenerationFixMulti * MaxArmor * fixMulti, MaxArmor);
            return (int)NowArmor - (int)lastArmor;
        }

        private int ChargeShield(float regenerationShieldMulti, float chargeMulti, float extraChargeMulti)
        {
            SurvivalChangeMarks.Add(SurvivalChangeMark.ShieldChange);
            var maxShield = (uint)(regenerationShieldMulti * MaxShield * chargeMulti);
            var lastShield = NowShield;
            NowShield = (uint)MathTools.Min(MaxShield * (1 + extraChargeMulti), NowShield + maxShield);
            return (int)NowShield - (int)lastShield;
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
            var baseAttributeMaxHp = (uint)(baseAttribute.MaxHP * multi);
            var baseAttributeMaxArmor = (uint)(baseAttribute.MaxArmor * multi);
            var baseAttributeArmorDefence = baseAttribute.ArmorDefence;
            var baseAttributeMaxShield = (uint)(baseAttribute.MaxShield * multi);
            var baseAttributeShieldRecover = (uint)Math.Round(baseAttribute.ShieldRecover);
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

            var lossHp = (int)MaxHp - (int)NowHp;
            MaxHp = (uint)MathTools.Max(1, baseSurvivalStatus.MaxHp * (1 + v[0]));
            NowHp = (uint)MathTools.Max(1, (int)MaxHp - lossHp);
            var lossAr = (int)MaxArmor - (int)NowArmor;
            MaxArmor = (uint)MathTools.Max(0, baseSurvivalStatus.MaxArmor * (1 + v[1]));
            NowArmor = (uint)MathTools.Max(0, (int)MaxArmor - lossAr);
            ArmorDefence = (uint)MathTools.Max(0, baseSurvivalStatus.ArmorDefence * (1 + v[2]));
            MaxShield = (uint)MathTools.Max(0, baseSurvivalStatus.MaxShield * (1 + v[3]));
            var shieldRecover = baseSurvivalStatus.ShieldRecover * (1 + v[4]);
            ShieldRecover = (uint)MathTools.Max(0, shieldRecover);
            var shieldInstability = baseSurvivalStatus.ShieldInstability * (1 + v[5]);
            ShieldInstability = (uint)MathTools.Max(0, shieldInstability);
            ShieldDelayTick = (uint)(baseSurvivalStatus.ShieldDelayTick / (1 + v[6]));
            SurvivalChangeMarks.Add(SurvivalChangeMark.MaxValueChange);
        }

        public float GenShortStatus()
        {
            var nowShield = (NowHp + NowArmor + NowShield) / (float)(MaxArmor + MaxShield + MaxHp);
            return nowShield;
        }

        public Regen AbsorbDamage(uint total, uint hit, AbsorbStatus absorbStatus, uint damageShardedDamage,
            float extraChargeMulti)
        {
            var hpAbs = HpAbs(absorbStatus.HpAbs, total);
            var armorAbs = ArmorAbs(absorbStatus.ArmorAbs, total, hit, damageShardedDamage);
            var shieldAbs = ShieldAbs(absorbStatus.ShieldAbs, total, hit, extraChargeMulti);
            var regen = new Regen(hpAbs, armorAbs, shieldAbs, 0);
            return regen;
        }

        private int ShieldAbs(float absorbStatusShieldAbs, uint damage, uint hit, float extraChargeMulti)
        {
            var shieldInstability = damage + hit * ShieldInstability;
            var lastShield = NowShield;
            NowShield = (uint)MathTools.Min(NowShield + MathTools.Max(0, shieldInstability * absorbStatusShieldAbs),
                MaxShield * (1 + extraChargeMulti));
            SurvivalChangeMarks.Add(SurvivalChangeMark.ShieldChange);
            return (int)(NowShield - lastShield);
        }

        private int ArmorAbs(float absorbStatusArmorAbs, uint damage, uint hit, uint damageShardedDamage)
        {
            var shieldInstability = Math.Max(0, (damage - hit * Math.Min(ArmorDefence, damageShardedDamage)));
            var lastArmor = NowArmor;
            NowArmor = (uint)MathTools.Min(MaxArmor,
                NowArmor + MathTools.Max(0, shieldInstability * absorbStatusArmorAbs));
            SurvivalChangeMarks.Add(SurvivalChangeMark.ArmorChange);
            return (int)(NowArmor - lastArmor);
        }

        private int HpAbs(float absorbStatusHpAbs, uint damage)
        {
            var lastHp = NowHp;
            NowHp = (uint)MathTools.Min(MaxHp, NowHp + MathTools.Max(0, damage * absorbStatusHpAbs));
            SurvivalChangeMarks.Add(SurvivalChangeMark.HpChange);
            return (int)(NowHp - lastHp);
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
            return (float)NowArmor / MathTools.Max(1, MaxArmor);
        }

        public float HpPercent()
        {
            return (float)NowHp / MathTools.Max(1, MaxHp);
        }

        public float ShieldPercent()
        {
            return (float)NowShield / MathTools.Max(1, MaxShield);
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

        public void GetMainValues(out int nowShield, out int nowArmor, out int nowHp)
        {
            nowShield = (int)NowShield;
            nowArmor = (int)NowArmor;
            nowHp = (int)NowHp;
        }

        public void GetMainLost(int lastS, int lastA, int lastH, out int lossS, out int lossA, out int lossH)
        {
            lossS = MathTools.Max(0, lastS - (int)NowShield);
            lossA = MathTools.Max(0, lastA - (int)NowArmor);
            lossH = MathTools.Max(0, lastH - (int)NowHp);
#if DEBUG
            Console.Out.WriteLine(
                $"take damage loss Sd:{lossS},AM {lossA} ,HP {lossH} result: {this}");
#endif
        }

        private void ShieldValueRegen(int regen)
        {
#if DEBUG
            Console.Out.WriteLine($"regen shield by TransRegen {NowShield} {regen}");
#endif
            NowShield = (uint)MathTools.Max(0, (int)NowShield + regen);
#if DEBUG
            Console.Out.WriteLine($"regen shield by TransRegen {NowShield} ");
#endif
        }

        private void ArmorValueRegen(int regen)
        {
#if DEBUG
            Console.Out.WriteLine($"regen armor by TransRegen {NowArmor} {regen}");
#endif
            NowArmor = (uint)MathTools.Max(0,
                MathTools.Min(MathTools.Max(NowArmor, MaxArmor), (int)NowArmor + regen));
#if DEBUG
            Console.Out.WriteLine($"regen armor by TransRegen {NowArmor} ");
#endif
        }

        private void HpValueRegen(int regen)
        {
            if (NowHp <= 0) return;
            {
#if DEBUG
                Console.Out.WriteLine($"regen Hp by TransRegen {NowHp} {regen}");
#endif

                NowHp = (uint)MathTools.Max(1, MathTools.Min(MathTools.Max(NowHp, MaxHp), (int)NowHp + regen));
#if DEBUG
                Console.Out.WriteLine($"regen Hp by TransRegen {NowHp}");
#endif
            }
        }

        private void TransRegen(int sR, int aR, int hR)
        {
            ShieldValueRegen(sR);
            ArmorValueRegen(aR);
            HpValueRegen(hR);
        }

        public int[] TakeDamageAndEtc(Damage damage, TransRegenEffectStatus regenEffectStatus,
            out float protectMulti,
            out float propMulti)
        {
            GetMainValues(out var lS, out var lA, out var lH);
            var takeAllDamage = TakeAllDamage(damage, regenEffectStatus, out protectMulti, out propMulti);

            GetMainLost(lS, lA, lH, out var lls, out var lossA, out var lossH);
            regenEffectStatus.GetTransValue(lls, lossA, lossH, out var sR, out var armorR, out var hpR);
            TransRegen(sR, armorR, hpR);
            return takeAllDamage;
        }

        private int[] TakeAllDamage(Damage damage, TransRegenEffectStatus transRegenEffectStatus,
            out float protectMulti,
            out float propMulti)
        {
            var harms = TakeDamage(damage, out var shieldBreak, out var armorBreak);
            protectMulti = 0f;
            propMulti = 0f;
            damage.GetOtherMulti(damage.OnBreakMulti);
            if (shieldBreak)
            {
                var takeDamage = TakeDamage(damage, out _, out _);
                harms[0] += takeDamage[0];
                if (takeDamage[1] >= 0)
                {
                    harms[1] = Math.Max(0, harms[1]) + takeDamage[1];
                }

                var regenOnShieldBreak = RegenOnShieldBreak(transRegenEffectStatus, out var p);
                protectMulti += regenOnShieldBreak;
                propMulti += p;
            }

            if (!armorBreak) return harms;
            var ints = TakeDamage(damage, out _, out _);
            harms[0] += ints[0];
            if (ints[1] >= 0)
            {
                harms[1] = Math.Max(0, harms[1]) + ints[1];
            }

            var regenOnArmorBreak = RegenOnArmorBreak(transRegenEffectStatus, out var ppp);
            protectMulti += regenOnArmorBreak;
            propMulti += ppp;
            return harms;
        }

        private float RegenOnArmorBreak(TransRegenEffectStatus transRegenEffectStatus, out float pp)
        {
            var armorBreakValue =
                transRegenEffectStatus.GetArmorBreakValue(out var armorBreakShield, out pp);
            var breakShield = (int)(armorBreakShield * MaxShield);

            ShieldValueRegen(breakShield);
            return armorBreakValue;
        }

        private float RegenOnShieldBreak(TransRegenEffectStatus transRegenEffectStatus, out float propMulti)
        {
            var shieldBreakValue =
                transRegenEffectStatus.GetShieldBreakValue(out var shieldBreakArmor, out propMulti);
            var breakArmor = (int)(shieldBreakArmor * MaxArmor);
            ArmorValueRegen(breakArmor);
            return shieldBreakValue;
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