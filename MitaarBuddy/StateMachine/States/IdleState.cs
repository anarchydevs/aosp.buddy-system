using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;

namespace MitaarBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.XanHubId)

            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20.0f
                    && Team.IsInTeam
                    && Extensions.CanProceed()
                    && MitaarBuddy._settings["Enable"].AsBool())
                {
                    return new EnterState();
                }

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) > 20.0f
                && Extensions.CanProceed()
                && MitaarBuddy._settings["Enable"].AsBool())
                {
                    return new DiedState();
                }
            }

            if (Playfield.ModelIdentity.Instance == Constants.MitaarId)
            {
                return new FightState();
            }

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return new SitState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("IdleState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("IdleState::OnStateExit");
        }

        public void Tick()
        {
        }
    }
}
