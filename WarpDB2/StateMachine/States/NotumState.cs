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
using System.Threading.Tasks;

namespace WarpDB2
{
    public class NotumState : IState
    {
        private static SimpleChar _redTower;
        private static SimpleChar _blueTower;

        private static SimpleChar _mist;

        public IState GetNextState()
        {

            if (!WarpDB2._settings["Toggle"].AsBool())
            {
                WarpDB2.NavMeshMovementController.Halt();
            }

            if (Playfield.ModelIdentity.Instance == Constants.PWId)
                return new IdleState();

            if (!WarpDB2._taggedNotum)
            {
                return new FightState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("NotumState");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit NotumState");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            _mist = DynelManager.NPCs
              .Where(c => c.Name.Contains("Notum Irregularity"))
              .FirstOrDefault();

            if (_mist != null)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(_mist.Position) < 0.6)
                {
                    Task.Factory.StartNew(
                               async () =>
                               {
                                   await Task.Delay(4000);
                                   WarpDB2._taggedNotum = false;
                               });
                }

                if (DynelManager.LocalPlayer.Position.DistanceFrom(_mist.Position) > 0.5)
                {
                    Task.Factory.StartNew(
                               async () =>
                               {
                                   await Task.Delay(1000);
                                   DynelManager.LocalPlayer.Position = _mist.Position;
                                   await Task.Delay(1000);
                                   MovementController.Instance.SetMovement(MovementAction.Update);
                                   await Task.Delay(1000);
                                   MovementController.Instance.SetMovement(MovementAction.Update);
                               });

                }
            }

            if (_mist == null)
            {
                WarpDB2.NavMeshMovementController.SetNavMeshDestination(Constants._startPosition);
            }

        }

    }
}