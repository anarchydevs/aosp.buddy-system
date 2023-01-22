using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using NavmeshMovementController;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace InfBuddy
{
    public class MoveToQuestGiverState : IState
    {
        private const int _minWait = 2;
        private const int _maxWait = 4;

        private static float _entropy = 1.34f;

        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (!InfBuddy.NavMeshMovementController.IsNavigating && Extensions.IsAtYutto())
                return new GrabMissionState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("MoveToQuestGiverState::OnStateEnter");

            if (!Extensions.IsAtYutto())
            {
                Vector3 randoPos = Constants.QuestGiverPos;
                randoPos.AddRandomness((int)_entropy);

                int randomWait = Extensions.Next(_minWait, _maxWait);

                if (DynelManager.LocalPlayer.Identity == InfBuddy.Leader
                    || InfBuddy._settings["Merge"].AsBool())
                    randomWait = 1;

                Chat.WriteLine($"Idling for {randomWait} seconds..");

                Task.Delay(randomWait * 1000).ContinueWith(x =>
                {
                    InfBuddy.NavMeshMovementController.SetNavMeshDestination(randoPos);
                    InfBuddy._stateTimeOut = Time.NormalTime;
                }, _cancellationToken.Token);
            }
        }

        public void OnStateExit()
        {
            Chat.WriteLine("MoveToQuestGiverState::OnStateExit");

            _cancellationToken.Cancel();
        }

        public void Tick()
        {
            if (!Extensions.IsAtYutto() && InfBuddy.NavMeshMovementController.IsNavigating
                && Time.NormalTime > InfBuddy._stateTimeOut + 40f)
            {
                Vector3 randoPos = Constants.QuestGiverPos;
                randoPos.AddRandomness((int)_entropy);

                InfBuddy._stateTimeOut = Time.NormalTime;

                InfBuddy.NavMeshMovementController.Halt();
                //DynelManager.LocalPlayer.Position = new Vector3(DynelManager.LocalPlayer.Position.X, DynelManager.LocalPlayer.Position.Y, DynelManager.LocalPlayer.Position.Z - 4f);
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(2743.0f, 24.6f, 3312.0f));
                InfBuddy.NavMeshMovementController.AppendNavMeshDestination(randoPos);
            }
        }
    }
}
