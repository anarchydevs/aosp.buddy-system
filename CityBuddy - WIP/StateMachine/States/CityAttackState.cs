using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using CityBuddy.IPCMessages;
using System;
using System.Linq;

namespace CityBuddy
{
    public class CityAttackState : IState
    {

        public static TeamMember selectedMember = null;
        private static Random rand = new Random();
        Dynel shipntrance = DynelManager.AllDynels.FirstOrDefault(c => c.Name == "Door");

        public IState GetNextState()
        {
            if (DynelManager.LocalPlayer.Identity == CityBuddy.Leader
                && Time.NormalTime > CityBuddy._cloakTime + 3660f
                && !DynelManager.NPCs.Any(c => c.Health > 0))
            {
                return new CityControllerState();
            }

            if (shipntrance != null)
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
            Chat.WriteLine("City attack state");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit city attack staye");
        }

        public void Tick()
        {
            SimpleChar _alien = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.DistanceFrom(DynelManager.LocalPlayer) < 30f)
                .OrderByDescending(c => c.Name.Contains("Hacker"))
                .FirstOrDefault();

            Corpse _genCorpse = DynelManager.Corpses
                .Where(c => c.Name.Contains("General"))
                .FirstOrDefault();

            

            if (_genCorpse != null
                && !CityBuddy.InCombat()
                && MovementController.Instance.IsNavigating == false
                && DynelManager.LocalPlayer.Position.DistanceFrom(_genCorpse.Position) > 2f)
            {
                MovementController.Instance.SetDestination(_genCorpse.Position);
            }

            if (_alien != null)
            {
                //CityBuddy.HandlePathing(_alien);

                if (_alien.IsInAttackRange() && DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                {
                    CityBuddy._combatTime = Time.NormalTime;
                    DynelManager.LocalPlayer.Attack(_alien);
                }
            }

            //if (Time.NormalTime > CityBuddy._combatTime + 7f
            //    && _genCorpse == null
            //    && !CityBuddy.InCombat()
            //    && MovementController.Instance.IsNavigating == false
            //    && DynelManager.LocalPlayer.Position.DistanceFrom(CityBuddy.ParkPos) > 2f)
            //{
            //    MovementController.Instance.SetDestination(CityBuddy.ParkPos);
            //}
        }
    }
}
