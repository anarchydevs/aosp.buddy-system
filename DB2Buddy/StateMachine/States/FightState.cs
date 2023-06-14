﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
<<<<<<< HEAD
=======
using AOSharp.Pathfinding;
>>>>>>> aab7ee3ccaa03c6ad6b10dee74da529f4148bb84

namespace DB2Buddy
{
    public class FightState : IState
    {
        private static SimpleChar _aune;
        private static Corpse _auneCorpse;
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

            if (Playfield.ModelIdentity.Instance != Constants.DB2Id)
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

                if (_aune != null)
                {
                    if (_redTower != null || DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
                    {
                        return new FightTowerState();
                    }

                    if (_blueTower != null || _aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients))
                    {
                        if (!DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
                            return new FightTowerState();
                    }
                }

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.first) < 60)
                    return new FellState();

                if (DB2Buddy.AuneCorpse
                        && Extensions.CanProceed()
                        && DB2Buddy._settings["Farming"].AsBool())
                    return new FarmingState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine($"FightBossState");
            DB2Buddy.NavMeshMovementController.Halt();
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit FightBossState");
            DynelManager.LocalPlayer.StopAttack();
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

            _auneCorpse = DynelManager.Corpses
               .Where(c => c.Name.Contains("Remains of Ground Chief Aune"))
               .FirstOrDefault();

            if (_auneCorpse != null)
                DB2Buddy.AuneCorpse = true;

            if (_aune != null)
            {
                if (DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttackPending
                    && !DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy)
                    && !_aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients)
                    && _aune.IsInAttackRange()
                    && !MovementController.Instance.IsNavigating)
                    DynelManager.LocalPlayer.Attack(_aune);

                if (DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy)
                    || _aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients)
                    || MovementController.Instance.IsNavigating)
                {
                    if (DynelManager.LocalPlayer.FightingTarget != null
                        && DynelManager.LocalPlayer.FightingTarget.Name == _aune.Name)
                        DynelManager.LocalPlayer.StopAttack();
                }

                if (DynelManager.LocalPlayer.Position.DistanceFrom (_aune.Position) > 20
<<<<<<< HEAD
                    && !MovementController.Instance.IsNavigating)
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_aune.Position);

                if (_aune.IsInLineOfSight && _aune.IsInAttackRange()
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) < 12)
=======
                    || !_aune.IsInAttackRange())
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_aune.Position, out NavMeshPath path);

                if (_aune.IsInLineOfSight && _aune.IsInAttackRange()
                    && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPosition) < 20)
>>>>>>> aab7ee3ccaa03c6ad6b10dee74da529f4148bb84
                    DB2Buddy.NavMeshMovementController.Halt();

            }
        }
       
    }
}