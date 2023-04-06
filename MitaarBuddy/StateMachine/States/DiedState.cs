using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using MitaarBuddy.IPCMessages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages.OrgServerMessages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MitaarBuddy
{
    public class DiedState : IState
    {
        public IState GetNextState()
        {
            if ((DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 10.0f)
                && Extensions.CanProceed())
                return new EnterState();

            return null;
        }

        public void OnStateEnter()
        {
            MitaarBuddy._died = true;

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
                    MitaarBuddy.NavMeshMovementController.SetMovement(MovementAction.LeaveSit);

                if (DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65
                    && DynelManager.LocalPlayer.GetStat(Stat.TemporarySkillReduction) <= 1
                    && DynelManager.LocalPlayer.MovementState != MovementState.Sit
                    && Playfield.ModelIdentity.Instance == Constants.XanHubId && !MitaarBuddy.NavMeshMovementController.IsNavigating)
                    MovementController.Instance.SetPath(Constants._pathToMitaar);
            }
        }
    }
}