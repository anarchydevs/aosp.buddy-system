using AOSharp.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AXPBuddy
{
    public class EnterAPFHubState : IState
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.APFHubId
                && Extensions.CanProceed())
                return new EnterSectorState();

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("DiedState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("DiedState::OnStateExit");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            Dynel Lever = DynelManager.AllDynels
                .Where(c => c.Name == "A Lever"
                    && c.DistanceFrom(DynelManager.LocalPlayer) < 6f)
                .FirstOrDefault();

            if (Lever != null)
            {
                Lever.Use();
                _cancellationToken.Cancel();
            }
            else if (!AXPBuddy.NavMeshMovementController.IsNavigating)
            {
                Task.Delay(2 * 1000).ContinueWith(x =>
                {
                    AXPBuddy.NavMeshMovementController.SetPath(Constants.UnicornHubPath);
                }, _cancellationToken.Token);
            }
        }
    }
}