using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace ALBBuddy
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

        public static bool TimedOut(double _time, float _timeOut)
        {
            return Team.IsInTeam && Time.NormalTime > _time + _timeOut;
        }

        public static bool CanProceed()
        {
            return DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65
                && DynelManager.LocalPlayer.GetStat(Stat.TemporarySkillReduction) <= 1
                && DynelManager.LocalPlayer.MovementState != MovementState.Sit;
        }

        public static bool IsNull(SimpleChar _target)
        {
            return _target == null
                || _target?.IsPet == true
                || _target?.IsValid == false
                || _target?.Health == 0;
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
    }
}
