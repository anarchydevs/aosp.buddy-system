﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;

namespace Db1Buddy
{
    public class DiedState : IState
    {
        public IState GetNextState()
        {
            if ((DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20.0f)
                && Extensions.CanProceed())
                //&& !Team.Members.Any(c => c.Character == null))
                return new EnterState();

            return null;
        }

        public void OnStateEnter()
        {
            Db1Buddy._died = true;

            Chat.WriteLine($"DiedState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("DiedState::OnStateExit");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._reclaim) < 10.0f)
            {
                if (DynelManager.LocalPlayer.MovementState == MovementState.Sit
                    && DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65)
                    Db1Buddy.NavMeshMovementController.SetMovement(MovementAction.LeaveSit);

                if (DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65
                    && DynelManager.LocalPlayer.GetStat(Stat.TemporarySkillReduction) <= 1
                    && DynelManager.LocalPlayer.MovementState != MovementState.Sit
                    && Playfield.ModelIdentity.Instance == Constants.PWId && !Db1Buddy.NavMeshMovementController.IsNavigating)
                    MovementController.Instance.SetPath(Constants._pathToMitaar);
            }
        }
    }
}