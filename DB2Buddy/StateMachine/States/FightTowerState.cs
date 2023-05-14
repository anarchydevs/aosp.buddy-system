using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
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
            

            if (Playfield.ModelIdentity.Instance == Constants.PWId)
                return new IdleState();

            if (_redTower == null && _blueTower == null)
            {
                if (_aune != null && !_aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients)
                 && !DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy)
                 && !MovementController.Instance.IsNavigating)
                {
                    //DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_aune.Position);
                    DynelManager.LocalPlayer.Position = (Constants._startPosition);
                    MovementController.Instance.SetMovement(MovementAction.Update);
                    return new FightState();
                }

                if (_aune == null && !MovementController.Instance.IsNavigating)
                {
                    //DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPosition);
                    DynelManager.LocalPlayer.Position = (Constants._startPosition);
                    MovementController.Instance.SetMovement(MovementAction.Update);
                    return new FightState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine($"FightTowerState");
            FightState._taggedMist = false;
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

            if (_redTower != null && !MovementController.Instance.IsNavigating)
            {
                if (_redTower.IsInLineOfSight
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) < 3f
                    && DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttackPending)
                    DynelManager.LocalPlayer.Attack(_redTower);

                //if (DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) > 3f)
                //    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_redTower.Position);

                if (DynelManager.LocalPlayer.Position.DistanceFrom(_redTower.Position) > 3f)
                {
                    DynelManager.LocalPlayer.Position = _redTower.Position;
                    MovementController.Instance.SetMovement(MovementAction.Update);
                }

            }
            else if (_blueTower != null && !MovementController.Instance.IsNavigating)
            {
                if (!DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy)
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) < 3f
                    && DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttackPending)
                    DynelManager.LocalPlayer.Attack(_blueTower);

                if (DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) > 3f
                    && !DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
                    //DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_blueTower.Position);

                //if (DynelManager.LocalPlayer.Position.DistanceFrom(_blueTower.Position) > 3f)
                {
                    DynelManager.LocalPlayer.Position = _blueTower.Position;
                    MovementController.Instance.SetMovement(MovementAction.Update);
                }

            }
        }

    }
}