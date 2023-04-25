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

namespace VortexxBuddy
{
    public class EnterState : IState
    {

        private static double _time;

        private const int MinWait = 3;
        private const int MaxWait = 5;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.VortexxId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPodium) < 5f)
                return new FightState();

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
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

            _cancellationToken.Cancel();
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && !Team.Members.Any(c => c.Character == null)
                && Time.NormalTime > _time + 2f)
            {
                _time = Time.NormalTime;

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20)
                {
                    VortexxBuddy.NavMeshMovementController.SetDestination(Constants._entrance);
                    VortexxBuddy.NavMeshMovementController.AppendDestination(Constants._reneterPos);
                }
            }

            if (Playfield.ModelIdentity.Instance == Constants.VortexxId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPodium) > 5)
                VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._centerPodium);

        }
    }
}