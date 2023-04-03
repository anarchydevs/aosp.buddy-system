﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MitaarBuddy
{
    //I like this template over the RoamState in InfBuddy (Same concept)
    public class MediumState : IState
    {
        private SimpleChar _target;

        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13GoalPos) <= 10f
                && Team.IsInTeam && !MitaarBuddy.NavMeshMovementController.IsNavigating)
                Team.Disband();

            if (Playfield.ModelIdentity.Instance == Constants.APFHubId && !Team.IsInTeam)
                return new ReformState();

            if (_target != null)
                return new FightState(_target);

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("RoamState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("RoamState::OnStateExit");
        }

        private void HandleScan()
        {
            SimpleChar mob = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.IsInLineOfSight
                    && c.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 88f)
                .OrderBy(c => c.HealthPercent)
                .ThenBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .FirstOrDefault(c => !Constants._ignores.Contains(c.Name));

            if (mob != null)
            {
                _target = mob;
                Chat.WriteLine($"Found target: {_target.Name}");
            }
            else if (!Team.Members.Any(c => c.Character == null)
                    && !Team.Members.Where(c => c.Character != null
                       && (c.Character.HealthPercent < 66 || c.Character.NanoPercent < 66))
                       .Any()
                    && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted()
                    && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13GoalPos) > 5f)
            {
                if (!MitaarBuddy._passedFirstCorrectionPos && !MitaarBuddy._passedSecondCorrectionPos)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13FirstCorrectionPos) < 5f)
                        MitaarBuddy._passedFirstCorrectionPos = true;
                    else
                        MitaarBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13FirstCorrectionPos);
                }
                else if (MitaarBuddy._passedFirstCorrectionPos && !MitaarBuddy._passedSecondCorrectionPos)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13SecondCorrectionPos) < 5f)
                        MitaarBuddy._passedSecondCorrectionPos = true;
                    else
                        MitaarBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13SecondCorrectionPos);
                }
                else
                    MitaarBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos);
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
                    if (!MitaarBuddy.NavMeshMovementController.IsNavigating && DynelManager.LocalPlayer.Position.DistanceFrom(MitaarBuddy._ourPos) > 15f)
                        MitaarBuddy.NavMeshMovementController.SetNavMeshDestination(MitaarBuddy._ourPos);

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(MitaarBuddy._ourPos) < 15f)
                        if (MitaarBuddy._died)
                            MitaarBuddy._died = false;
                }
            }

            if (DynelManager.LocalPlayer.Identity != MitaarBuddy.Leader)
            {
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

                    if (MitaarBuddy._leader?.FightingTarget != null)
                    {
                        SimpleChar targetMob = DynelManager.NPCs
                            .Where(c => c.Health > 0
                                && c.Identity == (Identity)MitaarBuddy._leader?.FightingTarget?.Identity)
                            .FirstOrDefault(c => !Constants._ignores.Contains(c.Name));

                        if (DynelManager.LocalPlayer.Position.DistanceFrom(MitaarBuddy._leaderPos) > 1.2f
                            && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted())
                            MitaarBuddy.NavMeshMovementController.SetNavMeshDestination(MitaarBuddy._leaderPos);

                        if (targetMob != null)
                        {
                            _target = targetMob;
                            Chat.WriteLine($"Found target: {_target.Name}");
                        }
                    }
                    else
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(MitaarBuddy._leaderPos) > 1.2f
                        && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted())
                        MitaarBuddy.NavMeshMovementController.SetNavMeshDestination(MitaarBuddy._leaderPos);
                }
                else
                    HandleScan();
            }
            else
                HandleScan();
        }
    }
}