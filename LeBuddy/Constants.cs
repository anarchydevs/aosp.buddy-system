using AOSharp.Common.GameData;
using System.Collections.Generic;

namespace LeBuddy
{
    public static class Constants
    {
        public const string UnicornRecruiter = "Unicorn Recruiter";

        public static List<string> _ignores = new List<string>
        {
            "Zix",
            "Nanovoider",
            //"Alien Coccoon"
        };

        public static Vector3 _unicornRecruiter = new Vector3(149.6f, 100.9f, 164.5f);
        public static Vector3 _reclaim = new Vector3(160.0f, 101.0f, 177.1f);
        public static Vector3 _reformArea = new Vector3(64.5f, 100.7f, 184.1f);
        public static Vector3 _entrance = new Vector3(64.1f, 101.1f, 181.3f);
        public static Vector3 _entranceStart = new Vector3(65.1f, 100.7f, 184.2f);//65.1, 184.2, 100.7
        public static Vector3 _entranceEnd = new Vector3(64.5f, 101.1f, 180.6f); // 64.5, 180.6, 101.1
        //public static Vector3 _doorExit = new Vector3(64.1f, 101.1f, 181.3f);// one 296.4, 165.0, 8.2 two 0.1, 225.3, 8.0 three 1.8, 145.0, 8.3, four 3.0, 125.0, 8.2,
        public static Vector3 _buttonExit = new Vector3(150.7f, 520.0f, 163.9f);


        public const int UnicornOutpost = 4364;
        public const int Ship = 101181;
    }
}
