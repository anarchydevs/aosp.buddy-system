using AOSharp.Common.GameData;
using AOSharp.Core;
using System.Collections.Generic;

namespace MitaarBuddy
{
    public static class Constants
    {
        // wont need when we get the navmeshes
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


        public static Vector3 _reclaim = new Vector3(610.0f, 309.8f, 519.8f);
        public static Vector3 _entrance = new Vector3(347.0f, 310.9f, 407.7f).Randomize(2f);
        public static Vector3 _reneterPos = new Vector3(353.2f, 310.9f, 409.3f);

        public static Vector3 _startPosition = new Vector3(91.3f, 12.1f, 110.2f);
        public static Vector3 _greenPodium = new Vector3(108.6f, 12.1f, 110.3f);
        public static Vector3 _redPodium = new Vector3(91.3f, 12.1f, 110.2f);
        public static Vector3 _bluePodium = new Vector3(92.2f, 12.1f, 97.8f);
        public static Vector3 _yellowPodium = new Vector3(108.7f, 12.1f, 97.6f);

        public const int MitaarId = 6017;

        public const int XanHubId = 6013;

       
    }
}
