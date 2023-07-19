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
    public class RoamState : IState
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

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("RoamState::OnStateEnter");
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

            if (!AXPBuddy._initMerge && AXPBuddy._settings["Merge"].AsBool())
            {
                if (!AXPBuddy._initMerge)
                    AXPBuddy._initMerge = true;

                AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos);
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
                            .FirstOrDefault();

                        if (targetMob != null)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget == null
                                && !DynelManager.LocalPlayer.IsAttacking && !DynelManager.LocalPlayer.IsAttackPending)
                            {  
                                DynelManager.LocalPlayer.Attack(targetMob);
                            }
                        }
                    }

                    if (DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted()
                        && DynelManager.LocalPlayer.Position.DistanceFrom(AXPBuddy._leaderPos) > 1.2f)
                    {
                        AXPBuddy.NavMeshMovementController.SetNavMeshDestination(AXPBuddy._leaderPos);
                    }
                }
               
                if (AXPBuddy._died)
                {
                    if (AXPBuddy._leader == null)
                    {
                        AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos);
                    }

                    if (AXPBuddy._leader != null && DynelManager.LocalPlayer.Position.DistanceFrom(AXPBuddy._leader.Position) < 15f)
                    {
                        if (AXPBuddy._died)
                            AXPBuddy._died = false;
                    }
                }
            }
            else
                HandleScan();
        }

        public void OnStateExit()
        {
            Chat.WriteLine("RoamState::OnStateExit");

            _init = false;
        }

        private void HandleScan()
        {
            SimpleChar mob = DynelManager.NPCs
                .Where(c => c.Health > 0)
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .FirstOrDefault();

            if (Team.Members.Any(c => c.Character != null))
            {
                if (mob != null)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(mob.Position) > 5f)
                    {
                        AXPBuddy.NavMeshMovementController.SetNavMeshDestination(mob.Position);
                    }
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(mob.Position) < 7f && mob.IsInLineOfSight)
                    {
                        if (DynelManager.LocalPlayer.FightingTarget == null
                                && !DynelManager.LocalPlayer.IsAttacking && !DynelManager.LocalPlayer.IsAttackPending)
                        {
                            DynelManager.LocalPlayer.Attack(mob);

                        }
                    }

                }
                else if (DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted()
                        && Team.Members.Any(c => c.Character != null)
                        && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13GoalPos) > 5f)
                {
                    AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos);
                }
            }
        }
    }
}