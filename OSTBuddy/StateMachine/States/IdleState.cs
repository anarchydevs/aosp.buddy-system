namespace OSTBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (OSTBuddy.Toggle == true)
                return new PullState();

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
