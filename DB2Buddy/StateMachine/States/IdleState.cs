using AOSharp.Core;

namespace DB2Buddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (DB2Buddy.Toggle == true && Team.IsInTeam
                && DB2Buddy._settings["Toggle"].AsBool())
            {
                return new EnterState();
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
