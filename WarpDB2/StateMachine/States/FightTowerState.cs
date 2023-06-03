using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace WarpDB2
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

            if (!WarpDB2._settings["Toggle"].AsBool())
                WarpDB2.NavMeshMovementController.Halt();

            if (Playfield.ModelIdentity.Instance == Constants.PWId)
                return new IdleState();

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id)
            {
                if (_redTower == null && _blueTower == null && _aune != null)
                {
                    return new FightState();
                }

                if (_aune == null && _blueTower == null && _redTower == null)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._startPosition) > 10)
                        WarpDB2.NavMeshMovementController.SetNavMeshDestination(Constants._startPosition);
                }

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.first) < 60)
                    return new FellState();

                Network.ChatMessageReceived += (s, msg) =>
                {
                    if (msg.PacketType != ChatMessageType.NpcMessage)
                        return;

                    var npcMsg = (NpcMessage)msg;

                    string[] triggerMsg = new string[2] { "Know the power of the Xan", "You will never know the secrets of the machine" };

                    if (triggerMsg.Any(x => npcMsg.Text.Contains(x)))
                    {
                        WarpDB2._taggedNotum = true;
                    }
                };

                if (WarpDB2._taggedNotum)
                {
                    return new NotumState();
                }
            }
            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine($"FightTowerState");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit FightTowerState");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

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

            if (_aune != null)
            {
                if (DynelManager.LocalPlayer.FightingTarget.Name == _aune.Name)
                    DynelManager.LocalPlayer.StopAttack();
            }

            if (_redTower != null || _blueTower != null)
            {
                if (_redTower != null)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) > 3)
                    {
                        DynelManager.LocalPlayer.Position = _redTower.Position;
                        MovementController.Instance.SetMovement(MovementAction.Update);
                    }

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) < 5
                        && DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_redTower);
                    }
                }

                else if (_blueTower != null && !DynelManager.LocalPlayer.Buffs.Contains(WarpDB2.Nanos.XanBlessingoftheEnemy))
                {

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) > 3)
                    {
                        DynelManager.LocalPlayer.Position = _blueTower.Position;
                        MovementController.Instance.SetMovement(MovementAction.Update);
                    }

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) < 5
                        && DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_blueTower);
                    }
                }
            }
        }

    }
}