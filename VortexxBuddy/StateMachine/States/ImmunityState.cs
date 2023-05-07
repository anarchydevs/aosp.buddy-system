using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace VortexxBuddy
{
    public class ImmunityState : IState
    {

        private static SimpleChar _vortexx;
        private static SimpleChar _releasedSpirit;

        private static double _timer;

        public IState GetNextState()
        {
            _releasedSpirit = DynelManager.NPCs
                  .Where(c => c.Health > 0
                         && c.Name.Contains("Released Spirit"))
                     .FirstOrDefault();

            if (Playfield.ModelIdentity.Instance == Constants.VortexxId
                 && _releasedSpirit == null)
                return new FightState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("ImmunityState");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit ImmunityState");
        }

        public void Tick()
        {
            if (!Team.IsInTeam || Game.IsZoning) { return; }

            foreach (TeamMember member in Team.Members)
            {
                if (!ReformState._teamCache.Contains(member.Identity))
                    ReformState._teamCache.Add(member.Identity);
            }

            if (Playfield.ModelIdentity.Instance == Constants.VortexxId)
            {

                _vortexx = DynelManager.NPCs
                 .Where(c => c.Health > 0
                  && c.Name.Contains("Ground Chief Vortexx"))
                  .FirstOrDefault();

                _releasedSpirit = DynelManager.NPCs
                   .Where(c => c.Health > 0
                          && c.Name.Contains("Released Spirit"))
                      .FirstOrDefault();

               
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPodium) > 5f
                    && !MovementController.Instance.IsNavigating)
                    VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._centerPodium);

                if (VortexxBuddy._settings["Immunity"].AsBool() && _releasedSpirit != null)
                    if (DynelManager.LocalPlayer.FightingTarget != null
                       && DynelManager.LocalPlayer.FightingTarget.Name == _vortexx.Name)
                    {
                        DynelManager.LocalPlayer.StopAttack();
                    }

                if (_releasedSpirit != null)
                {
                    if (_releasedSpirit.Position.DistanceFrom(Constants._redPodium) < 5)
                    {
                        if (!VortexxBuddy._red)
                        {
                            Task.Factory.StartNew(async () =>{
                                Item red = Inventory.Items.Where(x => ImmunityCrystals.BloodRedNotumCrystal.Contains(x.Id)).FirstOrDefault();

                                await Task.Delay(10000);

                                if (!Item.HasPendingUse)
                                    red.Use();

                            });
                        }
                        VortexxBuddy._red = true;
                    }

                    if (_releasedSpirit.Position.DistanceFrom(Constants._greenPodium) < 5)
                    {
                        if (!VortexxBuddy._green)
                        {
                            Task.Factory.StartNew(async () => {
                                Item green = Inventory.Items.Where(x => ImmunityCrystals.PulsatingGreenNotumCrystal.Contains(x.Id)).FirstOrDefault();

                                await Task.Delay(10000);

                                if (!Item.HasPendingUse)
                                    green.Use();

                            });
                        }
                        VortexxBuddy._green = true;
                    }

                    if (_releasedSpirit.Position.DistanceFrom(Constants._yellowPodium) < 5)
                    {
                        if (!VortexxBuddy._yellow)
                        {
                            Task.Factory.StartNew(async () => {
                                Item yellow = Inventory.Items.Where(x => ImmunityCrystals.GoldenNotumCrystal.Contains(x.Id)).FirstOrDefault(); 

                                await Task.Delay(10000);

                                if (!Item.HasPendingUse)
                                    yellow.Use();

                            });
                        }
                        VortexxBuddy._yellow = true;
                    }

                    if (_releasedSpirit.Position.DistanceFrom(Constants._bluePodium) < 5)
                    {
                        if (!VortexxBuddy._blue)
                        {
                            Task.Factory.StartNew(async () => {
                                Item blue = Inventory.Items.Where(x => ImmunityCrystals.CobaltBlueNotumCrystal.Contains(x.Id)).FirstOrDefault();

                                await Task.Delay(10000);

                                if (!Item.HasPendingUse)
                                    blue.Use();

                            });
                        }
                        VortexxBuddy._blue = true;
                    }

                   
                }
            }
        }

        public static class ImmunityCrystals
        {
            public static readonly int[] BloodRedNotumCrystal = {280581};
            public static readonly int[] PulsatingGreenNotumCrystal = {280585};
            public static readonly int[] GoldenNotumCrystal = {280586};
            public static readonly int[] CobaltBlueNotumCrystal = {280584};

           


        }
    }
}