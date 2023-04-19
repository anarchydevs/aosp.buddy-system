using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using AOSharp.Common.GameData;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;

namespace Db1Buddy
{
    public class EnterState : IState
    {

        private static double _time;

        private const int MinWait = 3;
        private const int MaxWait = 5;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.DB1Id
                && !Team.Members.Any(c => c.Character == null)
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._atDoor) < 20f)
                return new StartState();

            if (Playfield.ModelIdentity.Instance == Constants.PWId
                && !Extensions.CanProceed())
                return new IdleState();

            return null;
        }

        public void OnStateEnter()
        {
            if (Extensions.CanProceed())
            {
                Chat.WriteLine("Entering");

                _time = Time.NormalTime;

            }
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("EnterSectorState::OnStateExit");

            _cancellationToken.Cancel();
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Playfield.ModelIdentity.Instance == Constants.PWId
                && Time.NormalTime > _time + 2f)
            {
                _time = Time.NormalTime;

                Db1Buddy.NavMeshMovementController.SetDestination(Constants._entrance);
                Db1Buddy.NavMeshMovementController.AppendDestination(new Vector3 (2119.3f, 3.2f, 2762.1f));

            }

        }
    }
}