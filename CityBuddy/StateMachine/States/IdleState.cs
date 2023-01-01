namespace CityBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {

            if (CityBuddy.Running == true)
                return new ToggleState();

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
