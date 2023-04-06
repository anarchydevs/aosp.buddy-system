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

        public static Vector3 StartPos = new Vector3(339.3f, 31.0f, 134.0f); //start
        public static Vector3 FirstPos = new Vector3(314.7f, 33.0f, 187.1f);
        public static Vector3 SecondPos = new Vector3(343.5f, 33.0f, 229.6f);
        public static Vector3 ThirdPos = new Vector3(352.9f, 36.0f, 341.8f);
        public static Vector3 ForthPos = new Vector3(300.4f, 38.9f, 427.7f);
        public static Vector3 FifthPos = new Vector3(371.8f, 34.8f, 508.4f);
        public static Vector3 SixthPos = new Vector3(437.5f, 30.2f, 535.6f); 
        public static Vector3 SeventhPos = new Vector3(434.3f, 30.0f, 612.3f);
        public static Vector3 EighthPos = new Vector3(377.1f, 30.0f, 613.2f);
        public static Vector3 NinethPos = new Vector3(240.3f, 37.2f, 371.4f);
        public static Vector3 TenthPos = new Vector3(188.4f, 40.4f, 302.3f);
        public static Vector3 LastPos = new Vector3(211.2f, 36.8f, 251.6f);
        public static Vector3 EndPos = new Vector3(268.4f, 32.7f, 229.2f); //End

        public const int Inferno = 4005;
        public const int Albtraum = 4337;

        public const float MaxPullDistance = 60;
        public const float FightDistance = 3;
    }
}
