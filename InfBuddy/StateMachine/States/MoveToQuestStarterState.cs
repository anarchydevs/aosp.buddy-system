﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System.Linq;
using System.Threading.Tasks;

namespace InfBuddy
{
    public class MoveToQuestStarterState : IState
    {
        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (!InfBuddy.NavMeshMovementController.IsNavigating && Extensions.IsAtStarterPos())
            {
                if (DynelManager.LocalPlayer.Identity == InfBuddy.Leader)
                    return new StartMissionState();

                Constants.DefendPos = new Vector3(165.6f, 2.2f, 186.4f);
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.DefendPos);
                return new DefendSpiritState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("MoveToQuestStarter::OnStateEnter");

            InfBuddy._stateTimeOut = Time.NormalTime;

            if (!InfBuddy.NavMeshMovementController.IsNavigating && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.QuestStarterBeforePos) > 4f)
            {
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.QuestStarterBeforePos);
                InfBuddy.NavMeshMovementController.AppendNavMeshDestination(Constants.QuestStarterPos);
            }
        }

        public void OnStateExit()
        {
            Chat.WriteLine("MoveToQuestStarter::OnStateExit");
        }

        public void Tick()
        {
        }
    }
}
