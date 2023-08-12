using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class FellState : IState
    {
        private static bool _first = false;
        private static bool _second = false;
        private static bool _third = false;
        private static bool _forth = false;

        private string previousErrorMessage = string.Empty;

        public IState GetNextState()
        {

            if (!DB2Buddy._settings["Toggle"].AsBool())
            {
                DB2Buddy.NavMeshMovementController.Halt();
            }

            if (Playfield.ModelIdentity.Instance != Constants.DB2Id)
                return new IdleState();

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id
                 && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._warpPos) < 10f
                 && DB2Buddy._settings["Toggle"].AsBool())
            {
                _first = false;
                _second = false;
                _third = false;
                _forth = false;

                return new PathToBossState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            _first = false;
            _second = false;
            _third = false;
            _forth = false;

            Chat.WriteLine("FellState");
        }

        public void OnStateExit()
        {
            _first = false;
            _second = false;
            _third = false;
            _forth = false;

            Chat.WriteLine(" Exit FellState");
        }

        public void Tick()
        {
            try
            {
                if (Game.IsZoning) { return; }

                if (Playfield.ModelIdentity.Instance == Constants.DB2Id)
                {

                    foreach (TeamMember member in Team.Members)
                    {
                        if (!_first && !_second && !_third && !_forth)
                        {
                            if (DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.PathtoElevation1))
                                _first = true;

                            else
                                DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants.first);

                            //Chat.WriteLine("First");

                        }
                        else if (_first && !_second && !_third && !_forth)
                        {
                            if (DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.PathtoElevation2))
                                _second = true;

                            else
                                DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants.second);

                            //Chat.WriteLine("Second");

                        }
                        else if (_first && _second && !_third && !_forth)
                        {
                            if (DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.PathtoElevation3))
                                _third = true;

                            else
                                DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants.third);

                            //Chat.WriteLine("Third");

                        }
                        else if (_first && _second && _third && !_forth)
                        {
                            if (DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.PathtoElevation4))
                                _forth = true;

                            else
                                DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants.forth);

                            //Chat.WriteLine("Forth");

                        }

                    }

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