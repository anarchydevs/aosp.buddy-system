using AOSharp.Common.Unmanaged.Imports;
using AOSharp.Core;
using AOSharp.Core.UI;
using LeBuddy.IPCMessages;
using System;
using System.Linq;

namespace LeBuddy
{
    public class IdleState : IState
    {
        public static TeamMember selectedMember = null;
        private static Random rand = new Random();

        private static double _time;

        public IState GetNextState()
        {
            if (LeBuddy._settings["Enable"].AsBool())
            {
                bool missionExists = Mission.List.Exists(x => x.DisplayName.Contains("Infiltrate the alien ships!"));

                if (Playfield.ModelIdentity.Instance == Constants.UnicornOutpost)
                {

                    if (!missionExists
                        && DynelManager.LocalPlayer.Identity == LeBuddy.Leader
                        && !Team.Members.Any(c => c.Character == null)
                        && Extensions.CanProceed())
                        return new GrabMissionState();

                    if (Team.IsInTeam && selectedMember == null && DynelManager.LocalPlayer.Identity == LeBuddy.Leader
                     && missionExists
                     && !Team.Members.Any(c => c.Character == null))
                    {
                        int randomIndex = rand.Next(Team.Members.Count);
                        selectedMember = Team.Members[randomIndex];

                        if (selectedMember != null)
                        {
                            LeBuddy.IPCChannel.Broadcast(new SelectedMemberUpdateMessage()
                            { SelectedMemberIdentity = selectedMember.Identity });
                        }
                    }

                    if (selectedMember != null && missionExists
                       && DynelManager.LocalPlayer.Identity == selectedMember.Identity)
                    {
                        return new EnterState();
                    }

                    if (Team.Members.Count(c => c.Character == null) > 1
                        && missionExists)
                    {
                        return new EnterState();
                    }
                }

                if (Playfield.IsDungeon)
                {
                    if (DynelManager.LocalPlayer.Room.Name == "Mothership_bossroom")
                    {
                        return new BossRoomState();
                    }
                    else if (missionExists)
                    {
                        return new PathState();
                    }
                    else
                    {
                        return new ButtonExitState();
                    }
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("IdleState");

            selectedMember = null;
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit IdleState");
        }

        public void Tick()
        {
            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._reclaim) < 5)
            {
                LeBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._reformArea);
            }
        }
    }
}
