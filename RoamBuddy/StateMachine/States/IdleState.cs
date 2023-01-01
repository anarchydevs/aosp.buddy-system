namespace RoamBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (RoamBuddy.Toggle)
                return new RoamState();

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
