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
    public class PathToBossState : IState
    {
        private static bool _init = false;

        private static double _time;
        private static double _startTime;

        private static SimpleChar _aune;

        public IState GetNextState()
        {
            _aune = DynelManager.NPCs
            .Where(c => c.Health > 0
                && c.Name.Contains("Ground Chief Aune"))
            .FirstOrDefault();

            if (!DB2Buddy._settings["Toggle"].AsBool())
            {
                DB2Buddy.NavMeshMovementController.Halt();
            }

            if (Playfield.ModelIdentity.Instance != Constants.DB2Id)
                return new IdleState();

            if (_aune != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._startPosition) < 1)
            {
                return new FightState();
            }

            if (_aune == null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._startPosition) < 5)
            {
                return new IdleState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("PathToBossState");
            _time = Time.NormalTime;
            _startTime = Time.NormalTime;
            _init = true;
        }

        public void OnStateExit()
        {
            Chat.WriteLine(" Exit PathToBossState");

            _init = false;
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPosition) > 5f)
                {
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPosition);
                }
            }
        }
    }
}