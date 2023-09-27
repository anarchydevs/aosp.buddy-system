using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using org.critterai.nav;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LeBuddy
{
    public class DeviceExitState : IState
    {
        private Dynel _exitDevice;
        private Corpse _corpse;
        private Door _exitDoor;

        private static double _buttonTimer;

        public IState GetNextState()
        {
            if (!LeBuddy._settings["Enable"].AsBool() || !Playfield.IsDungeon)
                return new IdleState();

            return null;
        }

        public void OnStateEnter()
        {
            EnterState.NavGenSuccessful = false;
            Chat.WriteLine("Enter DeviceExitState");
        }

        public void OnStateExit()
        {
            LeBuddy.NavMeshMovementController.Halt();
            PathState._bossButtonLocation = Vector3.Zero;
            PathState._mobLocation.Clear();
            PathState._upButtonLocations.Clear();
            ButtonExitState._downButtonLocation.Clear();
            ButtonExitState._exitDoorLocation = Vector3.Zero;
            //NavGenState.DeleteNavMesh();
            Chat.WriteLine("Exit DeviceExitState");
        }

        public void Tick()
        {
            try
            {
                _exitDevice = DynelManager.AllDynels.FirstOrDefault(c => c.Name == "Exit Device");

                _corpse = DynelManager.Corpses.Where(c => !c.Name.Contains("Nanovoider") || !c.Name.Contains("Alien Cocoon"))
                    .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position)).FirstOrDefault();

                _exitDoor = Playfield.Doors.FirstOrDefault(d =>
                            (d.RoomLink1 != null && d.RoomLink2 == null) || (d.RoomLink1 == null && d.RoomLink2 != null));


                if (DynelManager.LocalPlayer.Room.Name == "Mothership_bossroom")
                {
                    if (_corpse != null && !LeBuddy.NavMeshMovementController.IsNavigating)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpse.Position) > 1.0f)
                        {
                            LeBuddy.NavMeshMovementController.SetNavMeshDestination(_corpse.Position);
                            Chat.WriteLine("Moving to corpse");
                        }
                    }
                    else if (_corpse == null && _exitDevice != null)
                    {

                        if (DynelManager.LocalPlayer.Position.DistanceFrom(_exitDevice.Position) > 5)
                        {
                            LeBuddy.NavMeshMovementController.SetNavMeshDestination(_exitDevice.Position);
                            //Chat.WriteLine("Moving to exit button");
                        }
                        else if (Time.NormalTime > _buttonTimer + 3.0)
                        {

                            LeBuddy.NavMeshMovementController.Halt();
                            _exitDevice.Use();
                            _buttonTimer = Time.NormalTime;
                        }
                    }
                }
                if (DynelManager.LocalPlayer.Room.Name == "Mothership_entrance")
                {
                    //Chat.WriteLine("in Mothership_entrance");
                    MoveToExit();
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + LeBuddy.GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != LeBuddy.previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    LeBuddy.previousErrorMessage = errorMessage;
                }
            }
        }

        private void MoveToExit()
        {
            if (_exitDoor != null)
            {
                float distanceFromExit = DynelManager.LocalPlayer.Position.DistanceFrom(_exitDoor.Position);

                if (!LeBuddy.NavMeshMovementController.IsNavigating)
                {
                    if (distanceFromExit > 5)
                    {
                        LeBuddy.NavMeshMovementController.SetNavMeshDestination(_exitDoor.Position);
                        Chat.WriteLine("Moving to exit door");
                    }
                    else if (distanceFromExit < 2)
                    {
                        LeBuddy.NavMeshMovementController.SetMovement(MovementAction.ForwardStart);
                    }
                }
            }
        }
    }
}
