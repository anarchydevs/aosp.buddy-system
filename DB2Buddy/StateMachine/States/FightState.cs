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

namespace DB2Buddy
{
    public class FightState : IState
    {
        private static SimpleChar _aune;
        private static Corpse _auneCorpse;
        private static SimpleChar _mist;
        private static SimpleChar _redTower;
        private static SimpleChar _blueTower;

        private static bool _init = false;
        private static double _time;
        private static double _mistCycle;
        public static bool _taggedMist = false;

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

            if (DB2Buddy.AuneCorpse
                        && Extensions.CanProceed()
                        && DB2Buddy._settings["Farming"].AsBool())
                return new FarmingState();

            if (_aune != null)
            {
                if (_aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients)
                 || DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
                    return new FightTowerState();
            }

            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.first) < 30)
                return new FellState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine($"FightBossState");

            _mistCycle = Time.NormalTime;
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit FightBossState");
            DynelManager.LocalPlayer.StopAttack();
            _init = false;
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
            
            _mist = DynelManager.NPCs
                .Where(c => c.Name.Contains("Notum Irregularity"))
                .FirstOrDefault();

            if (_auneCorpse != null)
                DB2Buddy.AuneCorpse = true;

            Network.ChatMessageReceived += (s, msg) =>
            {
                if (msg.PacketType != ChatMessageType.NpcMessage)
                    return;

                var npcMsg = (NpcMessage)msg;

                string[] triggerMsg = new string[2] { "Know the power of the Xan", "You will never know the secrets of the machine" };

                if (triggerMsg.Any(x => npcMsg.Text.Contains(x)) )
                {
                    //if (Extensions.Debuffed()) 
                    //    return;

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_mist.Position) > 0.1)
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_mist.Position);
                }
            };

            if (_aune != null)
            {
                //if (DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) < 10
                //    && MovementController.Instance.IsNavigating)
                //    DB2Buddy.NavMeshMovementController.Halt();

                if (DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttackPending
                    && !DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy)
                    && !_aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients)
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) < 20
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

                if (_mist == null && _blueTower == null && _redTower == null 
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) > 10f
                    && !MovementController.Instance.IsNavigating && !Extensions.Debuffed())
                {
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_aune.Position);

                    //DynelManager.LocalPlayer.Position = _aune.Position;
                    //MovementController.Instance.SetMovement(MovementAction.Update);
                }

                //if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._startPosition) > 130)
                //{
                //    DynelManager.LocalPlayer.Position = Constants._startPosition;
                //    MovementController.Instance.SetMovement(MovementAction.Update);
                //}
            }
        }
       
    }
}