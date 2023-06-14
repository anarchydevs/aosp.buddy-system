using AOSharp.Common.GameData;
using System.Collections.Generic;

namespace Db1Buddy
{
    public static class Constants
    {
        public static List<Vector3> _pathToStartPos = new List<Vector3>()
        {
            new Vector3(417.4f, 6.2f, 194.2f),// 417.4, 194.2, 6.2
            new Vector3(411.8f, 14.1f, 228.5f),//411.8, 228.5, 14.1
            new Vector3(406.7f, 26.0f, 262.1f),//406.7, 262.1, 26.0
            new Vector3(400.3f, 29.8f, 295.1f)
        };

        //outside
        public static Vector3 _entrance = new Vector3(2122.8f, 4.6f, 2760.0f);
        public static Vector3 _reneterPos = new Vector3(2109.2f, 0.0f, 2763.1f);

        //inside
        public static Vector3 _atDoor = new Vector3(422.7f, 5.7f, 141.8f); 
        public static Vector3 _startPosition = new Vector3(400.3f, 29.8f, 295.1f);
        public static Vector3 _returnPosition = new Vector3(397.7f, 28.2f, 353.3f); 

        //Podiums
        public static Vector3 _greenPodium = new Vector3(363.8f, 29.7f, 395.3f); 
        public static Vector3 _redPodium = new Vector3(441.9f, 32.0f, 318.3f);
        public static Vector3 _bluePodium = new Vector3(439.2f, 30.1f, 384.6f);
<<<<<<< HEAD
        public static Vector3 _yellowPodium = new Vector3(357.5f, 34.8f, 324.9f); 
=======
        public static Vector3 _yellowPodium = new Vector3(357.5f, 34.8f, 324.9f);
>>>>>>> aab7ee3ccaa03c6ad6b10dee74da529f4148bb84

        //Instance IDs
        public const int DB1Id = 6003;
        public const int PWId = 570;

    }
}
