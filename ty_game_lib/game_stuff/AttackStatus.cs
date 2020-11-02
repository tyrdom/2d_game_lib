namespace game_stuff
{
    public class AttackStatus
    {
        private uint BaseAttack { get; }

        private uint MainAttack { get; set; }

        private uint ShardedAttack { get; }

        private uint BaseShardedNum { get; }
        private uint ShardedNum { get; set; }

        public Damage GenDamage(float damageMulti)
        {
            var mainDamage = (uint) (MainAttack * damageMulti);
            var shardedDamage = (uint) (ShardedAttack * damageMulti);
            var damage = new Damage(ShardedNum, mainDamage, shardedDamage);
            return damage;
        }


        public void ChangeAttack()
        {
        }

        public AttackStatus(uint baseAttack)
        {
            BaseAttack = baseAttack;
            ShardedAttack = (uint) (baseAttack * TempConfig.ShardedAttackMulti);
            BaseShardedNum = 0;
            ShardedNum = 0;
            MainAttack = baseAttack;
        }

        public static AttackStatus StandardAttackStatus()
        {
            return new AttackStatus(100);
        }

        public void GetPassiveDo(IPassiveTrait passiveTrait)
        {
        }
    }
}