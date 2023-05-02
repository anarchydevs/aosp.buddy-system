using AOSharp.Common.GameData;
using System.Collections.Generic;

namespace VortexxBuddy
{
    public static class Constants
    {
        //public static List<Vector3> _pathToStartPos = new List<Vector3>()
        //{
        //    new Vector3(417.4f, 6.2f, 194.2f),// 417.4, 194.2, 6.2
        //    new Vector3(411.8f, 14.1f, 228.5f),//411.8, 228.5, 14.1
        //    new Vector3(406.7f, 26.0f, 262.1f),//406.7, 262.1, 26.0
        //    new Vector3(400.3f, 29.8f, 295.1f)
        //};

        //outside
        public static Vector3 _entrance = new Vector3(524.9f, 310.9f, 305.8f);
        public static Vector3 _reneterPos = new Vector3(522.4f, 310.9f, 308.9f); 

        //inside
        //public static Vector3 _atDoor = new Vector3(422.7f, 5.7f, 141.8f); 
        public static Vector3 _centerPodium = new Vector3(205.0f, 17.6f, 202.3f); 
       // public static Vector3 _returnPosition = new Vector3(397.7f, 28.2f, 353.3f);

        //Podiums
        public static Vector3 _redPodium = new Vector3(175.0f, 17.6f, 201.1f); //West
        public static Vector3 _greenPodium = new Vector3(204.1f, 17.8f, 230.0f); //North
        public static Vector3 _yellowPodium = new Vector3(233.0f, 16.7f, 202.0f); //East
        public static Vector3 _bluePodium = new Vector3(205.0f, 17.5f, 179.2f); //South


        //Instance IDs
        public const int VortexxId = 6061;
        public const int XanHubId = 6013;

    }
}
