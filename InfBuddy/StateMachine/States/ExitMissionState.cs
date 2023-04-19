using AOSharp.Core;
using AOSharp.Core.UI;
using System.Threading;
using System.Threading.Tasks;

namespace InfBuddy
{
    public class ExitMissionState : IState
    {
        private static bool _init = false;

        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.InfernoId)
            {
                if (InfBuddy._settings["DoubleReward"].AsBool() && !InfBuddy.DoubleReward)
                {
                    InfBuddy.DoubleReward = true;
                    return new MoveToQuestGiverState();
                }

                if (InfBuddy.DoubleReward)
                    InfBuddy.DoubleReward = false;

                return new ReformState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("ExitMissionState::OnStateEnter");

            int _time = 0;

            if (InfBuddy._settings["DoubleReward"].AsBool() && !InfBuddy.DoubleReward)
                _time = 1000;
            else
                _time = 5000;


            Task.Delay(_time).ContinueWith(x =>
            {
                InfBuddy._stateTimeOut = Time.NormalTime;
                _init = true;

                if (InfBuddy.ModeSelection.Leech == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                {
                    DynelManager.LocalPlayer.Position = Constants.LeechMissionExit;
                    InfBuddy.NavMeshMovementController.AppendDestination(Constants.ExitFinalPos);
                }
                else
                {
                    InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.ExitPos);
                    InfBuddy.NavMeshMovementController.AppendDestination(Constants.ExitFinalPos);
                }
            }, _cancellationToken.Token);

            foreach (Mission mission in Mission.List)
                if (mission.DisplayName.Contains("The Purification"))
                    mission.Delete();
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("ExitMissionState::OnStateExit");

            _cancellationToken.Cancel();
            _init = false;
        }

        public void Tick()
        {
            if (Game.IsZoning || !Team.IsInTeam) { return; }

            if (InfBuddy.ModeSelection.Leech != (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32()
                && _init
                && Time.NormalTime > InfBuddy._stateTimeOut + 15f)
            {
                InfBuddy._stateTimeOut = Time.NormalTime;

                InfBuddy.NavMeshMovementController.Halt();
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.QuestStarterBeforePos);
                InfBuddy.NavMeshMovementController.AppendDestination(Constants.ExitPos);
                InfBuddy.NavMeshMovementController.AppendDestination(Constants.ExitFinalPos);
            }
        }
    }
}
