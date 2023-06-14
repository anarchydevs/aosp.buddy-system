using AOSharp.Core;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AXPBuddy
{
    public class EnterAPFHubState : IState
    {
        public IState GetNextState()
        {
            if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f) { return null; }

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
            if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f) { return; }

            Dynel Lever = DynelManager.AllDynels
                .Where(c => c.Name == "A Lever"
                    && c.DistanceFrom(DynelManager.LocalPlayer) < 6f)
                .FirstOrDefault();

            if (Lever != null)
            {
                Lever.Use();
            }
            else if (!AXPBuddy.NavMeshMovementController.IsNavigating)
            {
                Task.Factory.StartNew(
                   async () =>
                   {
                       await Task.Delay(2000);
                       AXPBuddy.NavMeshMovementController.SetPath(Constants.UnicornHubPath);
                   });
            }
        }
    }
}