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



        public IState GetNextState()
        {
            _aune = DynelManager.NPCs
              .Where(c => c.Health > 0
                  && c.Name.Contains("Ground Chief Aune")
                  && !c.Name.Contains("Remains of "))
              .FirstOrDefault();

            if (!WarpDB2._settings["Toggle"].AsBool())
                WarpDB2.NavMeshMovementController.Halt();

            if (Playfield.ModelIdentity.Instance != Constants.DB2Id)
                return new IdleState();

            if (WarpDB2.AuneCorpse
                        && Extensions.CanProceed()
                        && WarpDB2._settings["Farming"].AsBool())
                return new FarmingState();

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

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine($"Fight State");
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


            if (_redTower != null || _blueTower != null)
            {
                if (_redTower != null)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) > 3)
                    {
                        Task.Factory.StartNew(
                                   async () =>
                                   {
                                       await Task.Delay(3000);
                                       DynelManager.LocalPlayer.Position = _redTower.Position;
                                       await Task.Delay(1000);
                                       MovementController.Instance.SetMovement(MovementAction.Update);
                                       await Task.Delay(1000);
                                       MovementController.Instance.SetMovement(MovementAction.Update);
                                   });

                    }

                    if (DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttackPending
                        && DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) < 5)
                    {
                        DynelManager.LocalPlayer.Attack(_redTower);
                    }
                }

                else if (_blueTower != null && !DynelManager.LocalPlayer.Buffs.Contains(WarpDB2.Nanos.XanBlessingoftheEnemy))
                {

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) > 3)
                    {
                        Task.Factory.StartNew(
                                   async () =>
                                   {
                                       await Task.Delay(3000);
                                       DynelManager.LocalPlayer.Position = _blueTower.Position;
                                       await Task.Delay(1000);
                                       MovementController.Instance.SetMovement(MovementAction.Update);
                                       await Task.Delay(1000);
                                       MovementController.Instance.SetMovement(MovementAction.Update);
                                   });

                    }

                    if (DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttackPending
                        && DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) < 5)
                    {
                        DynelManager.LocalPlayer.Attack(_blueTower);
                    }
                }
            }



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

                if (_blueTower == null && _redTower == null
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) > 10
                    && !Extensions.Debuffed())
                {
                    if (_aune != null)
                    {
                        Task.Factory.StartNew(
                                  async () =>
                                  {
                                      await Task.Delay(2000);
                                      DynelManager.LocalPlayer.Position = _aune.Position;
                                      await Task.Delay(1000);
                                      MovementController.Instance.SetMovement(MovementAction.Update);
                                      await Task.Delay(1000);
                                      MovementController.Instance.SetMovement(MovementAction.Update);
                                  });
                    }

                    if (_aune == null)
                    {
                        WarpDB2.NavMeshMovementController.SetNavMeshDestination(Constants._startPosition);
                    }
                }

            }

            if (_auneCorpse != null)
                WarpDB2.AuneCorpse = true;
        }

    }
}