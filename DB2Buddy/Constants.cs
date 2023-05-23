using AOSharp.Common.GameData;
using System.Collections.Generic;

namespace DB2Buddy
{
    public static class Constants
    {
        public static List<Vector3> _towerPositions = new List<Vector3>()
        {
            new Vector3(294.2f, 135.3f, 199.0f),
            new Vector3(250.9f, 135.3f, 225.1f),
            new Vector3(277.0f, 135.3f, 267.1f),
            new Vector3(320.2f, 135.3f, 242.1f)
        };


        ////outside
        public static Vector3 _entrance = new Vector3(2123.5f, 0.3f, 2768.5f);
        public static Vector3 _centerofentrance = new Vector3(2121.5f, 0.4f, 2769.2f);
        public static Vector3 _append = new Vector3(2120.4f, 0.3f, 2769.8f);
        public static Vector3 _reneterPos = new Vector3(2117.4, 0.0f, 2771.2f);

        ////inside
        ///Unable to find NavMeshPoint for ((286.0045, 133.2877, 233.4044))

        public static Vector3 _atDoor = new Vector3(280.6f, 135.3, 144.0f);
        public static Vector3 _startPosition = new Vector3(285.1f, 133.4f, 229.1f);
        public static Vector3 _centerPosition = new Vector3(286.1f, 133.3f, 233.5f);

        //Nutom pos
        public static Vector3 Pos1 = new Vector3(288.0f, 133.4f, 222.0f); 
        public static Vector3 Pos2 = new Vector3(283.0f, 133.4f, 244.0f); 
        public static Vector3 Pos3 = new Vector3(275.0f, 133.4f, 230.0f); 
        public static Vector3 Pos4 = new Vector3(296.0f, 133.4f, 236.9f); 

        //Tower locations
        public static Vector3 Tow1 = new Vector3(294.2f, 135.3f, 199.0f); 
        public static Vector3 Tow2 = new Vector3(250.9f, 135.3f, 225.1f); 
        public static Vector3 Tow3 = new Vector3(277.0f, 135.3f, 267.1f); 
        public static Vector3 Tow4 = new Vector3(320.2f, 135.3f, 242.1f);


        //Instance IDs
        public const int DB2Id = 6055;
        public const int PWId = 570;

        //podiums 
        public static Vector3 first = new Vector3 (263.9f, 50.8f , 246.0f);
        public static Vector3 second = new Vector3(299.2f, 50.8f, 255.4f);
        public static Vector3 third = new Vector3(307.3f, 50.8f, 220.7f);
        public static Vector3 forth = new Vector3(273.6f, 50.8f, 211.8f);
        
    }
}
