using AOSharp.Common.GameData;
using System.Collections.Generic;

namespace ALBBuddy
{
    public static class Constants
    {
        public static List<Vector3> UnicornHubPath = new List<Vector3>
        {
            new Vector3(140.3f, 100.9f, 178.0f),
            new Vector3(138.9f, 100.9f, 285.4f),
            new Vector3(108.0f, 100.9f, 274.7f),
            new Vector3(109.3f, 100.8f, 232.7f),
            new Vector3(70.5f, 101.0f, 232.0f)
        };

        public static List<string> _ignores = new List<string>
        {
            "The One Who Reveals The Hidden",
            "One Whose Courage Fails His Heart"
        };

        public static Vector3 S13EntrancePos = new Vector3(1135.0f, 20.3f, 756.0f);
        public static Vector3 S13ZoneOutPos = new Vector3(1138.2f, 19.9f, 755.1f);

        public static Vector3 S13ExitPos = new Vector3(168f, 36.4f, 49.5f);
        public static Vector3 S13StartPos = new Vector3(329.7f, 36.0f, 103.1f); //329.7, 103.1, 36.0

        public static Vector3 S13GoalPos = new Vector3(413.2f, 30.0f, 527.1f); //413.2f, 30.0f, 527.1f

        public static Vector3 S13FirstCorrectionPos = new Vector3(292.6f, 33.3f, 190.6f);
        public static Vector3 S13SecondCorrectionPos = new Vector3(263.8f, 33.8f, 379.1f);

        public static Vector3 testpos = new Vector3(132.9f, 36.4f, 49.8f);
        public static Vector3 XanHubPos = new Vector3(585.4f, 0.0f, 740.2);

        //public static Vector3 S13GoalPos = new Vector3(139.1f, 36.24f, 43.9f); //RapidReformTest
        public static Vector3 S28EntrancePos = new Vector3(232.6f, 1.0f, 169.9f);
        public static Vector3 S35EntrancePos = new Vector3(230.9f, 0.3f, 239.4f);
        public static Vector3 S35ExitPos = new Vector3(520.5f, 4.0f, 58.5f);

        public const int S13Id = 4337;
        public const int S28Id = 4367;
        public const int S35Id = 4366;
        public const int APFHubId = 4005;
        public const int UnicornHubId = 4364;
        public const int XanHubId = 6013;
        public const float MaxPullDistance = 30;
        public const float FightDistance = 3;
    }
}
