using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ALBBuddy
{
    public class LeechState : IState
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

            if (!ALBBuddy._died && Playfield.ModelIdentity.Instance == Constants.S13Id)
                ALBBuddy._ourPos = DynelManager.LocalPlayer.Position;

            if (!ALBBuddy._initMerge && ALBBuddy._settings["Merge"].AsBool())
            {
                if (!ALBBuddy._initMerge)
                    ALBBuddy._initMerge = true;

                ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos);
            }

            if (ALBBuddy._died)
            {
                if (ALBBuddy._ourPos != Vector3.Zero)
                {
                    if (ALBBuddy.NavMeshMovementController.IsNavigating && Time.NormalTime > _stuck + 30f)
                    {
                        DynelManager.LocalPlayer.Position = new Vector3(DynelManager.LocalPlayer.Position.X, DynelManager.LocalPlayer.Position.Y, DynelManager.LocalPlayer.Position.Z + 4f);
                        _stuck = Time.NormalTime;
                    }

                    if (!ALBBuddy.NavMeshMovementController.IsNavigating && DynelManager.LocalPlayer.Position.DistanceFrom(ALBBuddy._ourPos) > 15f)
                    {
                        ALBBuddy.NavMeshMovementController.SetNavMeshDestination(ALBBuddy._ourPos);
                        _stuck = Time.NormalTime;
                    }

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(ALBBuddy._ourPos) < 15f)
                        if (ALBBuddy._died)
                            ALBBuddy._died = false;
                }
            }

            ALBBuddy._leader = Team.Members
                .Where(c => c.Character?.Health > 0
                    && c.Character?.IsValid == true
                    && c.IsLeader)
                .FirstOrDefault()?.Character;

            if (ALBBuddy._leader != null)
            {
                if (ALBBuddy._died)
                    ALBBuddy._died = false;

                ALBBuddy._leaderPos = (Vector3)ALBBuddy._leader?.Position;

                //Reason: Edge correction
                if (ALBBuddy.NavMeshMovementController.IsNavigating)
                {
                    if (DynelManager.LocalPlayer.Position.Distance2DFrom(new Vector3(169.5f, 36.0f, 164.3f)) > 15f)
                        _init = false;

                    if (Time.NormalTime > _timeOut + 10f && _init)
                    {
                        ALBBuddy.NavMeshMovementController.Halt();
                        DynelManager.LocalPlayer.Position = new Vector3(ALBBuddy._leaderPos.X, 67.7f, ALBBuddy._leaderPos.Z);
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

                if (DynelManager.LocalPlayer.Position.Distance2DFrom(ALBBuddy._leaderPos) > 2f
                    && DynelManager.LocalPlayer.Position.Distance2DFrom(Constants.S13GoalPos) > 30f)
                    ALBBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(ALBBuddy._leaderPos.X, 67.7f, ALBBuddy._leaderPos.Z));

            }
        }
    }
}