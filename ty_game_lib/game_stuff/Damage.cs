namespace game_stuff
{
    public struct Damage
    {
        public uint MainDamage { get; private set; }
        public uint ShardedDamage { get; }
        public uint ShardedNum { get; private set; }

        public Damage(uint shardedNum, uint mainDamage, uint shardedDamage)
        {
            ShardedNum = shardedNum;
            MainDamage = mainDamage;
            ShardedDamage = shardedDamage;
        }


        public void GetBuffMulti(float damageMulti)
        {
            MainDamage = (uint) (damageMulti * MainDamage);
            ShardedNum = (uint) (damageMulti * ShardedNum);
        }
    }
}