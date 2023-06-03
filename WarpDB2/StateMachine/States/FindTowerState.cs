using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WarpDB2
{
    public class FindTowerState : IState
    {
        private static SimpleChar _aune;
        private static SimpleChar _redTower;
        private static SimpleChar _blueTower;

        private static bool _pos1 = false;
        private static bool _pos2 = false;
        private static bool _pos3 = false;
        private static bool _pos4 = false;

        public IState GetNextState()
        {
            _aune = DynelManager.NPCs
              .Where(c => c.Health > 0
                  && c.Name.Contains("Ground Chief Aune"))
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

            if (!WarpDB2._settings["Toggle"].AsBool())
                WarpDB2.NavMeshMovementController.Halt();

            if (Playfield.ModelIdentity.Instance != Constants.DB2Id)
                return new IdleState();

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id)
            {
                if (_pos4)
                    return new FightState();

                if (_redTower != null || _blueTower != null)
                    return new FightTowerState();

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
            Chat.WriteLine("Find Tower");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Tower Found");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            _aune = DynelManager.NPCs
             .Where(c => c.Health > 0
                 && c.Name.Contains("Ground Chief Aune")
                 && !c.Name.Contains("Remains of "))
             .FirstOrDefault();

            //if (_aune != null)
            //{
            //    if (DynelManager.LocalPlayer.FightingTarget.Name == _aune.Name)
            //        DynelManager.LocalPlayer.StopAttack();
            //}

            if (!_pos1 && !_pos2 && !_pos3 && !_pos4)
            {
                DynelManager.LocalPlayer.Position = Constants.Pos1;
                WarpDB2.NavMeshMovementController.SetMovement(MovementAction.Update);
                _pos1 = true;
            }
            else if (_pos1 && !_pos2 && !_pos3 && !_pos4)
            {
                DynelManager.LocalPlayer.Position = Constants.Pos2;
                WarpDB2.NavMeshMovementController.SetMovement(MovementAction.Update);
                _pos2 = true;
            }
            else if (_pos1 && _pos2 && !_pos3 && !_pos4)
            {
                DynelManager.LocalPlayer.Position = Constants.Pos3;
                WarpDB2.NavMeshMovementController.SetMovement(MovementAction.Update);
                _pos3 = true;
            }
            else if (_pos1 && _pos2 && _pos3 && !_pos4)
            {
                DynelManager.LocalPlayer.Position = Constants.Pos4;
                WarpDB2.NavMeshMovementController.SetMovement(MovementAction.Update);
                _pos4 = true;
            }
        }

    }
}