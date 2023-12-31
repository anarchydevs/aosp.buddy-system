﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeBuddy
{
    public class PathState : IState
    {
        private SimpleChar _allMobs;
        private SimpleChar _target;
        private static Corpse _corpse;
        private Dynel _bossButton;
        private Dynel _upButton;
        private Dynel _downButton;
        private static double _buttonTimer;
        bool isPathing = false;
        public static Vector3 _bossButtonLocation = Vector3.Zero;

        public static Dictionary<Identity, Tuple<Vector3, Room>> _mobLocation = new Dictionary<Identity, Tuple<Vector3, Room>>();

        public static List<Vector3> _upButtonLocations = new List<Vector3>();

        Dictionary<Vector3, Tuple<string, string>> allDoors = new Dictionary<Vector3, Tuple<string, string>>();
        List<string> visitedRoomNames = new List<string>();
        Dictionary<Vector3, Tuple<string, string>> unvisitedDoors = new Dictionary<Vector3, Tuple<string, string>>();

        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            if (!LeBuddy._settings["Enable"].AsBool() || !Playfield.IsDungeon)
            {
                return new IdleState();
            }

            if (LeBuddy._settings["Enable"].AsBool())
            {
                if (DynelManager.LocalPlayer.Room.Name == "Mothership_bossroom")
                {
                    return new BossRoomState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("PathState");
        }

        public void OnStateExit()
        {
            LeBuddy.NavMeshMovementController.Halt();

            Chat.WriteLine("Exit PathState");

            if (DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.StopAttack();
        }

        public void Tick()
        {
            try
            {
                Door _exitDoor = Playfield.Doors.FirstOrDefault(d =>
                        (d.RoomLink1 != null && d.RoomLink2 == null) || (d.RoomLink1 == null && d.RoomLink2 != null));

                _upButton = DynelManager.AllDynels
                .Where(c => c.Name == "Button (up)")
                .FirstOrDefault();

                _target = DynelManager.NPCs
                .Where(c => c.Health > 0 && !Constants._ignores.Contains(c.Name))
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .ThenBy(c => c.HealthPercent)
                .FirstOrDefault();

                if (Game.IsZoning || !Team.IsInTeam) { return; }

                _allMobs = GetAllMobs();

                _corpse = (Corpse)GetCorpse();

                _downButton = GetButton("Button (down)");
                _bossButton = GetButton("Button (boss)");

                if (_exitDoor != null && DynelManager.LocalPlayer.Room.Name == "Mothership_entrance")
                {
                    if (ButtonExitState._exitDoorLocation == Vector3.Zero)
                    {
                        ButtonExitState._exitDoorLocation = _exitDoor.Position;
                        //Chat.WriteLine("Set exit door location");
                    }
                }

                if (_downButton != null && !ButtonExitState._downButtonLocation.Contains(_downButton.Position))
                {
                    ButtonExitState._downButtonLocation.Add(_downButton.Position);
                    //Chat.WriteLine("Added down button location");
                }

                if (_upButton != null)
                {
                    foreach (Room room in Playfield.Rooms)
                    {
                        if (!_upButtonLocations.Contains(_upButton.Position))
                        {
                            _upButtonLocations.Add(_upButton.Position);
                            //Chat.WriteLine("Added up button location");
                        }
                    }
                }

                if (_bossButton != null && _bossButton.Position != _bossButtonLocation)
                {
                    _bossButtonLocation = _bossButton.Position;
                    //Chat.WriteLine("Set boss button location");
                }

                if (_target != null)
                {
                    if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) < 10f)
                    {
                        if (DynelManager.LocalPlayer.FightingTarget == null
                            && !DynelManager.LocalPlayer.IsAttacking
                            && !DynelManager.LocalPlayer.IsAttackPending
                            && _target.IsInLineOfSight)
                        {
                            DynelManager.LocalPlayer.Attack(_target);
                        }
                        else if (DynelManager.LocalPlayer.IsAttacking && !(_target.IsInLineOfSight))
                        {
                            DynelManager.LocalPlayer.StopAttack();
                        }
                    }
                }

                if (DynelManager.LocalPlayer.Identity != LeBuddy.Leader)
                {
                    LeBuddy._leader = GetLeaderCharacter();

                    if (LeBuddy._leader != null)
                    {

                        PathToLeader();
                    }
                    else
                    {
                        HandleButtonUsage();
                    }  
                }
                else
                {
                    foreach (Door door in Playfield.Doors)
                    {
                        string roomLink1Name = door.RoomLink1?.Name;
                        string roomLink2Name = door.RoomLink2?.Name;
                        Vector3 doorPosition = door.Position;

                        if (!allDoors.ContainsKey(doorPosition))
                        {
                            allDoors[doorPosition] = new Tuple<string, string>(roomLink1Name, roomLink2Name);
                            //Chat.WriteLine($"Added door at {doorPosition} to allDoors.");
                        }
                    }

                    Room currentRoom = DynelManager.LocalPlayer.Room;

                    if (currentRoom != null)
                    {
                        string currentRoomName = currentRoom.Name;

                        if (!visitedRoomNames.Contains(currentRoomName))
                        {
                            visitedRoomNames.Add(currentRoomName);
                            //Chat.WriteLine($"Added {currentRoomName} to visited rooms.");
                        }

                        foreach (Door door in currentRoom.Doors)
                        {
                            string doorLink1Name = door.RoomLink1?.Name;
                            string doorLink2Name = door.RoomLink2?.Name;
                            Vector3 doorPosition = door.Position;

                            if ((doorLink1Name == currentRoomName || doorLink2Name == currentRoomName) && unvisitedDoors.ContainsKey(doorPosition))
                            {
                                unvisitedDoors.Remove(doorPosition);
                                //Chat.WriteLine($"Removed door at {doorPosition} from unvisited doors.");
                            }
                        }
                    }

                    foreach (Door door in Playfield.Doors)
                    {
                        string roomLink1Name = door.RoomLink1?.Name;
                        string roomLink2Name = door.RoomLink2?.Name;
                        Vector3 doorPosition = door.Position;

                        if (!visitedRoomNames.Contains(roomLink1Name) && !visitedRoomNames.Contains(roomLink2Name) && !unvisitedDoors.ContainsKey(doorPosition))
                        {
                            unvisitedDoors[doorPosition] = new Tuple<string, string>(roomLink1Name, roomLink2Name);
                            //Chat.WriteLine($"Added door at {doorPosition} to unvisited doors.");
                        }
                    }

                    if (!Team.Members.Any(m => m.Character == null))
                    {
                        PathToDestination();
                    }
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

        private SimpleChar GetAllMobs()
        {
            return DynelManager.NPCs
                .Where(c => c.Health > 0 && !Constants._ignores.Contains(c.Name))
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .ThenBy(c => c.HealthPercent)
                .FirstOrDefault();
        }

        private Dynel GetCorpse()
        {
            return DynelManager.Corpses
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .FirstOrDefault();
        }

        private Dynel GetButton(string buttonName)
        {
            return DynelManager.AllDynels
                .Where(c => c.Name == buttonName)
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .FirstOrDefault();
        }

        private void PathToDestination()
        {
            LockedDoors();

            if (Team.Members.Any(c => c.Character == null) || !LeBuddy.Ready)
            {
                return;
            }

            if (_corpse != null && LeBuddy._settings["Looting"].AsBool())
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpse.Position) > 2)
                {
                    LeBuddy.NavMeshMovementController.SetNavMeshDestination(_corpse.Position);
                }
            }
            else if (_allMobs != null)
            {
                HandleTargetMovement();
            }
            else if (_allMobs == null && Extensions.CanProceed())
            {
                HandleDestinationMovement();
            }
        }

        private SimpleChar GetLeaderCharacter()
        {
            return Team.Members
                .Where(c => c.Character?.Health > 0 && c.Character?.IsValid == true && c.Identity == LeBuddy.Leader)
                .FirstOrDefault()?.Character;
        }

        private void PathToLeader()
        {
            if (LeBuddy._leader != null)
            {
                LeBuddy._leaderPos = (Vector3)LeBuddy._leader?.Position;

                if (LeBuddy._leaderPos == Vector3.Zero
                    || DynelManager.LocalPlayer.Position.DistanceFrom(LeBuddy._leaderPos) <= 1.6f
                    || DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                    return;

                LeBuddy.NavMeshMovementController.SetNavMeshDestination(LeBuddy._leaderPos);
            }
        }

        private void HandleButtonUsage()
        {
            if (_upButton != null)
            {
                if (_upButton.Position != null && _upButton.Position.DistanceFrom(DynelManager.LocalPlayer.Position) < 7f)
                {
                    if (Time.NormalTime > _buttonTimer + 3.0)
                    {
                        _buttonTimer = Time.NormalTime;
                        _upButton.Use();
                    }
                }
                else
                {
                    LeBuddy.NavMeshMovementController.SetNavMeshDestination(_upButton.Position);
                }
            }

            if (_bossButton != null)
            {
                if (_bossButton.Position != null && _bossButton.Position.DistanceFrom(DynelManager.LocalPlayer.Position) < 7f)
                {
                    if (Time.NormalTime > _buttonTimer + 3.0)
                    {
                        _buttonTimer = Time.NormalTime;
                        _bossButton.Use();
                    }
                }
                else
                {
                    LeBuddy.NavMeshMovementController.SetNavMeshDestination(_bossButton.Position);
                }
            }
        }

        private void HandleTargetMovement()
        {
            if (!Team.Members.Any(m => m.Character == null))
            {
                foreach (Room room in Playfield.Rooms)
                {
                    if (IsPointInRoom(_allMobs.Position, room))
                    {
                        if (!_mobLocation.ContainsKey(_allMobs.Identity))
                        {
                            _mobLocation[_allMobs.Identity] = new Tuple<Vector3, Room>(_allMobs.Position, room);
                            //Chat.WriteLine($"Mob added: Identity = {_allMobs.Identity}, Position = {_allMobs.Position}, Room = {room.Name}");
                        }
                        break;
                    }
                }

                if (_allMobs.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 2f || !_allMobs.IsInLineOfSight)
                {
                    LeBuddy.NavMeshMovementController.SetNavMeshDestination(_allMobs.Position);
                }
            }
        }

        private void HandleDestinationMovement()
        {
            if (!Team.Members.Any(c => c.Character == null))
            {
                if (_mobLocation.Count > 0)
                {
                    foreach (var pair in _mobLocation)
                    {
                        Room roomContainingMob = null;

                        foreach (Room room in Playfield.Rooms)
                        {
                            if (IsPointInRoom(pair.Value.Item1, room))
                            {
                                roomContainingMob = room;
                                break;
                            }
                        }

                        if (roomContainingMob != null && DynelManager.LocalPlayer.Position.DistanceFrom(pair.Value.Item1) > 5)
                        {
                            LeBuddy.NavMeshMovementController.SetNavMeshDestination(pair.Value.Item1);
                            break;
                        }
                    }

                    if (_allMobs == null)
                    {
                        var keysToRemove = new List<Identity>();

                        foreach (var pair in _mobLocation)
                        {
                            if (pair.Value.Item1.Distance2DFrom(DynelManager.LocalPlayer.Position) < 60f)
                            {
                                keysToRemove.Add(pair.Key);
                            }
                        }

                        foreach (var key in keysToRemove)
                        {
                            var removedValue = _mobLocation[key];
                            _mobLocation.Remove(key);
                            //Chat.WriteLine($"Mob removed: Identity = {key}, Position = {removedValue.Item1}, Room = {removedValue.Item2.Name}");
                        }
                    }
                }

                else if (_bossButton != null)
                {
                    //var teamMemberPositions = Team.Members.Select(member => DynelManager.GetDynel(member.Identity)?.Position);

                    //if (teamMemberPositions.All(position => position != null
                    //&& DynelManager.LocalPlayer.Position.DistanceFrom((Vector3)position) < 5f)
                    //&&
                    if (_bossButton.Position.DistanceFrom(DynelManager.LocalPlayer.Position) < 5f)
                    {
                        if (Time.NormalTime > _buttonTimer + 3.0)
                        {
                            _buttonTimer = Time.NormalTime;

                            Vector3 localPlayerPositionBeforeButtonUse = DynelManager.LocalPlayer.Position;

                            _bossButton.Use();

                            Vector3 localPlayerPositionAfterButtonUse = DynelManager.LocalPlayer.Position;

                            if (localPlayerPositionAfterButtonUse.DistanceFrom(localPlayerPositionBeforeButtonUse) > 0.01f)
                            {
                                allDoors.Clear();
                                visitedRoomNames.Clear();
                                unvisitedDoors.Clear();
                                _upButtonLocations.Clear();
                                _mobLocation.Clear();
                                LeBuddy.NavMeshMovementController.Halt();
                                return;
                            }
                        }
                    }

                    else if (_bossButton.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 5f)
                        //&& !LeBuddy.NavMeshMovementController.IsNavigating)
                    {
                        //Chat.WriteLine($" Pathing to Boss Button.");
                        LeBuddy.NavMeshMovementController.SetNavMeshDestination(_bossButton.Position);
                    }
                }

                else if (_upButton != null)
                {
                    //var teamMemberPositions = Team.Members.Select(member => DynelManager.GetDynel(member.Identity)?.Position);

                    if (_upButton.Position.DistanceFrom(DynelManager.LocalPlayer.Position) < 5f)
                    {
                        if (Time.NormalTime > _buttonTimer + 3.0)
                        {
                            _buttonTimer = Time.NormalTime;

                            Vector3 localPlayerPositionBeforeButtonUse = DynelManager.LocalPlayer.Position;

                            _upButton.Use();

                            Vector3 localPlayerPositionAfterButtonUse = DynelManager.LocalPlayer.Position;

                            if (localPlayerPositionAfterButtonUse.DistanceFrom(localPlayerPositionBeforeButtonUse) > 0.01f)
                            {
                                allDoors.Clear();
                                visitedRoomNames.Clear();
                                unvisitedDoors.Clear();
                                _upButtonLocations.Clear();
                                _mobLocation.Clear();
                                LeBuddy.NavMeshMovementController.Halt();
                                return;
                            }
                        }
                    }
                    else if (_upButton.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 5f)
                        //&& !LeBuddy.NavMeshMovementController.IsNavigating)
                    {
                        //Chat.WriteLine($" Pathing to Up Button.");
                        LeBuddy.NavMeshMovementController.SetNavMeshDestination(_upButton.Position);
                    }
                }

                else
                {
                    if (_bossButton == null && _bossButtonLocation != Vector3.Zero && !LeBuddy.NavMeshMovementController.IsNavigating)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(_bossButtonLocation) > 2)
                        {
                            LeBuddy.NavMeshMovementController.SetNavMeshDestination(_bossButtonLocation);
                            //Chat.WriteLine($" Pathing to saved Boss Button at {_bossButtonLocation}.");
                        }
                    }

                    else if (_upButton == null && _upButtonLocations.Count > 0 && !LeBuddy.NavMeshMovementController.IsNavigating)
                    {
                        Vector3 lastLocation = _upButtonLocations[_upButtonLocations.Count - 1];

                        if (DynelManager.LocalPlayer.Position.DistanceFrom(lastLocation) > 2)
                        {
                            LeBuddy.NavMeshMovementController.SetNavMeshDestination(lastLocation);
                            //Chat.WriteLine($"Pathing to saved Up Button at {lastLocation}.");
                        }

                        for (int i = _upButtonLocations.Count - 1; i >= 0; i--)
                        {
                            if (_upButtonLocations[i].Distance2DFrom(DynelManager.LocalPlayer.Position) < 60f)
                            {
                                _upButtonLocations.RemoveAt(i);
                            }
                        }
                    }

                    else
                    {
                        ProcessDoors();
                    }  
                }
            }
        }

        public void ProcessDoors()
        {
            if (unvisitedDoors.Count > 0 && !LeBuddy.NavMeshMovementController.IsNavigating)
            {
                Vector3 lastUnvisitedDoorPosition = unvisitedDoors.Keys.FirstOrDefault();
                float distanceToDoor = 0;

                if (lastUnvisitedDoorPosition != null)
                {
                    distanceToDoor = DynelManager.LocalPlayer.Position.Distance2DFrom(lastUnvisitedDoorPosition);

                    if (distanceToDoor > 5)
                    {
                        LeBuddy.NavMeshMovementController.SetNavMeshDestination(lastUnvisitedDoorPosition);
                        //Chat.WriteLine($"Lost, Pathing to door at {lastUnvisitedDoorPosition}.");
                    }
                    else if (distanceToDoor <= 10)
                    {
                        var nextDoor = unvisitedDoors.Keys.Skip(1).FirstOrDefault();
                        if (nextDoor != null)
                        {
                            LeBuddy.NavMeshMovementController.SetNavMeshDestination(nextDoor);
                            //Chat.WriteLine($"Switching to the next door at {nextDoor}.");
                        }
                    }

                    if (distanceToDoor <= 60)
                    {
                        Door _door = Playfield.Doors.FirstOrDefault(door => door.Position == lastUnvisitedDoorPosition);
                        if (_door == null)
                        {
                            unvisitedDoors.Remove(lastUnvisitedDoorPosition);
                            //Chat.WriteLine($"Door at {lastUnvisitedDoorPosition} does not exist, removing from the list.");
                        }
                    }

                    if (_allMobs != null || _upButton != null || _bossButton != null)
                    {
                        // Exit the "loop" early by returning
                        return;
                    }
                }
            }
        }

        private bool IsPointInRoom(Vector3 point, Room room)
        {
            Rect roomRect = room.Rect;
            return point.X >= roomRect.MinX && point.X <= roomRect.MaxX && point.Y >= roomRect.MinY && point.Y <= roomRect.MaxY;
        }

        private void LockedDoors()
        {
            Item lockPick = Inventory.Items
            .Where(c => c.Name.Contains("Lock Pick"))
            .FirstOrDefault();
            Door door = Playfield.Doors
            .OrderBy(c => c.DistanceFrom(DynelManager.LocalPlayer))
            .FirstOrDefault();

            if (door != null && lockPick != null)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(door.Position) < 5 && door.IsLocked)
                    lockPick.UseOn(door.Identity);
            }
        }
    }
}
