using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using CityBuddy.IPCMessages;
using System;
using System.Linq;

namespace CityBuddy
{
    public class WaitForShipState : IState
    {

        public static TeamMember selectedMember = null;

        private static Random rand = new Random();

        private Dynel shipentrance;

        public IState GetNextState()
        {
            if (!CityBuddy._settings["Toggle"].AsBool())
                return new IdleState();

            shipentrance = DynelManager.AllDynels.Where(c => c.Name == "Door").FirstOrDefault();

            if (shipentrance != null)
            {
                if (Team.IsInTeam && selectedMember == null && DynelManager.LocalPlayer.Identity == CityBuddy.Leader
                && !Team.Members.Any(c => c.Character == null))
                {
                    int randomIndex = rand.Next(Team.Members.Count);
                    selectedMember = Team.Members[randomIndex];

                    if (selectedMember != null)
                    {
                        CityBuddy.IPCChannel.Broadcast(new SelectedMemberUpdateMessage()
                        { SelectedMemberIdentity = selectedMember.Identity });
                    }
                }

                if (selectedMember != null && DynelManager.LocalPlayer.Identity == selectedMember.Identity)
                {
                    return new EnterState();
                }

                if (Team.Members.Count(c => c.Character == null) > 1)
                {
                    return new EnterState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {

            Chat.WriteLine("Waiting for ship.");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("Exit city attack state");
        }

        public void Tick()
        {
            
        }
    }
}
