using System;
using System.Collections.Immutable;
using collision_and_rigid;

namespace game_stuff
{
    internal class ChargeSkillCtrl
    {
        public bool OnCharging { get; private set; }
        public uint NowChargeTick { get; set; }
        private Skill? ReleaseSkill { get; set; }
        private uint MaxChargeTick { get; }
        private uint MaxChargeKeepTick { get; }
        private uint ChargePauseTick { get; }
        private float ChargeDamageMultiAddPerTick { get; }
        private ImmutableDictionary<uint, Skill> ChargeChangeSkills { get; }
        private ImmutableDictionary<uint, PlayingBuffsMaker> TickAddBuffs { get; }

        public ChargeSkillCtrl(uint maxChargeTick, uint maxChargeKeepTick, uint chargePauseTick,
            float chargeDamageMultiAddPerTick, ImmutableDictionary<uint, Skill> chargeChangeSkills,
            ImmutableDictionary<uint, PlayingBuffsMaker> tickAddBuffs)
        {
            OnCharging = false;
            NowChargeTick = 0;
            MaxChargeTick = maxChargeTick;
            MaxChargeKeepTick = maxChargeKeepTick;
            ChargePauseTick = chargePauseTick;
            ChargeDamageMultiAddPerTick = chargeDamageMultiAddPerTick;
            ChargeChangeSkills = chargeChangeSkills;
            TickAddBuffs = tickAddBuffs;
        }

        public float GetChargeDamageMulti()
        {
            return ChargeDamageMultiAddPerTick * MathTools.Min(MaxChargeTick, NowChargeTick);
        }

        public bool GoATickCharge(SkillAction? skillAction, uint nowOnTick, CharacterStatus characterStatus,
            out bool release,
            out Skill? releaseSkill)
        {
            release = false;
            releaseSkill = null;
            if (OnCharging == false)
            {
                return true;
            }

            // Console.Out.WriteLine($"Skill On Charge Check : {skillAction}");
            var b = skillAction != null && (int) skillAction < 6;
            var b1 = NowChargeTick >= MaxChargeKeepTick;
            if (b || b1)
            {
                // Console.Out.WriteLine("Charge release");
                OnCharging = false;
                var chargePause = new ChargePause(false);
                characterStatus.CharEvents.Add(chargePause);
                ReleaseSkill?.TakeChargeValue(NowChargeTick);
                release = true;
                releaseSkill = ReleaseSkill;
                return true;
            }

            if (nowOnTick < ChargePauseTick)
            {
                return true;
            }

            // Console.Out.WriteLine("Charge ing");
            GoATick(characterStatus);
            return false;
        }

        private void GoATick(CharacterStatus characterStatus)
        {
            if (NowChargeTick == 0)
            {
                var chargePause = new ChargePause(true);
                characterStatus.CharEvents.Add(chargePause);
            }

            NowChargeTick++;

            if (TickAddBuffs.TryGetValue(NowChargeTick, out var buffsMaker))
            {
                var playingBuffs = buffsMaker.GenBuffs(characterStatus);
                characterStatus.AddPlayingBuff(playingBuffs);
            }

            if (ChargeChangeSkills.TryGetValue(NowChargeTick, out var skill))
            {
                ReleaseSkill = skill;
            }
        }

        public void StartCharging()
        {
            NowChargeTick = 0;
            OnCharging = true;
            ReleaseSkill = null;
        }

        public void TakeChargeValue(uint nowChargeTick)
        {
            NowChargeTick = nowChargeTick;
        }

        public bool IsCharging()
        {
            return NowChargeTick > 0 && OnCharging;
        }
    }

    public class ChargePause : ICharEvent
    {
        public ChargePause(bool b)
        {
            PauseOn = b;
        }

        public bool PauseOn { get; }
    }
}