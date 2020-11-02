namespace game_stuff
{
    public readonly struct Damage
    {
        public uint MainDamage { get; }
        public uint ShardedDamage { get; }
        public uint ShardedNum { get; }

        public Damage(uint shardedNum, uint mainDamage, uint shardedDamage)
        {
            ShardedNum = shardedNum;
            MainDamage = mainDamage;
            ShardedDamage = shardedDamage;
        }

        public Damage GetBuff(float damageMulti)
        {
            var damageBuffDamageMultiM = (uint) (damageMulti * MainDamage);
            var damageBuffDamageMultiS = (uint) (damageMulti * ShardedDamage);
            return new Damage(ShardedNum, damageBuffDamageMultiM, damageBuffDamageMultiS);
        }
    }
}