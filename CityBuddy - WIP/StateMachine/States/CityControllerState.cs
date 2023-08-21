using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.GameData;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace CityBuddy
{
    public class CityControllerState : IState
    {

        private static DateTime _lastCruUseTime;
        public IState GetNextState()
        {
            if (!CityBuddy._settings["Toggle"].AsBool())
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

            if (CityController.CloakState == CloakStatus.Unknown)
            {
                CityController.Use();
            }
            else if (CityController.CloakState != CloakStatus.Unknown)
            {
                if ((DateTime.Now - _lastCruUseTime).TotalSeconds >= 5)  // 5 seconds cooldown
                {
                    Item cru = null;

                    foreach (var id in ControllerRecompilerUnit.Crus)
                    {
                        if (Inventory.Find(id, out cru))
                        {
                            break;
                        }
                    }

                    if (cru != null)
                    {
                        _lastCruUseTime = DateTime.Now;  // Update the last used time
                        cru.UseOn(cc.Identity);
                    }
                    else
                    {
                        Chat.WriteLine("No usable items found.");
                    }
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
