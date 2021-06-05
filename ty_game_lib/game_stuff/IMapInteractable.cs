using System.Collections;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public interface IMapInteractable : IAaBbBox, IMapMarkId
    {
        public void RecordQuad(int qI);

        public Queue<Quad> LocateRecord { get; }
        public Interaction? GetActOne(CharacterStatus characterStatus);
        public Interaction? GetActTwo(CharacterStatus characterStatus);
        public Round CanInterActiveRound { get; }
        public CharacterBody? NowInterCharacterBody { get; set; }
        public Interaction CharActOne { get; }
        public Interaction CharActTwo { get; }
        public void ReLocate(TwoDPoint pos);
        public void StartActOneBySomeBody(CharacterBody characterBody);
        public void StartActTwoBySomeBody(CharacterBody characterBody);
    }
}