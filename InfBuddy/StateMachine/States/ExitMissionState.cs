﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InfBuddy
{
    public class ExitMissionState : IState
    {
        private static double _pathTimeOut = Time.NormalTime;

        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public IState GetNextState()
        {
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
            Chat.WriteLine("ExitMissionState::OnStateEnter");

            int _time = 0;

            if (InfBuddy._settings["DoubleReward"].AsBool() && !InfBuddy.DoubleReward)
                _time = 1000;
            else
                _time = 22000;


            Task.Delay(_time).ContinueWith(x =>
            {
                InfBuddy._stateTimeOut = Time.NormalTime;

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
        }

        public void OnStateExit()
        {
            Chat.WriteLine("ExitMissionState::OnStateExit");

            _cancellationToken.Cancel();
        }

        public void Tick()
        {
            if (Time.NormalTime > InfBuddy._stateTimeOut + 15f)
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
