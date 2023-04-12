using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace CityBuddy
{
    public static class Extensions
    {
        public static void AddRandomness(this ref Vector3 pos, int entropy)
        {
            pos.X += Next(-entropy, entropy);
            pos.Z += Next(-entropy, entropy);
        }
        public static int Next(int min, int max)
        {
            if (min >= max)
            {
                throw new ArgumentException("Min value is greater or equals than Max value.");
            }

            byte[] intBytes = new byte[4];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(intBytes);
            }

            return min + Math.Abs(BitConverter.ToInt32(intBytes, 0)) % (max - min + 1);
        }
        public static bool InCombat()
        {
            if (Team.IsInTeam)
            {
                return DynelManager.Characters
                    .Any(c => c.FightingTarget != null && Team.Members.Select(m => m.Name).Contains(c.FightingTarget?.Name));
            }

            return DynelManager.Characters
                    .Any(c => c.FightingTarget != null && c.FightingTarget?.Name == DynelManager.LocalPlayer.Name);
        }

        public static CharacterWieldedWeapon GetWieldedWeapons(SimpleChar local) => (CharacterWieldedWeapon)local.GetStat(Stat.EquippedWeapons);

        public static void HandlePathing(SimpleChar target)
        {
            if (CityBuddy.NavMeshMovementController.IsNavigating && target?.IsInLineOfSight == true)
            {
                if (target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 4f
                    && (GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Melee)
                    || GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.MartialArts)))
                    CityBuddy.NavMeshMovementController.Halt();

                if (target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 11f
                    && GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Ranged))
                    CityBuddy.NavMeshMovementController.Halt();
            }

            if (target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 4f
                && (GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Melee)
                || GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.MartialArts)))
                CityBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)target?.Position);

            if (target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 11f
                && GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Ranged))
                CityBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)target?.Position);
        }

        public static bool CanUseSitKit()
        {
            if (Inventory.Find(297274, out Item premSitKit))
                if (DynelManager.LocalPlayer.Health > 0 && !InCombat()
                                    && !DynelManager.LocalPlayer.IsMoving && !Game.IsZoning) { return true; }

            if (DynelManager.LocalPlayer.IsAlive && !InCombat()
                    && !DynelManager.LocalPlayer.IsMoving && !Game.IsZoning)
            {
                List<Item> sitKits = Inventory.FindAll("Health and Nano Recharger").Where(c => c.Id != 297274).ToList();

                if (!sitKits.Any()) { return false; }

                foreach (Item sitKit in sitKits.OrderBy(x => x.QualityLevel))
                {
                    int skillReq = (sitKit.QualityLevel > 200 ? (sitKit.QualityLevel % 200 * 3) + 1501 : (int)(sitKit.QualityLevel * 7.5f));

                    if (DynelManager.LocalPlayer.GetStat(Stat.FirstAid) >= skillReq || DynelManager.LocalPlayer.GetStat(Stat.Treatment) >= skillReq)
                        return true;
                }
            }

            return false;
        }

        public static void ToggleCloak()
        {
            Chat.WriteLine("Toggling cloak.");
            Network.Send(new ToggleCloakMessage()
            {
                Unknown1 = 49152,
            });
            CityBuddy._cloakTime = Time.NormalTime;
        }

        [Flags]
        public enum CharacterWieldedWeapon
        {
            Fists = 0x0,            // 0x00000000000000000000b Fists / invalid
            MartialArts = 0x01,             // 0x00000000000000000001b martialarts / fists
            Melee = 0x02,             // 0x00000000000000000010b
            Ranged = 0x04,            // 0x00000000000000000100b
            Bow = 0x08,               // 0x00000000000000001000b
            Smg = 0x10,               // 0x00000000000000010000b
            Edged1H = 0x20,           // 0x00000000000000100000b
            Blunt1H = 0x40,           // 0x00000000000001000000b
            Edged2H = 0x80,           // 0x00000000000010000000b
            Blunt2H = 0x100,          // 0x00000000000100000000b
            Piercing = 0x200,         // 0x00000000001000000000b
            Pistol = 0x400,           // 0x00000000010000000000b
            AssaultRifle = 0x800,     // 0x00000000100000000000b
            Rifle = 0x1000,           // 0x00000001000000000000b
            Shotgun = 0x2000,         // 0x00000010000000000000b
            Grenade = 0x8000,     // 0x00000100000000000000b // 0x00001000000000000000b grenade / martial arts
            MeleeEnergy = 0x4000,     // 0x00001000000000000000b // 0x00000100000000000000b
            RangedEnergy = 0x10000,   // 0x00010000000000000000b
            Grenade2 = 0x20000,        // 0x00100000000000000000b
            HeavyWeapons = 0x40000,   // 0x01000000000000000000b
        }
    }
}
