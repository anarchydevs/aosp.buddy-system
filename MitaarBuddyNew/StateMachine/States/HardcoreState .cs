using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MitaarBuddy
{
    public class HardcoreState : IState
    {
        private static double _stuck;
        private static double _timeOut;

        private static bool _init = false;

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

            if (!MitaarBuddy._died && Playfield.ModelIdentity.Instance == Constants.S13Id)
                MitaarBuddy._ourPos = DynelManager.LocalPlayer.Position;

            if (!MitaarBuddy._initMerge && MitaarBuddy._settings["Merge"].AsBool())
            {
                if (!MitaarBuddy._initMerge)
                    MitaarBuddy._initMerge = true;

                MitaarBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos);
            }

            if (MitaarBuddy._died)
            {
                if (MitaarBuddy._ourPos != Vector3.Zero)
                {
                    if (MitaarBuddy.NavMeshMovementController.IsNavigating && Time.NormalTime > _stuck + 30f)
                    {
                        DynelManager.LocalPlayer.Position = new Vector3(DynelManager.LocalPlayer.Position.X, DynelManager.LocalPlayer.Position.Y, DynelManager.LocalPlayer.Position.Z + 4f);
                        _stuck = Time.NormalTime;
                    }

                    if (!MitaarBuddy.NavMeshMovementController.IsNavigating && DynelManager.LocalPlayer.Position.DistanceFrom(MitaarBuddy._ourPos) > 15f)
                    {
                        MitaarBuddy.NavMeshMovementController.SetNavMeshDestination(MitaarBuddy._ourPos);
                        _stuck = Time.NormalTime;
                    }

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(MitaarBuddy._ourPos) < 15f)
                        if (MitaarBuddy._died)
                            MitaarBuddy._died = false;
                }
            }

            MitaarBuddy._leader = Team.Members
                .Where(c => c.Character?.Health > 0
                    && c.Character?.IsValid == true
                    && c.IsLeader)
                .FirstOrDefault()?.Character;

            if (MitaarBuddy._leader != null)
            {
                if (MitaarBuddy._died)
                    MitaarBuddy._died = false;

                MitaarBuddy._leaderPos = (Vector3)MitaarBuddy._leader?.Position;

                //Reason: Edge correction
                if (MitaarBuddy.NavMeshMovementController.IsNavigating)
                {
                    if (DynelManager.LocalPlayer.Position.Distance2DFrom(new Vector3(169.5f, 36.0f, 164.3f)) > 15f)
                        _init = false;

                    if (Time.NormalTime > _timeOut + 10f && _init)
                    {
                        MitaarBuddy.NavMeshMovementController.Halt();
                        DynelManager.LocalPlayer.Position = new Vector3(MitaarBuddy._leaderPos.X, 67.7f, MitaarBuddy._leaderPos.Z);
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

                if (DynelManager.LocalPlayer.Position.Distance2DFrom(MitaarBuddy._leaderPos) > 2f
                    && DynelManager.LocalPlayer.Position.Distance2DFrom(Constants.S13GoalPos) > 30f)
                    MitaarBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(MitaarBuddy._leaderPos.X, 67.7f, MitaarBuddy._leaderPos.Z));

            }
        }
    }
}