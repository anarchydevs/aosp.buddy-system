using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class CircleState : IState
    {
        public static bool _init = false;

        private static double _time = Time.NormalTime;

        private static SimpleChar _redTower;
        private static SimpleChar _blueTower;

        public IState GetNextState()
        {
            _redTower = DynelManager.NPCs
               .Where(c => c.Health > 0
                   && c.Name.Contains("Strange Xan Artifact")
                   && c.Buffs.Contains(274119))
               .FirstOrDefault();

            _blueTower = DynelManager.NPCs
               .Where(c => c.Health > 0
                   && c.Name.Contains("Strange Xan Artifact")
                   && !c.Buffs.Contains(274119))
               .FirstOrDefault();

            if (_redTower != null)
            {
                DB2Buddy.NavMeshMovementController.Halt();
                DynelManager.LocalPlayer.Position = _redTower.Position;
                DB2Buddy.NavMeshMovementController.SetMovement(MovementAction.Update);
                return new FightTowerState();
            }

            if (_blueTower != null)
            {
                DB2Buddy.NavMeshMovementController.Halt();
                DynelManager.LocalPlayer.Position = _blueTower.Position;
                DB2Buddy.NavMeshMovementController.SetMovement(MovementAction.Update);
                return new FightTowerState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("CircleState::OnStateEnter");

            _time = Time.NormalTime;
        }

        public void OnStateExit()
        {
            Chat.WriteLine("CircleState::OnStateExit");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Time.NormalTime > _time + 2f)
            {
                _time = Time.NormalTime;

                if (!_init)
                {
                    _init = true;
                    DynelManager.LocalPlayer.Position = new Vector3(296.0f, 133.4f, 236.1f);
                    DB2Buddy.NavMeshMovementController.SetMovement(MovementAction.Update);
                }
                else
                {
                    DynelManager.LocalPlayer.Position = new Vector3(279.1f, 133.4f, 231.7f);
                    DB2Buddy.NavMeshMovementController.SetMovement(MovementAction.Update);
                    _init = false;
                }
            }
        }
    }
}