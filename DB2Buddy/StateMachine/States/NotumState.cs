using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DB2Buddy
{
    public class NotumState : IState
    {
        private SimpleChar _aune;
        private SimpleChar _mist;
        private SimpleChar _redTower;
        private SimpleChar _blueTower;

        private double _timeToTagReset;
        private double _timeForPositionUpdate;
        private double _timeForTurn;
        private bool _positionUpdated = false;
        private bool _turned = false;

        private string previousErrorMessage = string.Empty;

        public IState GetNextState()
        {

            _aune = DynelManager.NPCs
              .Where(c => c.Health > 0
                  && c.Name.Contains("Ground Chief Aune")
                  && !c.Name.Contains("Remains of "))
              .FirstOrDefault();

            _redTower = DynelManager.NPCs
               .Where(c => c.Health > 0
                   && c.Name.Contains("Strange Xan Artifact")
                   && !c.Name.Contains("Remains of ")
                   && c.Buffs.Contains(274119))
               .FirstOrDefault();

            _blueTower = DynelManager.NPCs
               .Where(c => c.Health > 0
                   && c.Name.Contains("Strange Xan Artifact")
                   && !c.Name.Contains("Remains of ")
                   && !c.Buffs.Contains(274119))
               .FirstOrDefault();

            if (!DB2Buddy._settings["Toggle"].AsBool())
            {
                DB2Buddy.NavMeshMovementController.Halt();
            }

            if (Playfield.ModelIdentity.Instance == Constants.PWId)
                return new IdleState();

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id && !DB2Buddy._taggedNotum)
            {

                if (_redTower == null && _blueTower == null && !MovementController.Instance.IsNavigating)
                {
                    if (_aune != null && !_aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients)
                     && !DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
                    {
                        return new FightState();
                    }
                }

                if (_aune != null)
                {
                    if (_redTower != null || DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
                    {
                        return new FightTowerState();
                    }

                    if (_blueTower != null || _aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients))
                    {
                        if (!DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
                            return new FightTowerState();
                    }
                }

                if (_aune != null)
                {
                    if (_redTower == null && _blueTower == null && !MovementController.Instance.IsNavigating)
                    {
                        return new FightState();
                    }
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("NotumState");
            DB2Buddy.NavMeshMovementController.Halt();
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit NotumState");
        }

        public void Tick()
        {
            try
            {
                if (Game.IsZoning) { return; }

                _mist = DynelManager.NPCs
                   .Where(c => c.Name.Contains("Notum Irregularity"))
                   .FirstOrDefault();

                if (_mist != null)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_mist.Position) < 0.6)
                    {
                        if (_timeToTagReset <= 0)
                        {
                            _timeToTagReset = Time.NormalTime + 5; // Schedule tag reset in 5 seconds
                        }
                        else if (Time.NormalTime >= _timeToTagReset)
                        {
                            DB2Buddy._taggedNotum = false;
                            _timeToTagReset = 0;
                        }
                    }

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_mist.Position) > 0.5)
                    {
                        if (_timeForPositionUpdate <= 0)
                        {
                            _timeForPositionUpdate = Time.NormalTime + 1; // Schedule position update in 1 second
                            _timeForTurn = _timeForPositionUpdate + 1; // Schedule turn in 1 second after position update
                        }

                        if (_timeForPositionUpdate > 0 && Time.NormalTime >= _timeForPositionUpdate)
                        {
                            DynelManager.LocalPlayer.Position = _mist.Position;
                            _positionUpdated = true;
                            _timeForPositionUpdate = 0;
                        }

                        if (_positionUpdated && _timeForTurn > 0 && Time.NormalTime >= _timeForTurn)
                        {
                            MovementController.Instance.SetMovement(MovementAction.TurnRightStart);
                            MovementController.Instance.SetMovement(MovementAction.TurnRightStop);
                            MovementController.Instance.SetMovement(MovementAction.Update);
                            _turned = true;
                            _timeForTurn = 0;
                        }

                        // Reset the flags for the next round of actions
                        if (_positionUpdated && _turned)
                        {
                            _positionUpdated = false;
                            _turned = false;
                        }
                    }
                }

                if (_mist == null)
                {
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPosition);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    previousErrorMessage = errorMessage;
                }
            }
        }
        private int GetLineNumber(Exception ex)
        {
            var lineNumber = 0;

            var lineMatch = Regex.Match(ex.StackTrace ?? "", @":line (\d+)$", RegexOptions.Multiline);

            if (lineMatch.Success)
                lineNumber = int.Parse(lineMatch.Groups[1].Value);

            return lineNumber;
        }
    }
}