﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;

namespace InfBuddy
{
    public class SitState : IState
    {
        public IState GetNextState()
        {
            if (DynelManager.LocalPlayer.MovementState != MovementState.Sit)
                return new IdleState();

            return null;
        }

        public void OnStateEnter()
        {
        }

        public void OnStateExit()
        {  
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            
                if (DynelManager.LocalPlayer.MovementState == MovementState.Sit
                    && DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65)
                    InfBuddy.NavMeshMovementController.SetMovement(MovementAction.LeaveSit);
           
            
        }
    }
}