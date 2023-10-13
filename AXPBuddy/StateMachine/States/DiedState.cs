using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;

namespace AXPBuddy
{
    public class DiedState : IState
    {
        public IState GetNextState()
        {
            if (Playfield.ModelId == PlayfieldId.Sector13 && Team.Members.Any(c => c.Character != null))
            {
                switch ((AXPBuddy.ModeSelection)AXPBuddy._settings["ModeSelection"].AsInt32())
                {
                    case AXPBuddy.ModeSelection.Leech:
                        if (AXPBuddy._settings["Merge"].AsBool() || (!Team.Members.Any(c => c.Character == null)))
                        {
                            return new LeechState();
                        }
                        break;

                    case AXPBuddy.ModeSelection.Path:
                        if (AXPBuddy._settings["Merge"].AsBool() || (!Team.Members.Any(c => c.Character == null)))
                        {
                            return new PathState();
                        }
                        break;

                    default:
                        if (AXPBuddy._settings["Merge"].AsBool() || (!Team.Members.Any(c => c.Character == null)))
                        {
                            return new PullState();
                        }
                        break;
                }
            }

            return null;
        }

        public void OnStateEnter()
        {

            Chat.WriteLine($"DiedState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("DiedState::OnStateExit");
        }

        public void Tick()
        {
            if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f) { return; }

            Dynel Lever = DynelManager.AllDynels
                .Where(c => c.Name == "A Lever"
                    && c.DistanceFrom(DynelManager.LocalPlayer) < 6f)
                .FirstOrDefault();

            if (DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65
                && DynelManager.LocalPlayer.GetStat(Stat.TemporarySkillReduction) <= 1
                && DynelManager.LocalPlayer.MovementState != MovementState.Sit
                && !AXPBuddy.NavMeshMovementController.IsNavigating)
            {

                if (Playfield.ModelIdentity.Instance == Constants.UnicornHubId)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.UnicornLever) > 6)
                    {
                        AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.UnicornLever);
                    }

                    else if (Lever != null)
                    {
                        AXPBuddy.NavMeshMovementController.Halt();
                        Lever.Use();
                    }
                }
                else if (Playfield.ModelIdentity.Instance == Constants.APFHubId)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13EntrancePos) > 6)
                    {
                        AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13EntrancePos);
                    }
                }
                else if (Playfield.ModelIdentity.Instance == Constants.S13Id)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13GoalPos) > 6)
                    {
                        AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos);
                    }
                }
            }     
        }
    }
}