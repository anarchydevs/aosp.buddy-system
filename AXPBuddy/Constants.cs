using AOSharp.Common.GameData;
using System.Collections.Generic;

namespace AXPBuddy
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
                    "Alien Fuel Tower",
                    "Unicorn Recon Agent Chittick",
                    "Kyr'Ozch Guardian",
                    "Engorged Sacrificial Husks",
                    "Honor Guard - Ilari'Uri",
                    "Representative of IPS",
                    "Transportation Officer Darren Plush",
                    "Unicorn Field Engineer",
                    "Buckethead Technodealer",
                    "Unicorn Commander Labbe",
                    "Professor Raji Masoud",
                    "Slimy Alien Plant",
                    "Awakened Xan",
                    "Altar of Torture",
                    "Altar of Purification",
                    "Calan-Cur",
                    "Spirit of Judgement",
                    "Wandering Spirit",
                    "Altar of Torture",
                    "Altar of Purification",
                    "Unicorn Coordinator Magnum Blaine",
                    "Xan Spirit",
                    "Watchful Spirit",
                    "Amesha Vizaresh",
                    "Guardian Spirit of Purification",
                    "Tibor 'Rocketman' Nagy",
                    "One Who Obeys Precepts",
                    "The Retainer Of Ergo",
                    "Outzone Supplier",
                    "Hollow Island Weed",
                    "Sheila Marlene",
                    "Unicorn Advance Sentry",
                    "Unicorn Technician",
                    "Basic Tools Merchant",
                    "Container Supplier",
                    "Basic Quality Pharmacist",
                    "Basic Quality Armorer",
                    "Basic Quality Weaponsdealer",
                    "Tailor",
                    "Unicorn Commander Rufus",
                    "Ergo, Inferno Guardian of Shadows",
                    "Unicorn Trooper",
                    "Unicorn Squadleader",
                    "Rookie Alien Hunter",
                    "Unicorn Service Tower Alpha",
                    "Unicorn Service Tower Delta",
                    "Unicorn Service Tower Gamma",
                    "Sean Powell",
                    "Xan Spirit",
                    "Unicorn Guard",
                    "Essence Fragment",
                    "Scalding Flames",
                    "Guide",
                    "Searing Flame",
                    "Searing Flames",
                    "Guard",
                    "Awakened Xan",
                    "Fourth Watch Down",
                    "Mortar Bombardment",
                    "Dogmatic Pestilence",
                    "Flame of the Immortal One",
                    "Calan-Cur",
                    "Pulsating Ice Obelisk",
                    "Unnatural Ice",
                    "Buckethead Technodealer",
                    "Zix"
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
