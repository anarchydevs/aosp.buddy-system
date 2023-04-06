namespace AttackBuddy
{
    public interface IState
    {
        void Tick();
        IState GetNextState();
        void OnStateEnter();
        void OnStateExit();
    }
}
