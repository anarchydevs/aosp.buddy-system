using AOSharp.Core.UI;

namespace PetHunt
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (PetHunt._settings["Enable"].AsBool())
            {
                return new ScanState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("Idle");
        }

        public void OnStateExit()
        {
            
        }

        public void Tick()
        {
        }
    }
}
