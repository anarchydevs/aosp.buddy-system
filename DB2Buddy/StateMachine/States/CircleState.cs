using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class CircleState : IState
    {
        public static bool _init = false;
        public static bool _POS1 = false;
        public static bool _POS2 = false;

        private static double _time = Time.NormalTime;

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
            {
                DB2Buddy.NavMeshMovementController.Halt();
            }

            if (Playfield.ModelIdentity.Instance == Constants.PWId)
                return new IdleState();

            if (_redTower != null)
            {
                //DB2Buddy.NavMeshMovementController.Halt();
                //DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_redTower.Position);
                DynelManager.LocalPlayer.Position = _redTower.Position;
                DB2Buddy.NavMeshMovementController.SetMovement(MovementAction.Update);
                return new FightTowerState();
            }

            if (_blueTower != null
                && !DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
            {
                //DB2Buddy.NavMeshMovementController.Halt();
                //DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_blueTower.Position);
                DynelManager.LocalPlayer.Position = _blueTower.Position;
                DB2Buddy.NavMeshMovementController.SetMovement(MovementAction.Update);
                return new FightTowerState();
            }



            //if (_redTower == null && _blueTower == null
            //    && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPosition) > 15)
            //{
            //    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPosition);
            //    return new FightState();
            //}

            //{
            //    if (_aune == null
            //        || (_aune != null && !_aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients)
            //        && !DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy)
            //        && !MovementController.Instance.IsNavigating))
            //    {
            //        //DB2Buddy.NavMeshMovementController.Halt();
            //        DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPosition);
            //        //DynelManager.LocalPlayer.Position = (Constants._startPosition);
            //        //MovementController.Instance.SetMovement(MovementAction.Update);
            //        return new FightState();
            //    }
            //}

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("CircleState");

            _time = Time.NormalTime;
            FightState._taggedMist = false;
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit CircleState");
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

            if (DynelManager.LocalPlayer.FightingTarget != null
                && DynelManager.LocalPlayer.FightingTarget.Name == _aune.Name)
                DynelManager.LocalPlayer.StopAttack();

            //if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPosition) > 5
            //    && !MovementController.Instance.IsNavigating)
            //    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPosition);

            if (Time.NormalTime > _time + 2f)
            {
                _time = Time.NormalTime;

                if (!_init)
                {
                    DynelManager.LocalPlayer.Position = Constants.Pos1;
                    DB2Buddy.NavMeshMovementController.SetMovement(MovementAction.Update);

                    _init = true;
                    _POS1 = true;
                }
                else if (_POS1)
                {
                    DynelManager.LocalPlayer.Position = Constants.Pos2;
                    DB2Buddy.NavMeshMovementController.SetMovement(MovementAction.Update);

                    _POS1 = false;
                    _POS2 = true;
                }
                else if (_POS2)
                {
                    DynelManager.LocalPlayer.Position = Constants.Pos3;
                    DB2Buddy.NavMeshMovementController.SetMovement(MovementAction.Update);

                    _POS2 = false;
                }
                else
                {
                    DynelManager.LocalPlayer.Position = Constants.Pos4;
                    DB2Buddy.NavMeshMovementController.SetMovement(MovementAction.Update);


                    _init = false;
                }
            }
        }

    }
}