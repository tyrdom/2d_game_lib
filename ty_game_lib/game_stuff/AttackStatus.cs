namespace game_stuff
{
    public class AttackStatus
    {
        private uint BaseAttack { get; }

        private uint MainAttack { get; set; }

        private uint ShardedAttack { get; }

        private uint ShardedNum { get; set; }

        public uint GenDamage(float damageMulti)
        {
            return (uint) (MainAttack * damageMulti);
        }


        public void ChangeAttack()
        {
        }

        public AttackStatus(uint baseAttack)
        {
            BaseAttack = baseAttack;
            ShardedAttack = (uint) (baseAttack * TempConfig.ShardedAttackMulti);
            ShardedNum = 0;
            MainAttack = baseAttack;
        }

        public static AttackStatus StandardAttackStatus()
        {
            return new AttackStatus(100);
        }
    }
}