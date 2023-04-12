using AOSharp.Core;
using AOSharp.Core.UI;

namespace InfBuddy
{
    public class IdleState : IState
    {
        public static bool _init = false;

        public IState GetNextState()
        {
            if (!InfBuddy.Toggle || !Team.IsInTeam)
                return null;

            if (Playfield.ModelIdentity.Instance == Constants.InfernoId)
            {
                foreach (Mission mission in Mission.List)
                    if (mission.DisplayName.Contains("The Purification"))
                        mission.Delete();

                return new MoveToQuestGiverState();
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
