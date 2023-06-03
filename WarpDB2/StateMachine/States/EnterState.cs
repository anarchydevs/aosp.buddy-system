using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WarpDB2
{

    public class EnterState : IState
    {
        private static bool _init = false;

        private static double _time;
        private static double _startTime;

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

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._atDoor) < 10)
                    WarpDB2.NavMeshMovementController.SetNavMeshDestination(Constants._warpPos);

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._warpPos) < 5f
                 && Team.IsInTeam
                 && !Team.Members.Any(c => c.Character == null)
                 && WarpDB2._settings["Toggle"].AsBool())
                    return new PathToBossState();

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPosition) < 30f
                        && Team.IsInTeam)
                    return new FightState();
            }

            if (Playfield.ModelIdentity.Instance == Constants.PWId
                && !Extensions.CanProceed())
                return new IdleState();

            return null;
        }

        public void OnStateEnter()
        {

            WarpDB2.AuneCorpse = false;
            Chat.WriteLine("EnterState");
            _time = Time.NormalTime;
            _startTime = Time.NormalTime;
            _init = true;
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit EnterState");
            WarpDB2.AuneCorpse = false;
            _init = false;
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Playfield.ModelIdentity.Instance == Constants.PWId 
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance)< 5)
            {
                DynelManager.LocalPlayer.Position = Constants._centerofentrance;
                MovementController.Instance.SetMovement(MovementAction.Update);
            }

            if (Playfield.ModelIdentity.Instance == Constants.PWId
                && Time.NormalTime > _time + 2f)
            {
                _time = Time.NormalTime;

                WarpDB2.NavMeshMovementController.SetDestination(Constants._entrance);
                WarpDB2.NavMeshMovementController.AppendDestination(Constants._append);
            }
        }
    }
}