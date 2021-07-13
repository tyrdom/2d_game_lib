namespace game_stuff
{
    public struct Damage
    {
        public uint MainDamage { get; private set; }
        public uint ShardedDamage { get; }
        public uint ShardedNum { get; private set; }
        public float OnBreakMulti { get; }

        public Damage(uint shardedNum, uint mainDamage, uint shardedDamage, float onBreakMulti)
        {
            ShardedNum = shardedNum;
            MainDamage = mainDamage;
            ShardedDamage = shardedDamage;
            OnBreakMulti = onBreakMulti;
        }

        
        public void GetOtherMulti(float damageMulti)
        {
            MainDamage = (uint) (damageMulti * MainDamage);
            ShardedNum = (uint) (damageMulti * ShardedNum);
        }
    }
}