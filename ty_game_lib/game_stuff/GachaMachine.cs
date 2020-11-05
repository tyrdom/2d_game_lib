using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public class GachaMachine : IMapInteractable
    {
        public void WriteQuadRecord(Quad quad)
        {
            throw new NotImplementedException();
        }

        public Quad? GetNextQuad()
        {
            throw new NotImplementedException();
        }

        public IShape GetShape()
        {
            throw new NotImplementedException();
        }

        public TwoDPoint GetPos()
        {
            throw new NotImplementedException();
        }

        public bool CanInteractive(TwoDPoint pos)
        {
            throw new NotImplementedException();
        }

        public Zone Zone { get; set; }

        public List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical)
        {
            throw new NotImplementedException();
        }

        public IAaBbBox FactAaBbBox(Zone zone)
        {
            throw new NotImplementedException();
        }

        public bool IsCage()
        {
            return false;
        }

        public bool IsVehicle()
        {
            return false;
        }

        public bool IsSale()
        {
            return true;
        }

        public Queue<Quad> LocateRecord { get; set; }

        public Interaction? GetActOne(CharacterStatus characterStatus)
        {
            throw new NotImplementedException();
        }

        public Interaction? GetActTwo(CharacterStatus characterStatus)
        {
            throw new NotImplementedException();
        }

        public Round CanInterActiveRound { get; }
        public CharacterBody? NowInterCharacterBody { get; set; }
        public Interaction CharActOne { get; }
        public Interaction CharActTwo { get; }

        public void ReLocate(TwoDPoint pos)
        {
            throw new NotImplementedException();
        }

        public void StartActOneBySomeBody(CharacterBody characterBody)
        {
            throw new NotImplementedException();
        }

        public void StartActTwoBySomeBody(CharacterBody characterBody)
        {
            throw new NotImplementedException();
        }
    }
}