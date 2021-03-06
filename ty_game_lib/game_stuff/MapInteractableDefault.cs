using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public static class MapInteractableDefault
    {
        public static IAaBbBox RecardQuad(int qI, IMapInteractable mapInteractable)
        {
            switch (qI)
            {
                case 0:
                    return mapInteractable;
                case 1:
                    mapInteractable.LocateRecord.Enqueue(Quad.One);
                    return mapInteractable;
                case 2:
                    mapInteractable.LocateRecord.Enqueue(Quad.Two);
                    return mapInteractable;
                case 3:
                    mapInteractable.LocateRecord.Enqueue(Quad.Three);
                    return mapInteractable;
                case 4:
                    mapInteractable.LocateRecord.Enqueue(Quad.Four);
                    return mapInteractable;

                default:
                    throw new ArgumentException($"no good id {qI}");
            }
        }

        public static IEnumerable<(int, IAaBbBox)> SplitByQuads(Zone zone,
            IMapInteractable mapInteractable)
        {
            var (horizon, vertical) = zone.GetMid();
            var splitByQuads = mapInteractable.Zone.InWhichQ(horizon, vertical);
            mapInteractable.RecordQuad(splitByQuads);
            return new List<(int, IAaBbBox)>
            {
                (splitByQuads,
                    mapInteractable)
            };
        }

        public static Quad? GetNextQuad(IMapInteractable mapInteractable)
        {
            var nextQuad = mapInteractable.LocateRecord.Count > 0
                ? mapInteractable.LocateRecord.Dequeue()
                : (Quad?) null;
            return nextQuad;
        }

        public static void ReLocate(TwoDPoint pos, IMapInteractable mapInteractable)
        {
            mapInteractable.LocateRecord.Clear();
            mapInteractable.CanInterActiveRound.O = pos;
            mapInteractable.NowInterCharacterBody = null;
            mapInteractable.Zone = mapInteractable.CanInterActiveRound.GetZones();
        }

        public static void StartActOneBySomeBody(CharacterBody characterBody, IMapInteractable mapInteractable)
        {
            var actOne = mapInteractable.GetActOne(characterBody.CharacterStatus);
            if (actOne == null)
            {
                return;
            }

            if (characterBody.CharacterStatus.LoadInteraction(mapInteractable.CharActOne))
                mapInteractable.NowInterCharacterBody = characterBody;
        }

        public static void StartActTwoBySomeBody(CharacterBody characterBody, IMapInteractable mapInteractable)
        {
            var actTwo = mapInteractable.GetActTwo(characterBody.CharacterStatus);
            if (actTwo == null)
            {
                return;
            }

            if (characterBody.CharacterStatus.LoadInteraction(mapInteractable.CharActTwo))
                mapInteractable.NowInterCharacterBody = characterBody;
        }

        public static void WriteQuadRecord(Quad quad, IMapInteractable mapInteractable)
        {
            mapInteractable.LocateRecord.Enqueue(quad);
        }
    }
}