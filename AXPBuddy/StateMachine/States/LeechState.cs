using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AXPBuddy
{
    public class LeechState : IState
    {
        private static double _stuck;

        public IState GetNextState()
        {
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
            Chat.WriteLine("LeechState::OnStateEnter");

            DynelManager.LocalPlayer.Position = new Vector3(150.8, 67.7f, 42.0f);
        }

        public void OnStateExit()
        {
            Chat.WriteLine("LeechState::OnStateExit");
        }

        public void Tick()
        {
            if (!Team.IsInTeam || Game.IsZoning) { return; }

            foreach (TeamMember member in Team.Members)
            {
                if (!ReformState._teamCache.Contains(member.Identity))
                    ReformState._teamCache.Add(member.Identity);
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
                    && c.IsLeader)
                .FirstOrDefault()?.Character;

            if (AXPBuddy._leader != null)
            {
                if (AXPBuddy._died)
                    AXPBuddy._died = false;

                AXPBuddy._leaderPos = (Vector3)AXPBuddy._leader?.Position;

                if (DynelManager.LocalPlayer.Position.Distance2DFrom(AXPBuddy._leaderPos) > 2f 
                    && DynelManager.LocalPlayer.Position.Distance2DFrom(Constants.S13GoalPos) > 30f)
                    AXPBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(AXPBuddy._leaderPos.X, 67.7f, AXPBuddy._leaderPos.Z));

            }
        }
    }
}