using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;

namespace AXPBuddy
{
    public class LeechState : IState
    {
        private static double _stuck;
        private static double _timeOut;

        private static bool _init = false;
        private static bool _initTeam = false;

        public IState GetNextState()
        {
            if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.APFHubId
                && !Team.IsInTeam
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13ZoneOutPos) <= 10f)
                return new ReformState();

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("LeechState::OnStateEnter");

            DynelManager.LocalPlayer.Position = new Vector3(150.8, 67.7f, 42.0f);
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("LeechState::OnStateExit");

            _initTeam = false;
        }

        public void Tick()
        {
            if (!Team.IsInTeam || Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f) { return; }

            if (DynelManager.LocalPlayer.Position.Distance2DFrom(Constants.S13GoalPos) <= 30f
                && !_initTeam)
            {
                foreach (TeamMember member in Team.Members)
                {
                    if (!ReformState._teamCache.Contains(member.Identity))
                        ReformState._teamCache.Add(member.Identity);
                }

                _initTeam = true;
            }

            if (!AXPBuddy._died && Playfield.ModelIdentity.Instance == Constants.S13Id)
                AXPBuddy._ourPos = DynelManager.LocalPlayer.Position;

            if (!AXPBuddy._initMerge && AXPBuddy._settings["Merge"].AsBool())
            {
                if (!AXPBuddy._initMerge)
                    AXPBuddy._initMerge = true;

                AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos);
            }

            if (AXPBuddy._died)
            {
                if (AXPBuddy._ourPos != Vector3.Zero)
                {
                    if (AXPBuddy.NavMeshMovementController.IsNavigating && Time.NormalTime > _stuck + 30f)
                    {
                        DynelManager.LocalPlayer.Position = new Vector3(DynelManager.LocalPlayer.Position.X, DynelManager.LocalPlayer.Position.Y, DynelManager.LocalPlayer.Position.Z + 4f);
                        _stuck = Time.NormalTime;
                    }

                    if (!AXPBuddy.NavMeshMovementController.IsNavigating && DynelManager.LocalPlayer.Position.DistanceFrom(AXPBuddy._ourPos) > 15f)
                    {
                        AXPBuddy.NavMeshMovementController.SetNavMeshDestination(AXPBuddy._ourPos);
                        _stuck = Time.NormalTime;
                    }

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(AXPBuddy._ourPos) < 15f)
                        if (AXPBuddy._died)
                            AXPBuddy._died = false;
                }
            }

            AXPBuddy._leader = Team.Members
                .Where(c => c.Character?.Health > 0
                    && c.Character?.IsValid == true
                    && (c.Identity == AXPBuddy.Leader || c.IsLeader ||
                            (AXPBuddy._settings["Merge"].AsBool()
                                && !string.IsNullOrEmpty(AXPBuddy.LeaderName)
                                && c.Character?.Name == AXPBuddy.LeaderName)))
                .FirstOrDefault()?.Character;

            if (AXPBuddy._leader != null)
            {
                if (AXPBuddy._died)
                    AXPBuddy._died = false;

                AXPBuddy._leaderPos = (Vector3)AXPBuddy._leader?.Position;

                //Reason: Edge correction
                if (AXPBuddy.NavMeshMovementController.IsNavigating)
                {
                    if (DynelManager.LocalPlayer.Position.Distance2DFrom(new Vector3(169.5f, 36.0f, 164.3f)) > 15f)
                        _init = false;

                    if (Time.NormalTime > _timeOut + 10f && _init)
                    {
                        AXPBuddy.NavMeshMovementController.Halt();
                        DynelManager.LocalPlayer.Position = new Vector3(AXPBuddy._leaderPos.X, 67.7f, AXPBuddy._leaderPos.Z);
                    }

                    if (DynelManager.LocalPlayer.Position.Distance2DFrom(new Vector3(169.5f, 36.0f, 164.3f)) <= 15f)
                    {
                        if (!_init)
                        {
                            _init = true;
                            _timeOut = Time.NormalTime;
                        }
                    }
                }

                //Reason: Edge correction: Leecher stuck trying to navigate to failed path.
                if (DynelManager.LocalPlayer.Position.Distance2DFrom(AXPBuddy._leaderPos) > 2f
                    && DynelManager.LocalPlayer.Position.Distance2DFrom(Constants.S13GoalPos) > 30f
                    && DynelManager.LocalPlayer.Position.Distance2DFrom(new Vector3(137.7f, 67.6f, 490.3f)) <= 5f)
                    AXPBuddy.NavMeshMovementController.SetDestination(new Vector3(161.5f, 67.6f, 488.3f));

                if (DynelManager.LocalPlayer.Position.Distance2DFrom(AXPBuddy._leaderPos) > 2f
                    && DynelManager.LocalPlayer.Position.Distance2DFrom(Constants.S13GoalPos) > 30f
                    && DynelManager.LocalPlayer.Position.Distance2DFrom(new Vector3(137.7f, 67.6f, 490.3f)) > 5f)
                    AXPBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(AXPBuddy._leaderPos.X, 67.7f, AXPBuddy._leaderPos.Z));
            }
        }
    }
}