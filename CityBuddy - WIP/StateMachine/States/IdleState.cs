using AOSharp.Core;

namespace CityBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (CityBuddy._settings["Toggle"].AsBool()
                && CityBuddy.Toggle)
            {
                CityBuddy.ParkPos = DynelManager.LocalPlayer.Position;

                if (Team.IsLeader)
                    return new ToggleState();
                else
                    return new AttackState();
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
