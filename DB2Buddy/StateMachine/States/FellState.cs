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
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class FellState : IState
    {
        private static bool _first = false;
        private static bool _second = false;
        private static bool _third = false;
        private static bool _forth = false;

        public IState GetNextState()
        {

            if (!DB2Buddy._settings["Toggle"].AsBool())
            {
                DB2Buddy.NavMeshMovementController.Halt();
            }

            if (Playfield.ModelIdentity.Instance == Constants.PWId)
                return new IdleState();

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id
                 && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._atDoor) < 10f
                 && DB2Buddy._settings["Toggle"].AsBool())
                return new PathToBossState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("FellState");
            _first = false;
            _second = false;
            _third = false;
            _forth = false;
        }

        public void OnStateExit()
        {
            Chat.WriteLine(" Exit FellState");
            _first = false;
            _second = false;
            _third = false;
            _forth = false;
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id)
            {

                foreach (TeamMember member in Team.Members)
                {
                    if (!_first && !_second && !_third && !_forth)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.first) > 1)
                            DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants.first);
                        _first = true;
                    }
                    else if (_first && !_second && !_third && !_forth)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.second) > 1)
                            DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants.second);
                        _second = true;

                    }
                    else if (_first && _second && !_third && !_forth)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.third) > 1)
                            DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants.second);
                        _third = true;

                    }
                    else if (_first && _second && _third && !_forth)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.forth) > 1)
                            DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants.second);
                        _forth = true;

                    }

                }

            }
        }
    }
}