using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using System.Linq;
using System.Threading.Tasks;

namespace CityBuddy
{
    public class ToggleState : IState
    {
        public static bool _init = false;
        public static bool _toggled = false;


        public IState GetNextState()
        {
            if (_toggled == true)
                return new AttackState();

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("PullState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("PullState::OnStateExit");
            _init = false;
            _toggled = false;
        }

        public void Tick()
        {
            Dynel citycontroller = DynelManager.AllDynels
                .Where(c => c.Name == "City Controller")
                .FirstOrDefault();

            if (citycontroller != null)
            {
                if (citycontroller?.DistanceFrom(DynelManager.LocalPlayer) < 5f
                    && !_init)
                {
                    MovementController.Instance.Halt();
                    _init = true;
                    Logic(citycontroller);
                }
                else if (MovementController.Instance.IsNavigating == false)
                    MovementController.Instance.SetDestination(citycontroller.Position);
            }
        }

        private static void Logic(Dynel cc)
        {
            Task.Factory.StartNew(
                async () =>
                {
                    await Task.Delay(3000);
                    cc.Use();
                    await Task.Delay(2000);
                    if (Inventory.Find(257110, out Item cru))
                        cru.UseOn(cc.Identity);
                    await Task.Delay(3000);
                    Extensions.ToggleCloak();
                    await Task.Delay(1500);
                    MovementController.Instance.SetDestination(CityBuddy.ParkPos);
                    _toggled = true;
                });
        }
    }
}
