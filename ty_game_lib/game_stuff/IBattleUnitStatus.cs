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

        Damage GenDamage(float damageMulti, bool back);
        void LoadCatchTrickSkill(TwoDVector? aim, CatchStunBuffMaker catchAntiActBuffMaker);
        float GetRr();
        void AddAKillScore(CharacterBody characterBody);
    }
}