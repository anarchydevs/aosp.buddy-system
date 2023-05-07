using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Threading;
using System.Threading.Tasks;


namespace InfBuddy
{
    public class MoveToQuestGiverState : IState
    {
        private const int _minWait = 2;
        private const int _maxWait = 4;

        private static float _entropy = 1.34f;

        private static bool _init = false;

        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (!InfBuddy.NavMeshMovementController.IsNavigating && Extensions.IsAtYutto())
                return new GrabMissionState();

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return new SitState();

            return null;
        }

        public void OnStateEnter()
        {
           //Chat.WriteLine("MoveToQuestGiverState::OnStateEnter");

            if (!Extensions.IsAtYutto())
            {
                Vector3 randoPos = Constants.QuestGiverPos;
                randoPos.AddRandomness((int)_entropy);

                int randomWait = Extensions.Next(_minWait, _maxWait);

                if (DynelManager.LocalPlayer.Identity == InfBuddy.Leader
                    || InfBuddy._settings["Merge"].AsBool())
                    randomWait = 1;

                //Chat.WriteLine($"Idling for {randomWait} seconds..");

                Task.Delay(randomWait * 1000).ContinueWith(x =>
                {
                    InfBuddy._stateTimeOut = Time.NormalTime;
                    _init = true;

                    InfBuddy.NavMeshMovementController.SetNavMeshDestination(randoPos);
                }, _cancellationToken.Token);
            }
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("MoveToQuestGiverState::OnStateExit");

            _cancellationToken.Cancel();
            _init = false;
        }

        public void Tick()
        {
            if (Game.IsZoning || !Team.IsInTeam) { return; }

            if (!Extensions.IsAtYutto()
                && _init
                && InfBuddy.NavMeshMovementController.IsNavigating
                && Time.NormalTime > InfBuddy._stateTimeOut + 40f)
            {
                Vector3 randoPos = Constants.QuestGiverPos;
                randoPos.AddRandomness((int)_entropy);

                InfBuddy._stateTimeOut = Time.NormalTime;

                InfBuddy.NavMeshMovementController.Halt();
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(2743.0f, 24.6f, 3312.0f));
                InfBuddy.NavMeshMovementController.AppendNavMeshDestination(randoPos);
            }
        }
    }
}
