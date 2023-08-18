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
            
                if (!CityBuddy._settings["Enable"].AsBool())
                    return new IdleState();

                if (Playfield.IsDungeon)
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

            CityBuddy._exitDoor = Playfield.Doors
           .OrderBy(c => c.DistanceFrom(DynelManager.LocalPlayer))
           .FirstOrDefault();

        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            Dynel shipntrance = DynelManager.AllDynels.FirstOrDefault(c => c.Name == "Door");

            if (DynelManager.LocalPlayer.Position.DistanceFrom(shipntrance.Position) > 1
                && MovementController.Instance.IsNavigating == false)
            {
                MovementController.Instance.SetDestination(shipntrance.Position);
            }


        }
    }
}