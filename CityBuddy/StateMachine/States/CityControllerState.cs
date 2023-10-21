using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Diagnostics;
using System.Linq;

namespace CityBuddy
{
    public class CityControllerState : IState
    {
        private static Stopwatch _actionStopwatch = new Stopwatch();
        private static Stopwatch _cruUseStopwatch = new Stopwatch();

        public IState GetNextState()
        {
            if (!CityBuddy._settings["Enable"].AsBool())
                return new IdleState();

            if ((CityController.CloakState != CloakStatus.Unknown && CityController.Charge >= 0.80f && !CityController.CanToggleCloak())
                || (!CityController.CanToggleCloak() && CityBuddy.CTWindowIsOpen))
            {
                return new CityAttackState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            CityBuddy.CTWindowIsOpen = false;
            _actionStopwatch.Start();
            Chat.WriteLine("Checking city controller.");
        }

        public void OnStateExit()
        {
            CityBuddy.CTWindowIsOpen = false;
            //Chat.WriteLine("Exiting city controller state.");
        }

        public void Tick()
        {
            if (_actionStopwatch.Elapsed.TotalSeconds >= 1) // 1 second interval
            {
                Dynel citycontroller = DynelManager.AllDynels
                    .FirstOrDefault(c => c.Name == "City Controller");

                if (citycontroller != null)
                {
                    if (citycontroller.DistanceFrom(DynelManager.LocalPlayer) < 7f)
                    {
                        MovementController.Instance.Halt();
                        ExecuteCityControllerActions(citycontroller);
                    }
                    else if (!MovementController.Instance.IsNavigating)
                    {
                        MovementController.Instance.SetDestination(citycontroller.Position);
                    }
                }

                _actionStopwatch.Restart();
            }
        }

        private void ExecuteCityControllerActions(Dynel cc)
        {
            Item cru = ControllerRecompilerUnit.Crus
                    .Select(id => Inventory.Find(id, out var item) ? item : null)
                    .FirstOrDefault(item => item != null);

            if (!CityBuddy.CTWindowIsOpen)
            {
                //Chat.WriteLine("Opening City Controller");
                CityController.Use();
            }
            else if (CityBuddy.CTWindowIsOpen)
            {
                //Chat.WriteLine("CT window is open");

                if (CityController.CanToggleCloak())
                {
                    //Chat.WriteLine("Can toggle cloak");

                    _cruUseStopwatch.Start();

                    if (CityController.CloakState == CloakStatus.Enabled)
                    {
                        if (CityController.Charge <= 0.75f)
                        {
                            // Add an additional check for `_lastCruUseTime`
                            if (cru != null && _cruUseStopwatch.Elapsed.TotalSeconds > 5)
                            {
                                Chat.WriteLine("Using Controller Recompiler Unit");
                                cru.UseOn(cc.Identity);

                                // Reset the CRU stopwatch whenever you use a CRU
                                _cruUseStopwatch.Restart();

                            }
                        }
                        else
                        {
                            Chat.WriteLine("Disabling Cloak");
                            CityController.ToggleCloak();
                        }
                    }

                    if (CityController.CloakState == CloakStatus.Disabled)
                    {
                        if (CityController.Charge <= 0.75f)
                        {
                            // Add an additional check for `_lastCruUseTime`
                            if (cru != null && _cruUseStopwatch.Elapsed.TotalSeconds > 5)
                            {
                                Chat.WriteLine("Using Controller Recompiler Unit");
                                cru.UseOn(cc.Identity);

                                // Reset the CRU stopwatch whenever you use a CRU
                                _cruUseStopwatch.Restart();

                            }
                        }
                        else
                        {
                            Chat.WriteLine("Enabling Cloak");
                            CityController.ToggleCloak();
                        }
                    }
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
