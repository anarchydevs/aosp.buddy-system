﻿using AOSharp.Common.GameData;
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

            if (!DB2Buddy._settings["Toggle"].AsBool())
                DB2Buddy.NavMeshMovementController.Halt();

            if (Playfield.ModelIdentity.Instance == Constants.PWId)
                return new IdleState();

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id)
            {
                Network.ChatMessageReceived += (s, msg) =>
                {
                    if (msg.PacketType != ChatMessageType.NpcMessage)
                        return;

                    var npcMsg = (NpcMessage)msg;

                    string[] triggerMsg = new string[2] { "Know the power of the Xan", "You will never know the secrets of the machine" };

                    if (triggerMsg.Any(x => npcMsg.Text.Contains(x)))
                    {
                        DB2Buddy._taggedNotum = true;
                    }
                };

                if (DB2Buddy._taggedNotum)
                {
                    return new NotumState();
                }

                if (_redTower == null && _blueTower == null)
                {
                    if (_aune != null && !_aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients)
                     && !DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
                    {
                        return new FightState();
                    }

                    if (_aune == null)
                        DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPosition);
                }

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.first) < 60)
                    return new FellState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            DB2Buddy.NavMeshMovementController.Halt();
            Chat.WriteLine($"FightTowerState");
        }

        public void OnStateExit()
        {
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

            if (_redTower != null)
            {
                if (_redTower.IsInLineOfSight
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) < 3f
                    && DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttackPending)
                    DynelManager.LocalPlayer.Attack(_redTower);

                if (DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) > 3f
                    && !MovementController.Instance.IsNavigating)
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_redTower.Position);
            }
            if (_blueTower != null && _redTower == null)
            {
                if (!DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy)
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) < 3f
                    && DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttackPending)
                    DynelManager.LocalPlayer.Attack(_blueTower);

                if (DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) > 3f
                    && !DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy)
                    && !MovementController.Instance.IsNavigating)
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_blueTower.Position);
            }
        }

    }
}