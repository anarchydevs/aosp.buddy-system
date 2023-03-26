using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class EnterState : IState
    {
        private const int MinWait = 8;
        private const int MaxWait = 10;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.Outside)
            {
                DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._entrancePos);
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("EnterSectorState::OnStateEnter");

            if (DynelManager.LocalPlayer.Identity == DB2Buddy.Leader)
            {
                Task.Delay(2 * 1000).ContinueWith(x =>
                {
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._entrancePos);
                }, _cancellationToken.Token);
            }
        }

        public void OnStateExit()
        {
            Chat.WriteLine("EnterSectorState::OnStateExit");

            _cancellationToken.Cancel();
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }
        }
    }
}