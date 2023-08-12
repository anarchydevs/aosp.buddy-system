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

namespace DB2Buddy
{
    public class MistState : IState
    {

        private static SimpleChar _mist;
        private static SimpleChar _redTower;
        private static SimpleChar _blueTower;

        public IState GetNextState()
        {
           
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

            if (_redTower != null && !MovementController.Instance.IsNavigating)
            {
                return new FightTowerState();
            }

            if (_blueTower != null && !MovementController.Instance.IsNavigating
                && !DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
            {
                return new FightTowerState();
            }

            if (_redTower == null && _blueTower == null && !MovementController.Instance.IsNavigating)
            {
                return new FightState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("MistState");
            DB2Buddy.NavMeshMovementController.Halt();
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit MistState");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            _mist = DynelManager.NPCs
               .Where(c => c.Name.Contains("Notum Irregularity"))
               .FirstOrDefault();

            DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_mist.Position);

        }

    }
}