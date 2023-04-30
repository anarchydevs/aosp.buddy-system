using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using AOSharp.Common.GameData;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MitaarBuddy
{
    public class EnterState : IState
    {
        private const int MinWait = 3;
        private const int MaxWait = 5;
        private static double _time;
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

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && !Extensions.CanProceed())
                return new IdleState();

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
                        MitaarBuddy.NavMeshMovementController.SetDestination(new Vector3(347.0f, 310.9f, 407.7f).Randomize(2f));
                    },
                    _cancellationToken.Token);
                }
                else
                {
                    int randomWait = Extensions.Next(MinWait, MaxWait);
                    Chat.WriteLine($"Idling for {randomWait} seconds..");

                    Task.Delay(randomWait * 1000).ContinueWith(x =>
                    {
                        MitaarBuddy.NavMeshMovementController.SetDestination(new Vector3(347.0f, 310.9f, 407.7f).Randomize(2f));

                    }, _cancellationToken.Token);
                }
                _time = Time.NormalTime;
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