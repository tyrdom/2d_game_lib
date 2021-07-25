using collision_and_rigid;

namespace game_stuff
{
    public interface IBattleUnitStatus
    {
        public DmgShow? TakeDamage(Damage genDamage);
        CharacterBody GetFinalCaster();
        CharacterStatus? CatchingWho { get; set; }
        TwoDPoint GetPos();
        int GetId();

        void BaseBulletAtkOk(int pauseToCaster, int ammoAddWhenSuccess, IBattleUnitStatus targetCharacterStatus);
        (UnitType unitType, int gid) GetTypeAndId();
        Damage GenDamage(float damageMulti, bool back);
        void LoadCatchTrickSkill(TwoDVector? aim, CatchStunBuffMaker catchAntiActBuffMaker);
        float GetRr();
        void AddAKillScore(CharacterBody characterBody);
        bool IsDeadOrCantDmg();
        StunFixStatus GetStunFixStatus();
    }
}