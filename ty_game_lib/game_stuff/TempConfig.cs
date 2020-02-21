using System;
using System.Collections.Generic;
using collision_and_rigid;

namespace game_stuff
{
    public static class TempConfig
    {
        public static readonly Dictionary<BodySize, float> SizeToR = new Dictionary<BodySize, float>
        {
            [BodySize.Small] = 1.5f,
            [BodySize.Medium] = 3f,
            [BodySize.Big] = 4.5f
        };

        public static readonly Dictionary<BodySize, float> SizeToMass = new Dictionary<BodySize, float>
        {
            [BodySize.Small] = 2.25f,
            [BodySize.Medium] = 9f,
            [BodySize.Big] = 20.25f
        };


        public static float MaxHeight = 2f;
        public static float MaxUp = MathF.Sqrt(2f * G * MaxHeight);
        public static float G = 10f;
        public static readonly float Friction = 1f;
    }

    public static class GameTools
    {
        public static void DoOp(IIdPointShape idPointShape, Dictionary<int, Operate> gidToOp)
        {
            switch (idPointShape)
            {
                case CharacterBody characterBody:
                    characterBody.DoOpFromDic(gidToOp);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(idPointShape));
            }
        }

        public static float GetMaxUp(float? height)
        {
            if (height == null)
            {
                return TempConfig.MaxUp;
            }

            var maxHeight = TempConfig.MaxHeight - height.Value;
            var f = 2f * TempConfig.G * maxHeight;
            return MathF.Sqrt(f);
        }
    }
}