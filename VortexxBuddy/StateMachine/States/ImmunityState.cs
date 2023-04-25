using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace VortexxBuddy
{
    public class ImmunityState : IState
    {

        private static SimpleChar _vortexx;
        private static SimpleChar _releasedSpirit;

        private static bool _red = false;
        private static bool _green = false;
        private static bool _yellow = false;
        private static bool _blue = false;

        public IState GetNextState()
        {

            if (Playfield.ModelIdentity.Instance == Constants.VortexxId
                 && _vortexx != null)
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

                //if(_releasedSpirit != null)
                //{
                //    if (_releasedSpirit.Position.DistanceFrom(Constants._redPodium) <5)
                //    {
                //        if (_releasedSpirit.IsFalling)
                //            Item.Use(_releasedSpirit, ImmunityCrystals.BloodRedNotumCrystal);
                //    }

                //    if (_releasedSpirit.Position.DistanceFrom(Constants._greenPodium) < 5)
                //    {
                //        if (_releasedSpirit.IsFalling)
                //            Item.Use(_releasedSpirit,ImmunityCrystals.PulsatingGreenNotumCrystal);
                //    }

                //    if (_releasedSpirit.Position.DistanceFrom(Constants._yellowPodium) < 5)
                //    {
                //        if (_releasedSpirit.IsFalling)
                //            Item.Use(_releasedSpirit,ImmunityCrystals.GoldenNotumCrystal);
                //    }

                //    if (_releasedSpirit.Position.DistanceFrom(Constants._bluePodium) < 5)
                //    {
                //        if (_releasedSpirit.IsFalling)
                //            Item.Use(_releasedSpirit,ImmunityCrystals.CobaltBlueNotumCrystal);
                //    }
                //}

            }
        }

        public static class ImmunityCrystals
        {
            public const int BloodRedNotumCrystal = 280581;
            public const int PulsatingGreenNotumCrystal = 280585;
            public const int GoldenNotumCrystal = 280586;
            public const int CobaltBlueNotumCrystal = 280584;

        }
    }
}