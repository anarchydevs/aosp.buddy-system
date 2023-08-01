namespace AttackBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (AttackBuddy.Toggle)
                return new ScanState();

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
