using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;

namespace AXPBuddy
{
    public class DiedState : IState
    {
        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.UnicornHubId
                && Extensions.CanProceed())
                return new EnterAPFHubState();

            if (Playfield.ModelIdentity.Instance == Constants.APFHubId
                && Extensions.CanProceed())
                return new EnterSectorState();

            return null;
        }

        public void OnStateEnter()
        {
            AXPBuddy._died = true;

            Chat.WriteLine($"DiedState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("DiedState::OnStateExit");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit
                && DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65)
                AXPBuddy.NavMeshMovementController.SetMovement(MovementAction.LeaveSit);

            if (DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65
                && DynelManager.LocalPlayer.GetStat(Stat.TemporarySkillReduction) <= 1
                && DynelManager.LocalPlayer.MovementState != MovementState.Sit
                && Playfield.ModelIdentity.Instance == Constants.XanHubId && !AXPBuddy.NavMeshMovementController.IsNavigating)
                AXPBuddy.NavMeshMovementController.SetDestination(Constants.XanHubPos);
        }
    }
}