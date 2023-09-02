using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.GameData;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CityBuddy
{
    public class CityControllerState : IState
    {

        private static double _lastActionTime = 0;
        private static double _lastCruUseTime;

        public IState GetNextState()
        {
            if (!CityBuddy._settings["Enable"].AsBool())
                return new IdleState();

            if (CityController.CloakState != CloakStatus.Unknown && CityController.Charge >= 0.80f
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

        public void Tick()
        {
            double currentTime = Time.NormalTime;
            if (currentTime < _lastActionTime + 3) // 3 second delay
            {
                return;
            }

            Dynel citycontroller = DynelManager.AllDynels
                .FirstOrDefault(c => c.Name == "City Controller");

            if (citycontroller != null)
            {
                if (citycontroller.DistanceFrom(DynelManager.LocalPlayer) < 5f)
                {
                    MovementController.Instance.Halt();
                    ExecuteCityControllerActions(citycontroller);
                    _lastActionTime = currentTime;
                }
                else if (!MovementController.Instance.IsNavigating)
                {
                    MovementController.Instance.SetDestination(citycontroller.Position);
                }
            }
        }

        private void ExecuteCityControllerActions(Dynel cc)
        {
            CityController.Use();

            if (CityController.Charge < 0.60f)
            {
                Item cru = ControllerRecompilerUnit.Crus
                    .Select(id => Inventory.Find(id, out var item) ? item : null)
                    .FirstOrDefault(item => item != null);

                if (cru != null && Time.NormalTime > _lastCruUseTime + 5)
                {
                    Chat.WriteLine("Using Controller Recompiler Unit");
                    cru.UseOn(cc.Identity);
                    _lastCruUseTime = Time.NormalTime;
                }
            }

            if (CityController.CanToggleCloak())
            {
                if (CityController.CloakState == CloakStatus.Enabled && CityController.Charge > 0.75f)
                {
                    CityController.ToggleCloak();
                }
                if (CityController.CloakState == CloakStatus.Disabled && CityController.Charge > 0.75f)
                {
                    CityController.ToggleCloak();
                }
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
