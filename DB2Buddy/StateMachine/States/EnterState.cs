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

namespace DB2Buddy
{
    public class EnterState : IState
    {
        private static bool _init = false;

        private static double _time;
        private static double _startTime;

        private static SimpleChar _aune;

        public IState GetNextState()
        {

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id
                 && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._atDoor) < 10f
                 && Team.IsInTeam
                 && Extensions.CanProceed()
                 && !Team.Members.Any(c => c.Character == null)
                 && DB2Buddy._settings["Toggle"].AsBool())
                return new PathToBossState();


            if (Playfield.ModelIdentity.Instance == Constants.PWId
                && !Extensions.CanProceed())
                return new IdleState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("EnterState");
            _time = Time.NormalTime;
            _startTime = Time.NormalTime;
            _init = true;
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit EnterState");
            DB2Buddy.AuneCorpse = false;
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

                DB2Buddy.NavMeshMovementController.SetDestination(Constants._entrance);
                DB2Buddy.NavMeshMovementController.AppendDestination(Constants._append);
            }
        }
    }
}