using AOSharp.Common.GameData;
using System.Collections.Generic;

namespace DB2Buddy
{
    public static class Constants
    {
        public static List<Vector3> _pathToAune = new List<Vector3>()
        {
            new Vector3(280.1f, 135.3f, 143.7f),
            new Vector3(293.7f, 135.3f, 149.0f),
            new Vector3(301.6f, 135.3f, 159.3f),
            new Vector3(294.1f, 135.4f, 197.1f),
            new Vector3(273.9f, 135.4f, 197.9f),
            new Vector3(269.5f, 135.4f, 201.2f),
            new Vector3(271.3f, 134.5f, 204.9f),
            new Vector3(267.6f, 133.4f, 208.3f),
            new Vector3(265.8f, 133.4f, 217.7f),
            new Vector3(274.1f, 133.4f, 224.0f),
            new Vector3(279.2f, 133.4f, 223.9f),
            new Vector3(286.4f, 133.4f, 230.8f)
        };

        public static Vector3 _teamFormStartPos = new Vector3(2108.7f, 0.0f, 2771.2f);
        public static Vector3 _entrancePos = new Vector3(2121.8f, 0.4f, 2769.1f).Randomize(2f);

        public const int Outside = 570;
        public const int Inside = 6055;

    }
}
