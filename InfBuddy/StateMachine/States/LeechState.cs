﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System.Linq;
using System.Threading.Tasks;

namespace InfBuddy
{
    public class LeechState : IState
    {
        private static bool _missionsLoaded = false;

        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (Extensions.CanExit(_missionsLoaded))
                return new ExitMissionState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("LeechState::OnStateEnter");

            DynelManager.LocalPlayer.Position = Constants.LeechSpot;
            MovementController.Instance.SetMovement(MovementAction.Update);
            MovementController.Instance.SetMovement(MovementAction.JumpStart);
            MovementController.Instance.SetMovement(MovementAction.Update);
        }

        public void OnStateExit()
        {
            Chat.WriteLine("LeechState::OnStateExit");

            _missionsLoaded = false;
            DynelManager.LocalPlayer.Position = new Vector3(160.4f, 2.6f, 103.0f);
        }

        public void Tick()
        {
            if (!_missionsLoaded && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                _missionsLoaded = true;
        }
    }
}
