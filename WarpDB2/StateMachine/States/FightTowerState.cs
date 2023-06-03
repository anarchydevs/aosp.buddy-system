using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages;
using AOSharp.Pathfinding;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using WarpDB2;

namespace WarpDB2
{
    public class FightTowerState : IState
    {
        private static SimpleChar _aune;
        private static SimpleChar _redTower;
        private static SimpleChar _blueTower;
        private static SimpleChar _mist;

        public IState GetNextState()
        {
            _aune = DynelManager.NPCs
              .Where(c => c.Health > 0
                  && c.Name.Contains("Ground Chief Aune")
                  && !c.Name.Contains("Remains of "))
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

            if (Playfield.ModelIdentity.Instance == Constants.PWId)
                return new IdleState();

            if (_redTower == null && _blueTower == null)
            {
                if (_aune != null && !_aune.Buffs.Contains(WarpDB2.Nanos.StrengthOfTheAncients)
                 && !DynelManager.LocalPlayer.Buffs.Contains(WarpDB2.Nanos.XanBlessingoftheEnemy))
                {
                    return new FightState();
                }

                if (_aune == null)
                {
                    DynelManager.LocalPlayer.Position = Constants._centerPosition;
                }
            }

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

            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.first) < 60)
                return new FellState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine($"FightTowerState");
        }

        public void OnStateExit()
        {
            WarpDB2.NavMeshMovementController.Halt();
            Chat.WriteLine("Exit FightTowerState");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            foreach (TeamMember member in Team.Members)
            {
                if (!ReformState._teamCache.Contains(member.Identity))
                    ReformState._teamCache.Add(member.Identity);
            }

            _aune = DynelManager.NPCs
             .Where(c => c.Health > 0
                 && c.Name.Contains("Ground Chief Aune")
                 && !c.Name.Contains("Remains of "))
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

            //if (_aune != null)
            //{
            //    if (DynelManager.LocalPlayer.FightingTarget.Name == _aune.Name)
            //        DynelManager.LocalPlayer.StopAttack();
            //}

            if (_redTower != null)
            {
                if (_redTower.IsInLineOfSight
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) < 3f
                    && DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttackPending)
                    DynelManager.LocalPlayer.Attack(_redTower);

                if (DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) > 3f)
                {
                    DynelManager.LocalPlayer.Position = _redTower.Position;
                    MovementController.Instance.SetMovement(MovementAction.Update);
                }
            }
            else if (_blueTower != null)
            {
                if (!DynelManager.LocalPlayer.Buffs.Contains(WarpDB2.Nanos.XanBlessingoftheEnemy)
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) < 3f
                    && DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttackPending)
                    DynelManager.LocalPlayer.Attack(_blueTower);

                if (DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) > 3f
                    && !DynelManager.LocalPlayer.Buffs.Contains(WarpDB2.Nanos.XanBlessingoftheEnemy))
                { 
                    DynelManager.LocalPlayer.Position = _blueTower.Position;
                    MovementController.Instance.SetMovement(MovementAction.Update);
                }
            }
        }

    }
}