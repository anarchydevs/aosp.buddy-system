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

namespace LeBuddy
{

    public class EnterState : IState
    {
        private static double _time;

        public static bool NavGenSuccessful;

        private bool _destinationSet = false;

        public IState GetNextState()
        {
            
                if (!LeBuddy._settings["Enable"].AsBool())
                //|| !Mission.List.Exists(x => x.DisplayName.Contains("Infiltrate the alien ships!")))
                    return new IdleState();


                if (Mission.List.Exists(x => x.DisplayName.Contains("Infiltrate the alien ships!")
                && Playfield.IsDungeon))
                {
                    if (IdleState.selectedMember != null)
                    {
                        if (DynelManager.LocalPlayer.Identity == IdleState.selectedMember.Identity)
                        {
                            if (!NavGenSuccessful)
                                return new NavGenState();
                            if (NavGenSuccessful && !Team.Members.Any(c => c.Character == null))
                                return new PathState();
                        }
                    }

                    if (DynelManager.LocalPlayer.Identity != IdleState.selectedMember.Identity
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

            LeBuddy._exitDoor = Playfield.Doors
           .OrderBy(c => c.DistanceFrom(DynelManager.LocalPlayer))
           .FirstOrDefault();

        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Playfield.ModelIdentity.Instance == Constants.UnicornOutpost
            && Extensions.CanProceed()
            && Mission.List.Exists(x => x.DisplayName.Contains("Infiltrate the alien ships!")))
            {
                if (!_destinationSet)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entranceStart) > 1
                        && !LeBuddy.NavMeshMovementController.IsNavigating)
                    {
                        LeBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._entranceStart);
                    }
                    else
                    {
                        _destinationSet = true; // Set the flag to true when we reach _entranceStart.
                        LeBuddy.NavMeshMovementController.AppendNavMeshDestination(Constants._entranceEnd);
                    }
                }
                else
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entranceEnd) > 1
                        && !LeBuddy.NavMeshMovementController.IsNavigating)
                    {
                        LeBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._entranceEnd);
                    }
                    else
                    {
                        _destinationSet = false; // Set the flag to false when we reach _entranceEnd.
                        LeBuddy.NavMeshMovementController.AppendNavMeshDestination(Constants._entranceStart);
                    }
                }
            }

            PlayshiftingFailed();
        }
        private void PlayshiftingFailed()
        {
            Network.ChatMessageReceived += (s, msg) =>
            {
                if (msg.PacketType != ChatMessageType.VicinityMessage)
                    return;

                var npcMsg = (VicinityMessage)msg;

                string[] triggerMsg = new string[1] { "Playshifting failed: The server was unable to start the mission building."};

                if (triggerMsg.Any(x => npcMsg.Text.Contains(x)))
                {
                    if (Mission.List.Exists(x => x.DisplayName.Contains("Infiltrate the alien ships!")))
                    {
                        foreach (Mission mission in Mission.List)
                            if (mission.DisplayName.Contains("Infiltrate the alien ships!"))
                                mission.Delete();
                    }

                }
            };
        }
       
    }
}