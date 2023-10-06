using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.UI;
using org.critterai.nav;
using System;
using System.Linq;
using LeBuddy.IPCMessages;

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

                if (Playfield.ModelIdentity.Instance == Constants.UnicornOutpost)
                {

                    if (!Mission.List.Exists(x => x.DisplayName.Contains("Infiltrate the alien ships!"))
                        && DynelManager.LocalPlayer.Identity == LeBuddy.Leader
                        && !Team.Members.Any(c => c.Character == null)
                        && Extensions.CanProceed())
                        return new GrabMissionState();

                    if (Team.IsInTeam && selectedMember == null && DynelManager.LocalPlayer.Identity == LeBuddy.Leader
                     && Mission.List.Exists(x => x.DisplayName.Contains("Infiltrate the alien ships!"))
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

                    if (selectedMember != null && Mission.List.Exists(x => x.DisplayName.Contains("Infiltrate the alien ships!")
                       && DynelManager.LocalPlayer.Identity == selectedMember.Identity))
                    {
                        return new EnterState();
                    }

                    if (Team.Members.Count(c => c.Character == null) > 1
                        && Mission.List.Exists(x => x.DisplayName.Contains("Infiltrate the alien ships!")))
                    {
                        return new EnterState();
                    }
                }

                if (Playfield.IsDungeon)
                {
                    if (Mission.List.Exists(x => x.DisplayName.Contains("Infiltrate the alien ships!")))
                    {
                        if (DynelManager.LocalPlayer.Room.Name == "Mothership_bossroom")
                            return new BossRoomState();
                        else
                            return new PathState();
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
