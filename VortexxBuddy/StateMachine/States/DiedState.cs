using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;

namespace VortexxBuddy
{
    public class DiedState : IState
    {
        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._startPos) < 20.0f
                && Extensions.CanProceed()
                && !Team.Members.Any(c => c.Character == null))
                return new EnterState();

            return null;
        }

        public void OnStateEnter()
        {
        }

        public void OnStateExit()
        {
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) > 20.0f
                && Playfield.ModelIdentity.Instance == Constants.XanHubId)
            {
                if (DynelManager.LocalPlayer.MovementState == MovementState.Sit
                    && DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65)
                    VortexxBuddy.NavMeshMovementController.SetMovement(MovementAction.LeaveSit);

                if (DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65
                    && DynelManager.LocalPlayer.GetStat(Stat.TemporarySkillReduction) <= 1
                    && DynelManager.LocalPlayer.MovementState != MovementState.Sit
                    && Playfield.ModelIdentity.Instance == Constants.XanHubId && !VortexxBuddy.NavMeshMovementController.IsNavigating)
                    VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPos);
            }
        }
    }
}