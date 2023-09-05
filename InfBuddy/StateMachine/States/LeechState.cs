using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;

namespace InfBuddy
{
    public class LeechState : IState
    {
        private static bool _missionsLoaded = false;

        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            //if (Extensions.CanExit(_missionsLoaded))
            //    return new ExitMissionState();

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId
                && !Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                return new ExitMissionState();

            if (Playfield.ModelIdentity.Instance == Constants.InfernoId)
                return new IdleState();

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("LeechState::OnStateEnter");

            DynelManager.LocalPlayer.Position = Constants.LeechSpot;
            MovementController.Instance.SetMovement(MovementAction.Update);
            MovementController.Instance.SetMovement(MovementAction.JumpStart);
            MovementController.Instance.SetMovement(MovementAction.Update);
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("LeechState::OnStateExit");

            _missionsLoaded = false;
            DynelManager.LocalPlayer.Position = new Vector3(160.4f, 2.6f, 103.0f);
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (!_missionsLoaded && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                _missionsLoaded = true;
        }
    }
}
