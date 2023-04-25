using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;

namespace VortexxBuddy
{
    public class IdleState : IState
    {
        private static SimpleChar _vortexx;

        private static Corpse _vortexxCorpse;
        public IState GetNextState()
        {
            _vortexx = DynelManager.NPCs
                .Where(c => c.Health > 0
                 && c.Name.Contains("Ground Chief Vortexx")
                 && !c.Name.Contains("Remains of"))
                 .FirstOrDefault();

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20f
                && Team.IsInTeam
                && Extensions.CanProceed()
                && VortexxBuddy._settings["Toggle"].AsBool())
                return new EnterState();

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) > 20f
                && Extensions.CanProceed()
                && VortexxBuddy._settings["Toggle"].AsBool())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.VortexxId 
                && _vortexx != null)
                return new FightState();

            if (Playfield.ModelIdentity.Instance == Constants.VortexxId
                && VortexxBuddy.VortexxCorpse
               && Extensions.CanProceed()
               && VortexxBuddy._settings["Farming"].AsBool())
                return new FarmingState();

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return new SitState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("IdleState");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit IdleState");
        }

        public void Tick()
        {
        }
    }
}
