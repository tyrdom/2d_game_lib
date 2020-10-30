namespace game_stuff
{
    public class AttackStatus
    {
        private uint BaseAttack { get; }

        private uint NowAttack { get; set; }

        public uint GenDamage(float damageMulti)
        {
            return (uint) (NowAttack * damageMulti);
        }


        public void ChangeAttack()
        {
            
        }

        public AttackStatus(uint baseAttack)
        {
            BaseAttack = baseAttack;
            NowAttack = baseAttack;
        }

        public static AttackStatus StandardAttackStatus()
        {
            return new AttackStatus(100);
        }
    }
}