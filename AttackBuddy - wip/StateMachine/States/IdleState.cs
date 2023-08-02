using AOSharp.Core.UI;

namespace AttackBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (AttackBuddy._settings["Enable"].AsBool())
            {
                return new ScanState();
            }

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
