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

namespace DB2Buddy
{
    public class FightState : IState
    {
        private static SimpleChar _aune;
        private static Corpse _auneCorpse;
        private static SimpleChar _redTower;
        private static SimpleChar _blueTower;
        private static SimpleChar _mist;

        private static bool _taggedNotum = false;
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

            if (Playfield.ModelIdentity.Instance != Constants.DB2Id)
                return new IdleState();

            if (DB2Buddy.AuneCorpse
                        && Extensions.CanProceed()
                        && DB2Buddy._settings["Farming"].AsBool())
                return new FarmingState();

            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.first) < 60)
                return new FellState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine($"Fight State");

            _mistCycle = Time.NormalTime;
        }

        public void OnStateExit()
        {
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

            Network.ChatMessageReceived += (s, msg) =>
            {
                if (msg.PacketType != ChatMessageType.NpcMessage)
                    return;

                var npcMsg = (NpcMessage)msg;

                string[] triggerMsg = new string[2] { "Know the power of the Xan", "You will never know the secrets of the machine" };

                if (triggerMsg.Any(x => npcMsg.Text.Contains(x)))
                {
                    _taggedNotum = true;
                }
            };

            if (_taggedNotum)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(_mist.Position) < 0.5)
                    _taggedNotum = false;

                if (DynelManager.LocalPlayer.Position.DistanceFrom(_mist.Position) > 0.5)
                    DynelManager.LocalPlayer.Position = _mist.Position;
                    //DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_mist.Position);
            }

            if (_redTower != null && !MovementController.Instance.IsNavigating)
            {
                if (_redTower.IsInLineOfSight
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) < 3f
                    && DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttackPending)
                    DynelManager.LocalPlayer.Attack(_redTower);

                if (DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) > 3f)
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_redTower.Position);

            }
            if (_blueTower != null && !MovementController.Instance.IsNavigating)
            {
                if (!DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy)
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) < 3f
                    && DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttackPending)
                    DynelManager.LocalPlayer.Attack(_blueTower);

                if (DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) > 3f
                    && !DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_blueTower.Position);


            }

            if (_auneCorpse != null)
                DB2Buddy.AuneCorpse = true;

            if (_aune != null) //&& _blueTower == null &&  _redTower == null )
            {

                if (DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttackPending
                    && !DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy)
                    && !_aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients)
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) < 19
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

                if (_blueTower == null && _redTower == null
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) > 10f
                    && !MovementController.Instance.IsNavigating && !Extensions.Debuffed())
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_aune.Position);

            }
        }

    }
}