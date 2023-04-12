using AOSharp.Core;

namespace AXPBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (AXPBuddy.Toggle == true && Team.IsInTeam && Team.IsRaid
                && AXPBuddy._settings["Toggle"].AsBool())
            {
                return new EnterSectorState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("IdleState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("IdleState::OnStateExit");
        }

        public void Tick()
        {
        }
    }
}
