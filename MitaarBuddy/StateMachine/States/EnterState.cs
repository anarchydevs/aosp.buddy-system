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

namespace MitaarBuddy
{
    public class EnterState : IState
    {
        private const int MinWait = 8;
        private const int MaxWait = 10;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.MitaarId)
            {
                if (MitaarBuddy.DifficultySelection.Easy == (MitaarBuddy.DifficultySelection)MitaarBuddy._settings["DifficultySelection"].AsInt32())
                    if (MitaarBuddy._died || (!MitaarBuddy._died && !Team.Members.Any(c => c.Character == null)))
                        return new EasyState();

                if (MitaarBuddy.DifficultySelection.Medium == (MitaarBuddy.DifficultySelection)MitaarBuddy._settings["DifficultySelection"].AsInt32())
                    if (MitaarBuddy._died || (!MitaarBuddy._died && !Team.Members.Any(c => c.Character == null)))
                        return new MediumState();

                if (MitaarBuddy.DifficultySelection.Hardcore == (MitaarBuddy.DifficultySelection)MitaarBuddy._settings["DifficultySelection"].AsInt32())
                    if (MitaarBuddy._died || (!MitaarBuddy._died && !Team.Members.Any(c => c.Character == null)))
                        return new HardcoreState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            if (Extensions.CanProceed())
            {
                Chat.WriteLine("Entering Mitaar");

                if (DynelManager.LocalPlayer.Identity == MitaarBuddy.Leader)
                {
                    Task.Delay(2 * 1000).ContinueWith(x =>
                    {
                        MovementController.Instance.SetDestination(Constants._entrance);
                    }, _cancellationToken.Token);
                }
                else
                {
                    int randomWait = Extensions.Next(MinWait, MaxWait);
                    Chat.WriteLine($"Idling for {randomWait} seconds..");

                    Task.Delay(randomWait * 1000).ContinueWith(x =>
                    {
                        MovementController.Instance.SetDestination(Constants._entrance);
                    }, _cancellationToken.Token);
                }
            }
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("EnterSectorState::OnStateExit");

            _cancellationToken.Cancel();
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }
            
        }
    }
}