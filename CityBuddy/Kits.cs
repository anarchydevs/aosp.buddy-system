﻿using AOSharp.Core;
using AOSharp.Common.GameData;
using System.Diagnostics;
using System.Linq;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using System.Collections.Generic;


namespace CityBuddy
{
    public class Kits
    {
        //private Stopwatch _kitTimer = new Stopwatch();

        public void SitAndUseKit()
        {
            var localPlayer = DynelManager.LocalPlayer;

            // Check if we should sit and use the kit.
            if ((localPlayer.NanoPercent < 66 || localPlayer.HealthPercent < 66) && !InCombat() && !localPlayer.Cooldowns.ContainsKey(Stat.Treatment) && CanUseSitKit())
            {
                // Sit if not already sitting.
                if (localPlayer.MovementState != MovementState.Sit)
                {
                    MovementController.Instance.SetMovement(MovementAction.SwitchToSit);
                }
                else
                {
                    // Use the kit.
                    UseKit();
                }
            }
            // Check if we should stand.
            else if ((localPlayer.NanoPercent >= 90 && localPlayer.HealthPercent >= 90) || InCombat() || localPlayer.Cooldowns.ContainsKey(Stat.Treatment))
            {
                // Stand up if sitting.
                if (localPlayer.MovementState == MovementState.Sit)
                {
                    MovementController.Instance.SetMovement(MovementAction.LeaveSit);
                }
            }
        }

        public bool CanUseSitKit()
        {
            if (!DynelManager.LocalPlayer.IsAlive || DynelManager.LocalPlayer.IsMoving || Game.IsZoning || InCombat())
            {
                return false;
            }

            List<Item> sitKits = Inventory.FindAll("Health and Nano Recharger").Where(c => c.Id != 297274).ToList();
            if (sitKits.Any())
            {
                return sitKits.OrderBy(x => x.QualityLevel).Any(sitKit => MeetsSkillRequirement(sitKit));
            }

            return Inventory.Find(297274, out Item premSitKit);
        }

        public void UseKit()
        {

            Item kit = Inventory.Items.FirstOrDefault(x => RelevantItems.Kits.Contains(x.Id));
            if (kit != null)
            {
                kit.Use(DynelManager.LocalPlayer, true);
                //_kitTimer.Restart();
            }
        }

        public bool MeetsSkillRequirement(Item sitKit)
        {
            var localPlayer = DynelManager.LocalPlayer;
            int skillReq = sitKit.QualityLevel > 200 ? (sitKit.QualityLevel % 200 * 3) + 1501 : (int)(sitKit.QualityLevel * 7.5f);

            return localPlayer.GetStat(Stat.FirstAid) >= skillReq || localPlayer.GetStat(Stat.Treatment) >= skillReq;
        }

        public static bool InCombat()
        {
            var localPlayer = DynelManager.LocalPlayer;

            if (Team.IsInTeam)
            {
                return DynelManager.Characters
                    .Any(c => c.FightingTarget != null
                        && Team.Members.Select(m => m.Name).Contains(c.FightingTarget.Name));
            }

            return DynelManager.Characters
                    .Any(c => c.FightingTarget != null
                        && c.FightingTarget.Name == localPlayer.Name)
                    || localPlayer.GetStat(Stat.NumFightingOpponents) > 0
                    || Team.IsInCombat()
                    || localPlayer.FightingTarget != null;
        }
    }
    public static class RelevantItems
    {
        public static readonly int[] Kits = {297274, 293296, 291084, 291083, 291082 };
    }
}
