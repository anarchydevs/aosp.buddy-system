using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;

namespace Db1Buddy
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
            Db1Buddy._died = true;

            Chat.WriteLine($"SitState");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit SitState");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            
                if (DynelManager.LocalPlayer.MovementState == MovementState.Sit
                    && DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65)
                    Db1Buddy.NavMeshMovementController.SetMovement(MovementAction.LeaveSit);
           
            
        }
    }
}