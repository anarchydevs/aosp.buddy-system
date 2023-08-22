using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using CityBuddy.IPCMessages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages;
using System;
using System.Linq;

namespace CityBuddy
{
    public class CityAttackState : IState
    {

        public static TeamMember selectedMember = null;

        private static Random rand = new Random();

        private SimpleChar _target;
        private Dynel shipentrance;

        public IState GetNextState()
        {
            Corpse _bossCorpse = DynelManager.Corpses
                    .Where(c => c.Name.Contains("General"))
               .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
               .FirstOrDefault();

            if (!CityBuddy._settings["Toggle"].AsBool())
                return new IdleState();

            if (DynelManager.LocalPlayer.Identity == CityBuddy.Leader
                && !DynelManager.NPCs.Any(c => c.Health > 0)
                && (CityController.CloakState == CloakStatus.Unknown || CityController.CanToggleCloak()))
            {
                return new CityControllerState();
            }

            if (_bossCorpse != null) 
            { 
                return new BossLootState(); 
            }

            return null;
        }

        public void OnStateEnter()
        {
            if (Playfield.ModelIdentity.Instance == CityBuddy.MontroyalCity)
            {
                MovementController.Instance.SetDestination(CityBuddy._montroyalGaurdPos);
            }

            //Chat.WriteLine("City state");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("Exit city state");
        }

        public void Tick()
        {
            try
            {
                shipentrance = DynelManager.AllDynels.Where(c => c.Name == "Door").FirstOrDefault();

                _target = DynelManager.NPCs.Where(c => c.Health > 0 && c.DistanceFrom(DynelManager.LocalPlayer) < 40f)
                    .OrderByDescending(c => c.Name.Contains("Hacker"))
                    .FirstOrDefault();

                Corpse _corpse = DynelManager.Corpses
                     .Where(c => !c.Name.Contains("General"))
                     .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                     .FirstOrDefault();


                if (_target != null)
                {
                    if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) < 10f)
                    {
                        if (DynelManager.LocalPlayer.FightingTarget == null
                            && !DynelManager.LocalPlayer.IsAttacking
                            && !DynelManager.LocalPlayer.IsAttackPending
                            && _target.IsInLineOfSight)
                        {
                            DynelManager.LocalPlayer.Attack(_target);
                        }
                    }
                    else if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 2f)
                    {
                        MovementController.Instance.SetDestination(_target.Position);
                    }
                }
                //else if (_corpse != null && _target == null)
                //{
                //    if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpse.Position) > 5)
                //    {
                //        MovementController.Instance.SetDestination(_corpse.Position);
                //    }
                //}
                else if (shipentrance == null)
                {
                    if (Playfield.ModelIdentity.Instance == CityBuddy.MontroyalCity)
                    {
                        if (DynelManager.LocalPlayer.Position.Distance2DFrom(CityBuddy._montroyalGaurdPos) > 10)
                        { MovementController.Instance.SetDestination(CityBuddy._montroyalGaurdPos); }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + CityBuddy.GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != CityBuddy.previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    CityBuddy.previousErrorMessage = errorMessage;
                }
            }
        }
    }
}
