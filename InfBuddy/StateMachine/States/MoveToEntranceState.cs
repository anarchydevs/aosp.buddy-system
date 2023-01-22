using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InfBuddy
{
    public class MoveToEntranceState : IState
    {
        private const int _minWait = 5;
        private const int _maxWait = 7;

        private static bool _init = false;

        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId)
            {
                if (InfBuddy.ModeSelection.Leech == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                    return new LeechState();

                if (DynelManager.LocalPlayer.Identity != InfBuddy.Leader && InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                {
                    if (!_init)
                    {
                        InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.QuestStarterPos);
                        _init = true;
                    }

                    if (_init && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.QuestStarterPos) < 10f)
                        return new RoamState();
                }

                if (!_init)
                    return new MoveToQuestStarterState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("MoveToEntranceState::OnStateEnter");

            int randomWait = Extensions.Next(_minWait, _maxWait);

            if (DynelManager.LocalPlayer.Identity == InfBuddy.Leader)
                randomWait = 4;

            Chat.WriteLine($"Idling for {randomWait} seconds..");

            Task.Delay(randomWait * 1000).ContinueWith(x =>
            {
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.EntrancePos);
                InfBuddy.NavMeshMovementController.AppendDestination(Constants.EntranceFinalPos);
                InfBuddy._stateTimeOut = Time.NormalTime;
            }, _cancellationToken.Token);
        }

        public void OnStateExit()
        {
            Chat.WriteLine("MoveToEntranceState::OnStateExit");

            _cancellationToken.Cancel();
            _init = false;
        }

        public void Tick()
        {
            if (Playfield.ModelIdentity.Instance != Constants.InfernoId
                || Game.IsZoning) { return; }

            if (InfBuddy.NavMeshMovementController.IsNavigating
                && Time.NormalTime > InfBuddy._stateTimeOut + 35f)
            {
                InfBuddy._stateTimeOut = Time.NormalTime;

                InfBuddy.NavMeshMovementController.Halt();
                //DynelManager.LocalPlayer.Position = new Vector3(DynelManager.LocalPlayer.Position.X, DynelManager.LocalPlayer.Position.Y, DynelManager.LocalPlayer.Position.Z - 4f);
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(2769.6f, 24.6f, 3319.9f));
                InfBuddy.NavMeshMovementController.AppendNavMeshDestination(Constants.EntrancePos);
                InfBuddy.NavMeshMovementController.AppendDestination(Constants.EntranceFinalPos);
            }
        }
    }
}
