using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class FightTowerState : IState
    {
        private static SimpleChar _aune;
        private static SimpleChar _redTower;
        private static SimpleChar _blueTower;

        public IState GetNextState()
        {
            _aune = DynelManager.NPCs
               .Where(c => c.Health > 0
                   && c.Name.Contains("Ground Chief Aune"))
               .FirstOrDefault();

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

            if (_redTower == null && _blueTower == null)
            {
                if (_aune == null
                    || (_aune != null && !_aune.Buffs.Contains(273220) && !DynelManager.LocalPlayer.Buffs.Contains(274101)))
                {
                    DynelManager.LocalPlayer.Position = new Vector3(285.1f, 133.4f, 229.1f);
                    MovementController.Instance.SetMovement(MovementAction.Update);
                    return new FightBossState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine($"FightTowerState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("FightTowerState::OnStateExit");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

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
                if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) < 19f)
                    DynelManager.LocalPlayer.Attack(_redTower);

                DynelManager.LocalPlayer.Position = _redTower.Position;
                MovementController.Instance.SetMovement(MovementAction.Update);
            }
            else if (_blueTower != null)
            {
                if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) < 19f)
                    DynelManager.LocalPlayer.Attack(_blueTower);

                DynelManager.LocalPlayer.Position = _blueTower.Position;
                MovementController.Instance.SetMovement(MovementAction.Update);
            }
        }
    }
}