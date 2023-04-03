using AOSharp.Common.GameData;
using System.Collections.Generic;

namespace ALBBuddy
{
    public static class Constants
    {
       

        public static List<string> _ignores = new List<string>
        {
            "The One Who Reveals The Hidden",
            "One Whose Courage Fails His Heart",
        };

        public static Vector3 EntrancePos = new Vector3(1135.0f, 20.3f, 756.0f);  
        public static Vector3 ZoneOutPos = new Vector3(1138.2f, 19.9f, 755.1f); 
        

        public static Vector3 ExitPos = new Vector3(168f, 36.4f, 49.5f);
        public static Vector3 StartPos = new Vector3(375.3f, 39.4f, 97.2f); 

        public static Vector3 PosOne = new Vector3(415.8f, 32.8f, 301.0f); //415.8, 301.0, 32.8
        public static Vector3 PosTwo = new Vector3(411.3f, 30.0f, 629.1f); //411.3, 629.1, 30.0
        public static Vector3 PosThree = new Vector3(232.3f, 37.9f, 301.6f); //232.3, 301.6, 37.9

        public static Vector3 EndPos = new Vector3(329.2f, 36.0f, 98.8f); //329.2, 98.8, 36.0  

        public const int Inferno = 4005;
        public const int Albtraum = 4337;

        public const float MaxPullDistance = 30;
        public const float FightDistance = 3;
    }
}
