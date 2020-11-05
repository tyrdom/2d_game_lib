using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public static class MapInteractableDefault
    {
        public static List<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical,
            IMapInteractable mapInteractable)
        {
            var splitByQuads = mapInteractable.Zone.SplitByQuads(horizon, vertical);
            return splitByQuads
                .Select(x => (x.Item1,
                    mapInteractable.FactAaBbBox(x.Item2)))
                .ToList();
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
            var actOne = mapInteractable.GetActTwo(characterBody.CharacterStatus);
            if (actOne == null)
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