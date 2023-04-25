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


namespace VortexxBuddy
{
    public class ImmunityState : IState
    {

        private static SimpleChar _vortexx;
        private static SimpleChar _releasedSpirit;

        private static double _timer;

        public IState GetNextState()
        {

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
                  && c.Name.Contains("Ground Chief Vortexx")
                  && !c.Name.Contains("Remains of"))
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
                    if (_releasedSpirit.Position.DistanceFrom(Constants._redPodium) < 3)
                    {
                        if (Time.NormalTime > _timer + 10)
                        {
                            Item red = Inventory.Items.Where(x => ImmunityCrystals.BloodRedNotumCrystal.Contains(x.Id)).FirstOrDefault();
                            red.Use();

                            _timer = Time.NormalTime;
                        }
                    }

                    if (_releasedSpirit.Position.DistanceFrom(Constants._greenPodium) < 3)
                    {
                        if (Time.NormalTime > _timer + 10)
                        {
                            Item green = Inventory.Items.Where(x => ImmunityCrystals.PulsatingGreenNotumCrystal.Contains(x.Id)).FirstOrDefault();
                            green.Use();

                            _timer = Time.NormalTime;
                        }
                    }

                    if (_releasedSpirit.Position.DistanceFrom(Constants._yellowPodium) < 3)
                    {
                        if (Time.NormalTime > _timer + 10)
                        {
                            Item yellow = Inventory.Items.Where(x => ImmunityCrystals.GoldenNotumCrystal.Contains(x.Id)).FirstOrDefault();
                            yellow.Use();

                            _timer = Time.NormalTime;
                        }
                    }

                    if (_releasedSpirit.Position.DistanceFrom(Constants._bluePodium) < 3)
                    {
                        if (Time.NormalTime > _timer + 10)
                        {
                            Item blue = Inventory.Items.Where(x => ImmunityCrystals.CobaltBlueNotumCrystal.Contains(x.Id)).FirstOrDefault();
                            blue.Use();

                            _timer = Time.NormalTime;
                        }
                    }
                }
            }
        }

        public static class ImmunityCrystals
        {
            public static readonly int[] BloodRedNotumCrystal = { 280581};
            public static readonly int[] PulsatingGreenNotumCrystal = {280585};
            public static readonly int[] GoldenNotumCrystal = { 280586 };
            public static readonly int[] CobaltBlueNotumCrystal = { 280584 };

           


        }
    }
}