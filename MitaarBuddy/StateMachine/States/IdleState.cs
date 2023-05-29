using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;

namespace MitaarBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.XanHubId)

            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20.0f
                    && Team.IsInTeam
                    && Extensions.CanProceed()
                    && MitaarBuddy._settings["Toggle"].AsBool())
                {
                    return new EnterState();
                }

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) > 20.0f
                && Extensions.CanProceed()
                && MitaarBuddy._settings["Toggle"].AsBool())
                {
                    return new DiedState();
                }
            }

            if (Playfield.ModelIdentity.Instance == Constants.MitaarId)
            {
                if (MitaarBuddy.DifficultySelection.Easy == (MitaarBuddy.DifficultySelection)MitaarBuddy._settings["DifficultySelection"].AsInt32())
                    if (MitaarBuddy._died || (!MitaarBuddy._died && !Team.Members.Any(c => c.Character == null)))
                        return new EasyState();

                if (MitaarBuddy.DifficultySelection.Medium == (MitaarBuddy.DifficultySelection)MitaarBuddy._settings["DifficultySelection"].AsInt32())
                    if (MitaarBuddy._died || (!MitaarBuddy._died && !Team.Members.Any(c => c.Character == null)))
                        return new MediumState();

                if (MitaarBuddy.DifficultySelection.Hardcore == (MitaarBuddy.DifficultySelection)MitaarBuddy._settings["DifficultySelection"].AsInt32())
                    if (MitaarBuddy._died || (!MitaarBuddy._died && !Team.Members.Any(c => c.Character == null)))
                        return new HardcoreState();
            }

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return new SitState();

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
