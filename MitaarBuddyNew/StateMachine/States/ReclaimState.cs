using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using MitaarBuddy.IPCMessages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages.OrgServerMessages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MitaarBuddy
{
    public class ReclaimState : IState
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.APFHubId
                && Extensions.CanProceed())
                return new EnterState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("DiedState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("DiedState::OnStateExit");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

           
            if (!MitaarBuddy.NavMeshMovementController.IsNavigating)
            {
                Task.Delay(2 * 1000).ContinueWith(x =>
                {
                    MitaarBuddy.NavMeshMovementController.SetPath(Constants._pathToMitaar);
                }, _cancellationToken.Token);
            }
        }
    }
}