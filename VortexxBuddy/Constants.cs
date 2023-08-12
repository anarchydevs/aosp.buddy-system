using AOSharp.Common.GameData;
using System.Collections.Generic;

namespace VortexxBuddy
{
    public static class Constants
    {
        
        //outside
        public static Vector3 _entrance = new Vector3(524.9f, 310.9f, 305.8f);
        public static Vector3 _reneterPos = new Vector3(522.4f, 310.9f, 308.9f);
        public static Vector3 _startPos = new Vector3(519.2f, 310.9f, 314.3f);

        //inside
        public static Vector3 _centerPodium = new Vector3(205.0f, 17.6f, 202.3f); 
       // public static Vector3 _returnPosition = new Vector3(397.7f, 28.2f, 353.3f);

        //Podiums
        public static Vector3 _redPodium = new Vector3(175.0f, 17.6f, 201.1f); //West
        public static Vector3 _greenPodium = new Vector3(204.1f, 17.8f, 230.0f); //North
        public static Vector3 _yellowPodium = new Vector3(233.0f, 16.7f, 202.0f); //East
        public static Vector3 _bluePodium = new Vector3(205.0f, 17.5f, 179.2f); //South

        //Beacon
        public static Vector3 _BeaconPos = new Vector3(191.0f, 17.1f, 208.9f);

        //Instance IDs
        public const int VortexxId = 6061;
        public const int XanHubId = 6013;

    }
}
