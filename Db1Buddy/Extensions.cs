using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using static Db1Buddy.Db1Buddy;

namespace Db1Buddy
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

        public static bool Debuffed()
        {
            return DynelManager.LocalPlayer.Buffs.Contains(267283)
                || DynelManager.LocalPlayer.Buffs.Contains(280469)
                || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Cocoon) // coon
                || DynelManager.LocalPlayer.Buffs.Contains(280470)
                || DynelManager.LocalPlayer.Buffs.Contains(280488);
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

        public static bool HasDied()
        {
            return Playfield.ModelIdentity.Instance == Constants.PWId
                        && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) > 10f;
        }
        public static bool TimedOut(double _time, float _timeOut)
        {
            return Team.IsInTeam && Time.NormalTime > _time + _timeOut;
        }

        public static bool CanProceed()
        {
            return DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65
                && DynelManager.LocalPlayer.GetStat(Stat.TemporarySkillReduction) <= 1
                && DynelManager.LocalPlayer.MovementState != MovementState.Sit
                && Spell.List.Any(c => c.IsReady)
                && !Spell.HasPendingCast
                && !Debuffed();
        }

        public static bool IsNull(SimpleChar _target)
        {
            return _target == null
                || _target?.IsPet == true
                || _target?.IsValid == false
                || _target?.Health == 0;
        }
    }
}
