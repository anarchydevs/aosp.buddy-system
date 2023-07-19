using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AXPBuddy
{
    public class PatrolState : IState
    {
        private SimpleChar _target;

        private static bool _init = false;
        private static double _timer;

        public IState GetNextState()
        {
            if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.APFHubId && !Team.IsInTeam)
                return new ReformState();

            if (_target != null)
                return new FightState(_target);

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("PatrolState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("PatrolState::OnStateExit");

            _init = false;
        }

        private void HandleScan()
        {
            SimpleChar mob = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && !Constants._ignores.Contains(c.Name)
                    && c.IsInLineOfSight
                    && c.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 35f)
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .ThenBy(c => c.HealthPercent)
                .FirstOrDefault();

            if (mob != null)
            {
                if (!Team.Members.Any(c => c.Character != null && (c.Character.HealthPercent < 66 || c.Character.NanoPercent < 66))
                    || Extensions.InCombat())
                {
                    _target = mob;
                    Chat.WriteLine($"Found target: {_target.Name}");
                }
            }
            else if (DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted()
                    && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13GoalPos) > 5f)
            {
                if (Team.Members.Any(c => c.Character != null && (c.Character.HealthPercent < 66 || c.Character.NanoPercent < 66))
                    || Team.Members.Any(c => c.Character == null)) { return; }

                if (!AXPBuddy._passedFirstCorrectionPos && !AXPBuddy._passedSecondCorrectionPos)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13FirstCorrectionPos) < 5f)
                        AXPBuddy._passedFirstCorrectionPos = true;
                    else
                        AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13FirstCorrectionPos);
                }
                else if (AXPBuddy._passedFirstCorrectionPos && !AXPBuddy._passedSecondCorrectionPos)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13SecondCorrectionPos) < 5f)
                        AXPBuddy._passedSecondCorrectionPos = true;
                    else
                        AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13SecondCorrectionPos);
                }
                else
                    AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos);
            }
        }

        public void Tick()
        {
            if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f) { return; }

            if (Team.IsInTeam && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13GoalPos) <= 10f
                && !DynelManager.NPCs.Any(c => c.Health > 0 && c.DistanceFrom(DynelManager.LocalPlayer) < 30f)
                && !_init)
            {
                foreach (TeamMember member in Team.Members)
                {
                    if (!ReformState._teamCache.Contains(member.Identity))
                        ReformState._teamCache.Add(member.Identity);
                }

                _init = true;
                _timer = Time.NormalTime;
            }

            if (Team.IsInTeam && _init && Time.NormalTime > _timer + 2f
                && !DynelManager.NPCs.Any(c => c.Health > 0 && c.DistanceFrom(DynelManager.LocalPlayer) < 30f)
                && DynelManager.LocalPlayer.Identity == AXPBuddy.Leader)
            {
                Team.Disband();
            }

            if (!AXPBuddy._died && Playfield.ModelIdentity.Instance == Constants.S13Id)
                AXPBuddy._ourPos = DynelManager.LocalPlayer.Position;

            if (!AXPBuddy._initMerge && AXPBuddy._settings["Merge"].AsBool())
            {
                if (!AXPBuddy._initMerge)
                    AXPBuddy._initMerge = true;

                AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos, out NavMeshPath path);
            }

            if (AXPBuddy._died)
            {
                if (AXPBuddy._ourPos != Vector3.Zero)
                {
                    if (!AXPBuddy.NavMeshMovementController.IsNavigating && DynelManager.LocalPlayer.Position.DistanceFrom(AXPBuddy._ourPos) > 15f)
                        AXPBuddy.NavMeshMovementController.SetNavMeshDestination(AXPBuddy._ourPos);

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(AXPBuddy._ourPos) < 15f)
                        if (AXPBuddy._died)
                            AXPBuddy._died = false;
                }
            }

            if (DynelManager.LocalPlayer.Identity != AXPBuddy.Leader)
            {
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

                    if (AXPBuddy._leader?.FightingTarget != null || AXPBuddy._leader?.IsAttacking == true)
                    {
                        SimpleChar targetMob = DynelManager.NPCs
                            .Where(c => c.Health > 0
                                && c.Identity == (Identity)AXPBuddy._leader?.FightingTarget?.Identity)
                            .FirstOrDefault(c => !Constants._ignores.Contains(c.Name));

                        if (targetMob != null)
                        {
                            _target = targetMob;
                            Chat.WriteLine($"Found target: {_target.Name}");
                        }
                    }

                    if (DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted()
                        && DynelManager.LocalPlayer.Position.DistanceFrom(AXPBuddy._leaderPos) > 1.2f)
                    {
                        AXPBuddy.NavMeshMovementController.SetNavMeshDestination(AXPBuddy._leaderPos);
                    }
                }
                else
                    HandleScan();
            }
            else
                HandleScan();
        }
    }
}