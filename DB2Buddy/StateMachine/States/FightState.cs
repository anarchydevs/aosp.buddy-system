using AOSharp.Common.GameData;
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
using AOSharp.Pathfinding;
using System.Text.RegularExpressions;

namespace DB2Buddy
{
    public class FightState : IState
    {
        private SimpleChar _aune;
        private Corpse _auneCorpse;
        private SimpleChar _redTower;
        private SimpleChar _blueTower;

        private string previousErrorMessage = string.Empty;

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
                        if (!FightTowerState._towerPOS.ContainsKey(_redTower.Position))
                        {
                            FightTowerState._towerPOS[_redTower.Position] = _redTower.Name;
                        }

                        return new FightTowerState();
                    }

                    if (_blueTower != null || _aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients))
                    {
                        if (!FightTowerState._towerPOS.ContainsKey(_blueTower.Position))
                        {
                            FightTowerState._towerPOS[_blueTower.Position] = _blueTower.Name;
                        }

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
            //DB2Buddy.NavMeshMovementController.Halt();
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit FightBossState");
            DynelManager.LocalPlayer.StopAttack();
        }

        public void Tick()
        {
            try
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
                        && _aune.IsInAttackRange(true)
                        //&& DynelManager.LocalPlayer.IsInAttackRange(true)
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

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) > 20
                        || !_aune.IsInAttackRange(true)) //&& DynelManager.LocalPlayer.IsInAttackRange(true)))
                        DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_aune.Position, out NavMeshPath path);

                    if (_aune.IsInLineOfSight && _aune.IsInAttackRange(true) //&& DynelManager.LocalPlayer.IsInAttackRange(true)
                        && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPosition) < 20)
                        DB2Buddy.NavMeshMovementController.Halt();

                }
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    previousErrorMessage = errorMessage;
                }
            }
        }
        private int GetLineNumber(Exception ex)
        {
            var lineNumber = 0;

            var lineMatch = Regex.Match(ex.StackTrace ?? "", @":line (\d+)$", RegexOptions.Multiline);

            if (lineMatch.Success)
                lineNumber = int.Parse(lineMatch.Groups[1].Value);

            return lineNumber;
        }

    }
}