using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ALBBuddy
{
    public class EnterAlbtraumState : IState
    {
        private const int MinWait = 8;
        private const int MaxWait = 10;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.Albtraum)
            {
                if (!Team.Members.Any(c => c.Character == null))
                    return new PatrolState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            if (DynelManager.LocalPlayer.Identity == ALBBuddy.Leader)
            {
                Task.Delay(2 * 1000).ContinueWith(x =>
                {
                    MovementController.Instance.SetDestination(Constants.EntrancePos);
                }, _cancellationToken.Token);
            }
            else
            {
                int randomWait = Extensions.Next(MinWait, MaxWait);
                Chat.WriteLine($"Idling for {randomWait} seconds..");

                Task.Delay(randomWait * 1000).ContinueWith(x =>
                {
                    MovementController.Instance.SetDestination(Constants.EntrancePos);
                }, _cancellationToken.Token);
            }

        }

        public void OnStateExit()
        {
            _cancellationToken.Cancel();
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }
        }
    }
}