using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using org.critterai.nav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CityBuddy
{
    public class ButtonExitState : IState
    {
        private Dynel _downButton;
        private Door _exitDoor;

        private static double _buttonTimer;

        public static Vector3 _exitDoorLocation = Vector3.Zero;

        public static List<Vector3> _downButtonLocation = new List<Vector3>();

        public IState GetNextState()
        {
            if (!CityBuddy._settings["Enable"].AsBool() || !Playfield.IsDungeon)
                return new IdleState();

            return null;
        }

        public void OnStateEnter()
        {
            EnterState.NavGenSuccessful = false;
            //Chat.WriteLine("Enter ButtonExitState");
        }

        public void OnStateExit()
        {
            CityBuddy.NavMeshMovementController.Halt();
            _downButtonLocation.Clear();
            PathState._mobLocation.Clear();
            PathState._upButtonLocations.Clear();
            PathState._bossButtonLocation = Vector3.Zero;
            _exitDoorLocation = Vector3.Zero;
            //Chat.WriteLine("Exit ButtonExitState");
        }

        public void Tick()
        {
            try
            {
                _downButton = DynelManager.AllDynels.Where(c => c.Name == "Button (down)")
                    .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position)).FirstOrDefault();

                _exitDoor = Playfield.Doors.FirstOrDefault(d =>
                            (d.RoomLink1 != null && d.RoomLink2 == null) || (d.RoomLink1 == null && d.RoomLink2 != null));

                if (_downButtonLocation.Count > 0)
                {
                    if (_downButton != null)
                    {
                        if (_downButton.Position.DistanceFrom(_downButtonLocation.Last()) < 2)
                        {
                            _downButtonLocation.Remove(_downButton.Position);
                        }
                        else if (!_downButtonLocation.Contains(_downButton.Position))
                        {
                            if (DynelManager.LocalPlayer.Position.DistanceFrom(_downButton.Position) > 5)
                                CityBuddy.NavMeshMovementController.SetNavMeshDestination(_downButton.Position);
                            else if (Time.NormalTime > _buttonTimer + 3.0)
                            {
                                CityBuddy.NavMeshMovementController.Halt();
                                _downButton.Use();
                                _buttonTimer = Time.NormalTime;
                            }
                        }
                    }
                    else if (DynelManager.LocalPlayer.Position.DistanceFrom(_downButtonLocation.Last()) > 5)
                    {
                        CityBuddy.NavMeshMovementController.SetNavMeshDestination(_downButtonLocation.Last());
                    }
                }
                else if (_downButton != null)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_downButton.Position) > 5)
                    {
                        CityBuddy.NavMeshMovementController.SetNavMeshDestination(_downButton.Position);
                    }
                    else if (Time.NormalTime > _buttonTimer + 3.0)
                    {
                        _downButton.Use();
                        _buttonTimer = Time.NormalTime;
                    }
                }

                else if (_exitDoorLocation != Vector3.Zero && DynelManager.LocalPlayer.Room.Name != "AI_entrance")
                {
                    CityBuddy.NavMeshMovementController.SetNavMeshDestination(_exitDoorLocation);
                }
                else if (DynelManager.LocalPlayer.Room.Name == "AI_entrance")
                {
                    MoveToExit();
                }

            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + CityBuddy.GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != CityBuddy.previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    CityBuddy.previousErrorMessage = errorMessage;
                }
            }
        }


        private void MoveToExit()
        {
            if (_exitDoor != null)
            {
                float distanceFromExit = DynelManager.LocalPlayer.Position.DistanceFrom(_exitDoor.Position);

                if (!CityBuddy.NavMeshMovementController.IsNavigating)
                {
                    if (distanceFromExit > 5)
                    {
                        CityBuddy.NavMeshMovementController.SetNavMeshDestination(_exitDoor.Position);
                        Chat.WriteLine("Moving to exit door");
                    }
                    else if (distanceFromExit < 2)
                    {
                        CityBuddy.NavMeshMovementController.SetMovement(MovementAction.ForwardStart);
                    }
                }
            }
        }
    }
}
