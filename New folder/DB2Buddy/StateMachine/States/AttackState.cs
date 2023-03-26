using AOSharp.Core;
using AOSharp.Core.Movement;
using SmokeLounge.AOtomation.Messaging.GameData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DB2Buddy
{
    public class AttackState : IState
    {
        public IState GetNextState()
        {
            if (Team.IsLeader
                && !DynelManager.NPCs.Any(c => c.Health > 0))
            {
                return new ToggleState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("AttackState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("AttackState::OnStateExit");
        }

        public void Tick()
        {

            SimpleChar aune = DynelManager.NPCs
           .Where(c => c.Health > 0
               && c.Name.Contains("Ground Chief Aune"))
           .FirstOrDefault();

            List<Dynel> towers = DynelManager.AllDynels
                .Where(c => c.Name.Contains("Strange Xan Artifact"))
                .OrderByDescending(c => DynelManager.LocalPlayer.Position.DistanceFrom(c.Position))
                .ToList();

            SimpleChar tower = DynelManager.NPCs
                .Where(c => c.Health > 0
                && c.Name.Contains("Strange Xan Artifact"))
                .FirstOrDefault();



            if (aune.Buffs.Contains(273220)
                || aune.Buffs.Contains(273221))
            {
                if (DynelManager.LocalPlayer.FightingTarget != null)
                    DynelManager.LocalPlayer.StopAttack();

                if (DynelManager.LocalPlayer.Position.DistanceFrom(towers.FirstOrDefault().Position) > 3f)
                    DynelManager.LocalPlayer.Position = towers.FirstOrDefault().Position;

                if (DynelManager.LocalPlayer.Position.DistanceFrom(towers.FirstOrDefault().Position) < 3f
                    && DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                {
                    if (tower != null)
                        DynelManager.LocalPlayer.Attack(tower);
                }
            }
            else
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(aune.Position) < 3f
                    && DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                {
                    if (aune != null)
                        DynelManager.LocalPlayer.Attack(aune);
                }
            }
            //if (Time.NormalTime > DB2Buddy._combatTime + 7f
            //    && _genCorpse == null
            //    && !Extensions.InCombat()
            //    && MovementController.Instance.IsNavigating == false
            //    && DynelManager.LocalPlayer.Position.DistanceFrom(DB2Buddy.ParkPos) > 2f)
            //{
            //    MovementController.Instance.SetDestination(DB2Buddy.ParkPos);
            //}

        }
    }
}
