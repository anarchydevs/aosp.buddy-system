﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace AttackBuddy
{
    public static class Extensions
    {
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

        public static bool IsFightingAny(SimpleChar mob)
        {
            if (mob?.FightingTarget == null) { return true; }

            if (mob?.FightingTarget?.Name == "Guardian Spirit of Purification"
                || mob?.FightingTarget?.Name == "Rookie Alien Hunter"
                || mob?.FightingTarget?.Name == "Unicorn Service Tower Alpha"
                || mob?.FightingTarget?.Name == "Unicorn Service Tower Delta"
                || mob?.FightingTarget?.Name == "Unicorn Service Tower Gamma") { return true; }

            if (Team.IsInTeam)
            {
                return Team.Members.Select(m => m.Name).Contains(mob.FightingTarget?.Name)
                   || AttackBuddy._helpers.Contains(mob?.FightingTarget.Name)
                   || (mob?.FightingTarget?.IsPet == true
                        && Team.Members.Select(c => c.Identity.Instance).Any(c => c == mob?.FightingTarget?.PetOwnerId));
            }

            return mob.FightingTarget?.Name == DynelManager.LocalPlayer.Name
                || AttackBuddy._helpers.Contains(mob?.FightingTarget.Name)
                || (mob?.FightingTarget?.IsPet == true
                    && mob.FightingTarget?.PetOwnerId == DynelManager.LocalPlayer.Identity.Instance);
        }

        //public static bool IsFightingAny(SimpleChar target)
        //{
        //    if (Team.IsInTeam)
        //    {
        //        if (target?.FightingTarget == null) { return true; }

        //        if (target?.FightingTarget != null
        //            && (target?.FightingTarget.Name == DynelManager.LocalPlayer.Name
        //                || AttackBuddy._helpers.Contains(target?.FightingTarget.Name)
        //                || Team.Members.Any(c => c.Name == target?.FightingTarget?.Name))) { return true; }

        //        if (target?.FightingTarget?.Name == "Guardian Spirit of Purification"
        //            || target?.FightingTarget?.Name == "Rookie Alien Hunter"
        //            || target?.FightingTarget?.Name == "Unicorn Service Tower Alpha"
        //            || target?.FightingTarget?.Name == "Unicorn Service Tower Delta"
        //            || target?.FightingTarget?.Name == "Unicorn Service Tower Gamma") { return true; }

        //        return false;
        //    }
        //    else
        //    {
        //        if (target?.FightingTarget == null) { return true; }

        //        if (target?.FightingTarget != null
        //            && (target?.FightingTarget.Name == DynelManager.LocalPlayer.Name
        //                || AttackBuddy._helpers.Contains(target?.FightingTarget.Name)
        //                || DynelManager.LocalPlayer.Pets.Any(c => target?.FightingTarget?.Name == c.Character?.Name))) { return true; }

        //        if (target?.FightingTarget?.Name == "Guardian Spirit of Purification"
        //            || target?.FightingTarget?.Name == "Rookie Alien Hunter"
        //            || target?.FightingTarget?.Name == "Unicorn Service Tower Alpha"
        //            || target?.FightingTarget?.Name == "Unicorn Service Tower Delta"
        //            || target?.FightingTarget?.Name == "Unicorn Service Tower Gamma") { return true; }

        //        return false;
        //    }
        //}
        public static SimpleChar GetLeader(Identity leader)
        {
            if (!DynelManager.Players
                .Any(c => c.Identity == leader
                    && c.DistanceFrom(DynelManager.LocalPlayer) < 45f
                    && c.IsValid
                    && !c.Buffs.Contains(280470) && !c.Buffs.Contains(257127) && !c.Buffs.Contains(260301)))
                return DynelManager.LocalPlayer;

            return DynelManager.Players
                .FirstOrDefault(c => c.Identity == leader
                    && c.DistanceFrom(DynelManager.LocalPlayer) < 45f
                    && c.IsValid
                    && !c.Buffs.Contains(280470) && !c.Buffs.Contains(257127) && !c.Buffs.Contains(260301));
        }
        public static bool BossHasCorrespondingBuff(int buff)
        {
            SimpleChar boss = DynelManager.NPCs
                                .Where(c => c.Name == "Kyr'Ozch Technician" && c.Buffs.Contains(buff))
                                .FirstOrDefault();
            if (boss != null)
                return true;
            else
                return false;
        }

        public static bool IsNull(SimpleChar _target)
        {
            return _target == null
                || _target?.IsValid == false
                || _target?.Health == 0
                || _target?.IsInLineOfSight == false;
        }

        public static bool IsBoss(SimpleChar _boss)
        {
            return _boss?.MaxHealth >= 1000000 || _boss?.Name == "Kyr'Ozch Maid" || _boss?.Name == "Kyr'Ozch Technician"
                        || _boss?.Name == "Defense Drone Tower" || _boss?.Name == "Kyr'Ozch Technician";
        }

        public static bool ShouldStopAttack()
        {
            if (DynelManager.LocalPlayer.FightingTarget == null
                || DynelManager.LocalPlayer.IsAttacking
                || DynelManager.LocalPlayer.IsAttackPending) { return false; }

            if (DynelManager.LocalPlayer.FightingTarget?.Buffs.Contains(253953) == true
                //|| DynelManager.LocalPlayer.FightingTarget?.Buffs.Contains(NanoLine.ShovelBuffs) == true
                // || DynelManager.LocalPlayer.FightingTarget?.Buffs.Contains(302745) == true
                || DynelManager.LocalPlayer.FightingTarget?.Buffs.Contains(205607) == true) { return true; }

            return DynelManager.LocalPlayer.FightingTarget?.IsPlayer == true;
        }

        public static bool ShouldTaunt(SimpleChar _target)
        {
            return _target?.IsInLineOfSight == true
                && !DynelManager.LocalPlayer.IsMoving
                && !DynelManager.LocalPlayer.IsAttacking && !DynelManager.LocalPlayer.IsAttackPending;
        }

        public static bool CanAttack()
        {
            return DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttacking && !DynelManager.LocalPlayer.IsAttackPending;
        }
    }
}
