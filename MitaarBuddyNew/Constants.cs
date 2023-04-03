using AOSharp.Common.GameData;
using System.Collections.Generic;

namespace MitaarBuddy
{
    public static class Constants
    {

        public static List<Vector3> _pathToMitaar = new List<Vector3>()
        {
            new Vector3(610.0f, 309.8f, 519.8f),// 610.0, 519.8, 309.8 // reclaim
            new Vector3(606.1, 310.1f, 517.5f),//606.1, 517.5, 310.1 //602.0, 511.0, 310.9,
            new Vector3(594.6, 310.9f, 506.0f),//594.6, 506.0, 310.9,
            new Vector3(593.5f, 310.9f, 501.8f),// 593.5, 501.8, 310.9, 594.4, 505.1, 310.9,
            new Vector3(584.0f, 310.9f, 471.6f),// 584.0, 471.6, 310.9,
            new Vector3(605.4f, 309.4f, 446.2f),// 605.4, 446.2, 309.4
            new Vector3(594.7f, 309.1f, 392.2f),// 594.7, 392.2, 309.1
            new Vector3(563.8f, 310.9f, 381.9f),// 563.8, 381.9, 310.9
            new Vector3(536.6f, 310.9f, 370.0f),// 536.6, 370.0, 310.9
            new Vector3(526.3f, 310.9f, 329.3f),// 526.3, 329.6, 310.9
            new Vector3(484.1f, 308.6f, 329.3f),// 484.1, 329.3, 308.6
            new Vector3(438.9f, 312.1f, 355.6f),// 438.9, 355.6, 312.1
            new Vector3(388.2f, 309.0f, 383.1f),// 388.2, 383.1, 309.0,
            new Vector3(358.4f, 310.9f, 411.0f),// 358.4, 411.0, 310.9, // mitaar
            new Vector3(354.7f, 310.9f, 410.4f)//354.7, 410.4, 310.9
        };

        public static List<string> _ignores = new List<string>
        {
                    ""
        };

        public static Vector3 S13EntrancePos = new Vector3(145.7f, 0.5f, 206.5f);
        public static Vector3 S13ZoneOutPos = new Vector3(149.5f, 0.5f, 206.4f);
        public static Vector3 S13ExitPos = new Vector3(168f, 36.4f, 49.5f);
        public static Vector3 S13StartPos = new Vector3(150.7f, 36.4f, 42.0f);
        public static Vector3 S13GoalPos = new Vector3(170.3f, 6.2f, 486.1f); //170.3f, 6.2f, 486.1f           197f, 5.1f, 472f
        public static Vector3 S13FirstCorrectionPos = new Vector3(73.7f, 31.6f, 333.1f);
        public static Vector3 S13SecondCorrectionPos = new Vector3(70.3f, 6.9f, 467.1f);
        public static Vector3 testpos = new Vector3(132.9f, 36.4f, 49.8f);
        public static Vector3 XanHubPos = new Vector3(585.4f, 0.0f, 740.2);

        //public static Vector3 S13GoalPos = new Vector3(139.1f, 36.24f, 43.9f); //RapidReformTest
        public static Vector3 S28EntrancePos = new Vector3(232.6f, 1.0f, 169.9f);
        public static Vector3 S35EntrancePos = new Vector3(230.9f, 0.3f, 239.4f);
        public static Vector3 S35ExitPos = new Vector3(520.5f, 4.0f, 58.5f);
        public const int S13Id = 4365;
        public const int S28Id = 4367;
        public const int S35Id = 4366;
        public const int APFHubId = 4368;
        public const int UnicornHubId = 4364;
        public const int XanHubId = 6013;
        public const float MaxPullDistance = 30;
        public const float FightDistance = 3;
    }
}
