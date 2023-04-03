using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ALBBuddy
{
    public class PatrolState : IState
    {
        private SimpleChar _target;

        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13GoalPos) <= 10f
                && Team.IsInTeam && !ALBBuddy.NavMeshMovementController.IsNavigating)
                Team.Disband();

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
        }

        private void HandleScan()
        {
            SimpleChar mob = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.IsInLineOfSight
                    && c.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 28f)
                .OrderBy(c => c.HealthPercent)
                .ThenBy(c => c.MaxHealth)
                .ThenBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .FirstOrDefault(c => !Constants._ignores.Contains(c.Name));

            if (mob != null)
            {
                _target = mob;
                Chat.WriteLine($"Found target: {_target.Name}");
            }
            else if (!Team.Members.Any(c => c.Character == null)
                    && !Team.Members.Where(c => c.Character != null
                       && (c.Character.HealthPercent < 66 || c.Character.NanoPercent < 66
                            || c.Character.Position.Distance2DFrom(DynelManager.LocalPlayer.Position) > 2f))
                       .Any()
                    && Spell.List.Any(c => c.IsReady)
                    && !Spell.HasPendingCast
                    && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted()
                    && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13GoalPos) > 5f)
            {

                if (!ALBBuddy._passedFirstCorrectionPos && !ALBBuddy._passedSecondCorrectionPos)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13FirstCorrectionPos) < 5f)
                        ALBBuddy._passedFirstCorrectionPos = true;
                    else
                        ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13FirstCorrectionPos);
                }
                else if (ALBBuddy._passedFirstCorrectionPos && !ALBBuddy._passedSecondCorrectionPos)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13SecondCorrectionPos) < 5f)
                        ALBBuddy._passedSecondCorrectionPos = true;
                    else
                        ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13SecondCorrectionPos);
                }
                else
                    ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos);
            }
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
                    if (!ALBBuddy.NavMeshMovementController.IsNavigating && DynelManager.LocalPlayer.Position.DistanceFrom(ALBBuddy._ourPos) > 15f)
                        ALBBuddy.NavMeshMovementController.SetNavMeshDestination(ALBBuddy._ourPos);

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(ALBBuddy._ourPos) < 15f)
                        if (ALBBuddy._died)
                            ALBBuddy._died = false;
                }
            }

            if (DynelManager.LocalPlayer.Identity != ALBBuddy.Leader)
            {
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

                    if (ALBBuddy._leader?.FightingTarget != null)
                    {
                        SimpleChar targetMob = DynelManager.NPCs
                            .Where(c => c.Health > 0
                                && c.Identity == (Identity)ALBBuddy._leader?.FightingTarget?.Identity)
                            .FirstOrDefault(c => !Constants._ignores.Contains(c.Name));

                        if (DynelManager.LocalPlayer.Position.DistanceFrom(ALBBuddy._leaderPos) > 2f
                            && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted())
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(ALBBuddy._leaderPos);

                        if (targetMob != null)
                        {
                            _target = targetMob;
                            Chat.WriteLine($"Found target: {_target.Name}");
                        }
                    }
                    else 
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(ALBBuddy._leaderPos) > 2f
                        && Spell.List.Any(c => c.IsReady)
                        && !Spell.HasPendingCast
                        && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted())
                        ALBBuddy.NavMeshMovementController.SetNavMeshDestination(ALBBuddy._leaderPos);
                }
                else
                    HandleScan();
            }
            else
                HandleScan();
        }
    }
}