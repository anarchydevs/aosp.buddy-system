namespace DefendBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (DefendBuddy.Toggle == true)
                return new DefendState();

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
