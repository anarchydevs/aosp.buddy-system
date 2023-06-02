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

namespace WarpDB2
{
    public class FellState : IState
    {
        private static bool _first = false;
        private static bool _second = false;
        private static bool _third = false;
        private static bool _forth = false;

        public IState GetNextState()
        {

            if (!WarpDB2._settings["Toggle"].AsBool())
            {
                WarpDB2.NavMeshMovementController.Halt();
            }

            if (Playfield.ModelIdentity.Instance != Constants.DB2Id)
                return new IdleState();

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id
                 && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._warpPos) < 10f
                 && WarpDB2._settings["Toggle"].AsBool())
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
            if (Game.IsZoning) { return; }

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id)
            {

                foreach (TeamMember member in Team.Members)
                {
                    if (!_first && !_second && !_third && !_forth)
                    {
                        if (DynelManager.LocalPlayer.Buffs.Contains(WarpDB2.Nanos.PathtoElevation1))
                        _first = true;

                        else
                        WarpDB2.NavMeshMovementController.SetNavMeshDestination(Constants.first);

                        //Chat.WriteLine("First");

                    }
                    else if (_first && !_second && !_third && !_forth)
                    {
                        if (DynelManager.LocalPlayer.Buffs.Contains(WarpDB2.Nanos.PathtoElevation2))
                            _second = true;

                        else
                            WarpDB2.NavMeshMovementController.SetNavMeshDestination(Constants.second);

                        //Chat.WriteLine("Second");
                        
                    }
                    else if (_first && _second && !_third && !_forth)
                    {
                        if (DynelManager.LocalPlayer.Buffs.Contains(WarpDB2.Nanos.PathtoElevation3))
                            _third = true;

                        else
                            WarpDB2.NavMeshMovementController.SetNavMeshDestination(Constants.third);

                        //Chat.WriteLine("Third");
                       
                    }
                    else if (_first && _second && _third && !_forth)
                    {
                        if (DynelManager.LocalPlayer.Buffs.Contains(WarpDB2.Nanos.PathtoElevation4))
                            _forth = true;

                        else
                        WarpDB2.NavMeshMovementController.SetNavMeshDestination(Constants.forth);

                        //Chat.WriteLine("Forth");

                    }

                }

            }
        }
    }
}