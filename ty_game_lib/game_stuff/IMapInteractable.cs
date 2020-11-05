using System.Collections;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public interface IMapInteractable : IAaBbBox
    {
        public IAaBbBox FactAaBbBox(Zone zone);

        public bool IsCage();
        public bool IsVehicle();

        public bool IsSale();
        public Queue<Quad> LocateRecord { get; set; }
        public Interaction? GetActOne(CharacterStatus characterStatus);
        public Interaction? GetActTwo(CharacterStatus characterStatus);
        Round CanInterActiveRound { get; }
        public CharacterBody? NowInterCharacterBody { get; set; }
        public Interaction CharActOne { get; }
        public Interaction CharActTwo { get; }
        public void ReLocate(TwoDPoint pos);
        public void StartActOneBySomeBody(CharacterBody characterBody);
        public void StartActTwoBySomeBody(CharacterBody characterBody);
    }
}