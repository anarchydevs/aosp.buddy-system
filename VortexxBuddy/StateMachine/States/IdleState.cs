using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;

namespace VortexxBuddy
{
    public class IdleState : IState
    {


        public IState GetNextState()
        {

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId && VortexxBuddy._settings["Toggle"].AsBool())
            {
                if (!Team.IsInTeam && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._startPos) > 3)
                    VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPos);

                if (!Team.IsInTeam && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._startPos) < 3)
                    return new ReformState();

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20f
                    && !VortexxBuddy._settings["Clear"].AsBool()
                    && Team.IsInTeam
                    && Extensions.CanProceed())
                    return new EnterState();

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) > 20f
                    && Extensions.CanProceed())
                    return new DiedState();
            }

            if (Playfield.ModelIdentity.Instance == Constants.VortexxId)
                return new FightState();

            

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("IdleState");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("Exit IdleState");
        }

        public void Tick()
        {
        }
    }
}
