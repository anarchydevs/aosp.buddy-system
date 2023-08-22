using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.GameData;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CityBuddy
{
    public class CityControllerState : IState
    {

        private static double _lastCruUseTime;

        public IState GetNextState()
        {
            if (!CityBuddy._settings["Enable"].AsBool())
                return new IdleState();

            if (CityController.CloakState != CloakStatus.Unknown && CityController.Charge >= 0.5f
                && !CityController.CanToggleCloak())
            {
                return new CityAttackState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Checking city controller.");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("Exiting city controller state.");
        }

        public async void Tick()
        {
            Dynel citycontroller = DynelManager.AllDynels
                .Where(c => c.Name == "City Controller")
                .FirstOrDefault();

            if (citycontroller != null)
            {
                if (citycontroller.DistanceFrom(DynelManager.LocalPlayer) < 5f)
                {
                    MovementController.Instance.Halt();
                   await ExecuteCityControllerActionsAsync(citycontroller);
                }
                else if (citycontroller.DistanceFrom(DynelManager.LocalPlayer) > 5f && !MovementController.Instance.IsNavigating)
                {
                    MovementController.Instance.SetDestination(citycontroller.Position);
                }
            }
        }

        private static async Task ExecuteCityControllerActionsAsync(Dynel cc)
        {
            
            CityController.Use();

            await Task.Delay(3000);

            if (CityController.Charge < 0.50f)
            {
                Item cru = null;

                foreach (var id in ControllerRecompilerUnit.Crus)
                {
                    if (Inventory.Find(id, out cru))
                    {
                        break;
                    }
                }

                if (cru != null && Time.NormalTime > _lastCruUseTime +5)
                {
                    Chat.WriteLine("Using Controller Recompiler Unit");
                    cru.UseOn(cc.Identity);
                    _lastCruUseTime = Time.NormalTime;
                }
            }

            if (CityController.CanToggleCloak())
            {
                CityController.ToggleCloak();
            }
        }

        public static class ControllerRecompilerUnit
        {
            public static readonly int[] Crus = {
                257110, 254364, 305225, 254367, 254359, 258522, 254350, 254329, 254328, 254327, 254326
            };
        }
    }
}
