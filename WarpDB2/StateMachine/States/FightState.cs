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
using System.Runtime.ConstrainedExecution;

namespace WarpDB2
{
    public class FightState : IState
    {
        private static SimpleChar _aune;
        private static Corpse _auneCorpse;
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

            _mist = DynelManager.NPCs
              .Where(c => c.Name.Contains("Notum Irregularity"))
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

                if (_aune != null)
                {
                    if (DynelManager.LocalPlayer.Buffs.Contains(WarpDB2.Nanos.XanBlessingoftheEnemy)
                    || _aune.Buffs.Contains(WarpDB2.Nanos.StrengthOfTheAncients))
                    {
                        return new FindTowerState();
                    }
                }

                if (_redTower != null || _blueTower != null)
                    return new FightTowerState();

                if (WarpDB2.AuneCorpse
                    && Extensions.CanProceed()
                    && WarpDB2._settings["Farming"].AsBool())
                    return new FarmingState();
            }
            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine($"Fight State");
        }

        public void OnStateExit()
        {
            DynelManager.LocalPlayer.StopAttack();
            Chat.WriteLine("Exit Fight State");
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

            _auneCorpse = DynelManager.Corpses
               .Where(c => c.Name.Contains("Remains of Ground Chief Aune"))
               .FirstOrDefault();

            if (_aune != null)
            {
                if (DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttackPending
                    && !DynelManager.LocalPlayer.Buffs.Contains(WarpDB2.Nanos.XanBlessingoftheEnemy)
                    && !_aune.Buffs.Contains(WarpDB2.Nanos.StrengthOfTheAncients)
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) < 19)
                    DynelManager.LocalPlayer.Attack(_aune);

                if (DynelManager.LocalPlayer.Buffs.Contains(WarpDB2.Nanos.XanBlessingoftheEnemy)
                    || _aune.Buffs.Contains(WarpDB2.Nanos.StrengthOfTheAncients)
                    || _blueTower != null || _redTower != null)
                {
                    if (DynelManager.LocalPlayer.FightingTarget.Name == _aune.Name)
                        DynelManager.LocalPlayer.StopAttack();
                }

                if (DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) > 10)
                {
                    DynelManager.LocalPlayer.Position = _aune.Position;
                    MovementController.Instance.SetMovement(MovementAction.Update);
                }
            }

            if (_auneCorpse != null)
            {
                WarpDB2.AuneCorpse = true;
            }

        }

    }
}