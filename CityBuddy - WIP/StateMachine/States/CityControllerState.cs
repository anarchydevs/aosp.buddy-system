using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;

namespace CityBuddy
{
    public class CityControllerState : IState
    {
        public IState GetNextState()
        {
            if (!CityBuddy._settings["Toggle"].AsBool())
                return new IdleState();

            if (CityController.CloakState != CloakStatus.Unknown && CityController.Charge == 100.0f)
            {
                return new CityAttackState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Entering city controller state.");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exiting city controller state.");
        }

        public void Tick()
        {
            Dynel citycontroller = DynelManager.AllDynels
                .Where(c => c.Name == "City Controller")
                .FirstOrDefault();

            if (citycontroller != null)
            {
                if (citycontroller.DistanceFrom(DynelManager.LocalPlayer) < 5f)
                {
                    MovementController.Instance.Halt();
                    ExecuteCityControllerActions(citycontroller);
                }
                else if (citycontroller.DistanceFrom(DynelManager.LocalPlayer) > 5f && !MovementController.Instance.IsNavigating)
                {
                    MovementController.Instance.SetDestination(citycontroller.Position);
                }
            }
        }

        private static void ExecuteCityControllerActions(Dynel cc)
        {
            // If cloak status is unknown, use the city controller to update it
            if (CityController.CloakState == CloakStatus.Unknown)
            {
                CityController.Use();
            }

            // Check and use CRU if the charge is less than 75%
            if (CityController.Charge < 75.0f)
            {
                Chat.WriteLine($"Current city controller charge is {CityController.Charge}%.");

                //if (Inventory.Find(257110, out Item cru))
                //{
                //    cru.UseOn(cc.Identity);
                //}
            }

            // Check if cloak can be toggled
            if (CityController.CanToggleCloak())
            {
                if (CityController.CloakState == CloakStatus.Enabled)
                {
                    // Disable cloak
                    CityController.ToggleCloak();
                }
                else if (CityController.CloakState == CloakStatus.Disabled)
                {
                    // Enable cloak
                    CityController.ToggleCloak();
                }
            }
        }
    }
}



//namespace CityBuddy
//{
//    public class CityControllerState : IState
//    {
//        public static bool _init = false;
//        public static bool _toggled = false;


//        public IState GetNextState()
//        {
//            if (!CityBuddy._settings["Toggle"].AsBool())
//                return new IdleState();

//            if (_toggled == true)
//                return new CityAttackState();

//            return null;
//        }

//        public void OnStateEnter()
//        {
//            Chat.WriteLine("City controller state.");
//        }

//        public void OnStateExit()
//        {
//            Chat.WriteLine("Exit city controller state");
//            _init = false;
//            _toggled = false;
//        }

//        public void Tick()
//        {
//            Dynel citycontroller = DynelManager.AllDynels
//                .Where(c => c.Name == "City Controller")
//                .FirstOrDefault();

//            if (citycontroller != null)
//            {
//                if (citycontroller?.DistanceFrom(DynelManager.LocalPlayer) < 5f
//                    && !_init)
//                {
//                    MovementController.Instance.Halt();
//                    _init = true;
//                    Logic(citycontroller);
//                }
//                else if (citycontroller?.DistanceFrom(DynelManager.LocalPlayer) > 5f && MovementController.Instance.IsNavigating == false)
//                    MovementController.Instance.SetDestination(citycontroller.Position);
//            }
//        }

//        private static void Logic(Dynel cc)
//        {
//            Task.Factory.StartNew(
//                async () =>
//                {
//                    await Task.Delay(3000);
//                    cc.Use();
//                    await Task.Delay(2000);
//                    if (Inventory.Find(257110, out Item cru))
//                        cru.UseOn(cc.Identity);
//                    await Task.Delay(3000);
//                    CityBuddy.ToggleCloak();
//                    await Task.Delay(1500);
//                    //MovementController.Instance.SetDestination(CityBuddy._montroyalGaurdPos);
//                    _toggled = true;
//                });
//        }
//    }
//}
