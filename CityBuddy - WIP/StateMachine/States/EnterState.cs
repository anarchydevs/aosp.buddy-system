using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CityBuddy
{

    public class EnterState : IState
    {
        private static double _time;

        public static bool NavGenSuccessful;

        private bool _destinationSet = false;

        public IState GetNextState()
        {
            
                if (!CityBuddy._settings["Toggle"].AsBool())
                    return new IdleState();

                if (DynelManager.LocalPlayer.Room.Name == "AI_entrance")
                {
                    if (CityAttackState.selectedMember != null)
                    {
                        if (DynelManager.LocalPlayer.Identity == CityAttackState.selectedMember.Identity)
                        {
                            if (!NavGenSuccessful)
                                return new NavGenState();
                            if (NavGenSuccessful && !Team.Members.Any(c => c.Character == null))
                                return new PathState();
                        }
                    }

                    if (DynelManager.LocalPlayer.Identity != CityAttackState.selectedMember.Identity
                        && Team.Members.Any(c => c.Character != null))
                        return new PathState();
                }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("EnterState");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit EnterState");

        }

        public void Tick()
        {
            try
            {
                if (Game.IsZoning) { return; }

                Dynel shipentrance = DynelManager.AllDynels.Where(c => c.Name == "Door").FirstOrDefault();

                if (shipentrance != null)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(shipentrance.Position) > 1
                        && !MovementController.Instance.IsNavigating)
                    {
                        MovementController.Instance.SetDestination(shipentrance.Position);
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