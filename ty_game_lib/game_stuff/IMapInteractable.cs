using System.Collections;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public interface IMapInteractable : IAaBbBox
    {
        public Queue<Quad> LocateRecord { get; set; }
        public Interaction? GetAct(CharacterStatus characterStatus);
        Round CanInterActiveRound { get; }
        public CharacterBody? NowInterCharacterBody { get; set; }
        public Interaction CharAct { get; }
        public void ReLocate(TwoDPoint pos);

        public void StartPickBySomeBody(CharacterBody characterBody);
        public void StartRecycleBySomeBody(CharacterBody characterBody);
    }
}