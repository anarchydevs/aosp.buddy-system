using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using AOSharp.Common.GameData;
using System.Linq;
using System.Collections.Generic;

namespace MitaarBuddy
{
    public class EnterState : IState
    {
        private const int MinWait = 3;
        private const int MaxWait = 5;
        private static double _time;
        private double _nextMoveTime;

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.MitaarId)
            {
                return new FightState();
            }

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && !Extensions.CanProceed())
                return new IdleState();

            return null;
        }

        public void OnStateEnter()
        {
            MitaarBuddy.SinuhCorpse = false;
            Chat.WriteLine("Entering Mitaar");
            _nextMoveTime = Time.NormalTime + 2; // Initialize next movement time
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("EnterSectorState::OnStateExit");

        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Team.IsInTeam && Time.NormalTime > _nextMoveTime)
            {
                _nextMoveTime = Time.NormalTime + 2; // Schedule next movement in 2 seconds

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20)
                {
                    MitaarBuddy.NavMeshMovementController.SetDestination(Constants._entrance);
                    MitaarBuddy.NavMeshMovementController.AppendDestination(Constants._reneterPos);
                }
            }
        }
    }
}