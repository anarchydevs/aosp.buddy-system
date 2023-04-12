using AOSharp.Common.GameData;
using AOSharp.Core;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace InfBuddy
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
        public static bool IsAtYutto()
        {
            return DynelManager.LocalPlayer.Position.DistanceFrom(Constants.QuestGiverPos) < 8.5f;
        }
        public static bool IsAtStarterPos()
        {
            return DynelManager.LocalPlayer.Position.DistanceFrom(Constants.QuestStarterPos) <= 4f;
        }

        public static bool HasDied()
        {
            return Playfield.ModelIdentity.Instance == Constants.OmniPandeGId || Playfield.ModelIdentity.Instance == Constants.ClanPandeGId;
        }
        public static bool TimedOut(double _time, float _timeOut)
        {
            return Team.IsInTeam && Time.NormalTime > _time + _timeOut;
        }

        public static bool CanExit(bool loaded)
        {
            return loaded && !Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri"));
        }

        public static bool IsClear()
        {
            return !DynelManager.NPCs.Any(c => c.FightingTarget?.Name != "Guardian Spirit of Purification");
        }

        public static bool IsNull(SimpleChar _target)
        {
            return _target == null
                || _target?.IsPet == true
                || _target?.IsValid == false
                || _target?.Health == 0
                || _target?.Name == "Guardian Spirit of Purification";
        }

        public static CharacterWieldedWeapon GetWieldedWeapons(SimpleChar local) => (CharacterWieldedWeapon)local.GetStat(Stat.EquippedWeapons);

        public static void HandlePathing(SimpleChar target)
        {
            if (InfBuddy.NavMeshMovementController.IsNavigating && target?.IsInLineOfSight == true)
            {
                if (target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 4f
                    && (GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Melee)
                    || GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.MartialArts)))
                    InfBuddy.NavMeshMovementController.Halt();

                if (target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 11f
                    && GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Ranged))
                    InfBuddy.NavMeshMovementController.Halt();
            }

            if (target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 4f
                && (GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Melee)
                || GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.MartialArts)))
                InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)target?.Position);

            if (target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 11f
                && GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Ranged))
                InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)target?.Position);
        }

        public static bool Rooted()
        {
            return DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Root)
                || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.AOERoot);
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
