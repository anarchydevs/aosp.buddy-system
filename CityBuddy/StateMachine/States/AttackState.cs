using AOSharp.Core;
using AOSharp.Core.Movement;
using System.Linq;

namespace CityBuddy
{
    public class AttackState : IState
    {
        public IState GetNextState()
        {
            if (Team.IsLeader
                && Time.NormalTime > CityBuddy._cloakTime + 3660f
                && !DynelManager.NPCs.Any(c => c.Health > 0))
            {
                return new ToggleState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("AttackState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("AttackState::OnStateExit");
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
                && !Extensions.InCombat()
                && MovementController.Instance.IsNavigating == false
                && DynelManager.LocalPlayer.Position.DistanceFrom(_genCorpse.Position) > 2f)
            {
                MovementController.Instance.SetDestination(_genCorpse.Position);
            }

            if (_alien != null)
            {
                //Extensions.HandlePathing(_alien);

                if (_alien.IsInAttackRange() && DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                {
                    CityBuddy._combatTime = Time.NormalTime;
                    DynelManager.LocalPlayer.Attack(_alien);
                }
            }

            if (Time.NormalTime > CityBuddy._combatTime + 7f
                && _genCorpse == null
                && !Extensions.InCombat()
                && MovementController.Instance.IsNavigating == false
                && DynelManager.LocalPlayer.Position.DistanceFrom(CityBuddy.ParkPos) > 2f)
            {
                MovementController.Instance.SetDestination(CityBuddy.ParkPos);
            }
        }
    }
}
