namespace game_stuff
{
    public class AttackStatus
    {
        public uint Attack { get; set; }

        public uint GenDamage(float damageMulti)
        {
            return (uint) (Attack * damageMulti);
        }
    }
}