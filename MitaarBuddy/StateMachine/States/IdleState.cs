using AOSharp.Core;

namespace DB2Buddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (DB2Buddy._settings["Toggle"].AsBool()
                && DB2Buddy.Toggle)
            {
                DB2Buddy.ParkPos = DynelManager.LocalPlayer.Position;

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
