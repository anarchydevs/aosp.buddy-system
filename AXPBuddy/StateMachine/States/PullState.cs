using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System;
using System.Linq;

namespace AXPBuddy
{
    public class PullState : IState
    {
        private static bool _init = false;
        private static double _timer;

        public static int _aggToolCounter = 0;
        public static int _attackTimeout = 0;

        public IState GetNextState()
        {
            if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f)
                return null;

            if (Playfield.ModelIdentity.Instance == Constants.UnicornHubId)
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.APFHubId)
            {
                if (!Team.IsInTeam)
                    return new ReformState();
                else
                    return new IdleState();
            }

            if (AXPBuddy.ModeSelection.Path == (AXPBuddy.ModeSelection)AXPBuddy._settings["ModeSelection"].AsInt32())
            {
                return new PathState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Pull state");
        }

        public void Tick()
        {
            try
            {
                if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f)
                    return;

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
                            && (c.Identity == AXPBuddy.Leader || c.IsLeader))
                        .FirstOrDefault()?.Character;

                    if (AXPBuddy._leader != null)
                    {
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
                                   && !DynelManager.LocalPlayer.IsAttacking
                                   && !DynelManager.LocalPlayer.IsAttackPending
                                   && targetMob.IsInLineOfSight)
                                {
                                    DynelManager.LocalPlayer.Attack(targetMob);
                                }
                            }
                        }

                        if (DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted()
                            && DynelManager.LocalPlayer.Position.DistanceFrom(AXPBuddy._leaderPos) > 2.4f)
                        {
                            AXPBuddy.NavMeshMovementController.SetNavMeshDestination(AXPBuddy._leaderPos);
                        }
                    }
                }
                else
                {
                    SimpleChar mob = DynelManager.NPCs
                        .Where(c => c.Health > 0 && !c.Name.Contains("Unicorn Recon Agent Chittick"))
                        .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                        .FirstOrDefault();

                    if (Team.Members.Any(c => c.Character != null))
                    {
                        if (mob != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13GoalPos) > 10f)
                        {
                            if (!mob.IsInLineOfSight)
                            {
                                AXPBuddy.NavMeshMovementController.SetNavMeshDestination(mob.Position);
                            }

                            else if (mob.IsInLineOfSight)
                            {
                                AXPBuddy.NavMeshMovementController.Halt();

                                if (mob.IsInAttackRange())
                                {
                                    if (DynelManager.LocalPlayer.FightingTarget == null
                                   && !DynelManager.LocalPlayer.IsAttacking
                                   && !DynelManager.LocalPlayer.IsAttackPending
                                   && mob.IsInLineOfSight)
                                    {
                                        DynelManager.LocalPlayer.Attack(mob);
                                    }
                                }

                                else if (!mob.IsInAttackRange())
                                {
                                    HandleTaunting(mob);
                                }

                                else if (mob.IsAttacking && !mob.IsInAttackRange() && !Spell.HasPendingCast && DynelManager.LocalPlayer.NanoPercent > 70
                       && DynelManager.LocalPlayer.HealthPercent > 70 && Spell.List.Any(spell => spell.IsReady) && AXPBuddy.Ready)
                                {
                                    AXPBuddy.NavMeshMovementController.SetNavMeshDestination(mob.Position);
                                }
                            }
                        }
                        else if (DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted()
                                && Team.Members.Any(c => c.Character != null)
                                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.S13GoalPos) > 5f 
                                && !Spell.HasPendingCast && DynelManager.LocalPlayer.NanoPercent > 70
                       && DynelManager.LocalPlayer.HealthPercent > 70 && Spell.List.Any(spell => spell.IsReady) && AXPBuddy.Ready)
                        {
                            AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + AXPBuddy.GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != AXPBuddy.previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    AXPBuddy.previousErrorMessage = errorMessage;
                }
            }
        }

        public static void HandleTaunting(SimpleChar target)
        {
            if (_aggToolCounter >= 2)
            {
                if (_attackTimeout >= 1)
                {
                    AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13GoalPos);
                    _attackTimeout = 0;
                    _aggToolCounter = 0;
                    return;
                }

                _attackTimeout++;
                _aggToolCounter = 0;
            }

            Item item = null;

            if (Inventory.Find(83920, out item)  ||  // Aggression Enhancer
                Inventory.Find(83919, out item)  ||  // Aggression Multiplier
                Inventory.Find(151692, out item) ||  // Modified Aggression Enhancer (low)
                Inventory.Find(151693, out item) ||  // Modified Aggression Enhancer (High)
                Inventory.Find(152029, out item) || // Aggression Enhancer (Jealousy Augmented)
                Inventory.Find(152028, out item) || // Aggression Multiplier (Jealousy Augmented)
                Inventory.Find(244655, out item) || // Scorpio's Aim of Anger
                Inventory.Find(253186, out item) || // Codex of the Insulting Emerto (Low)
                Inventory.Find(253187, out item))   // Codex of the Insulting Emerto (High)
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    item.Use(target, true);
                    _aggToolCounter++;
                }
            }
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("PullState::OnStateExit");

            _init = false;
        }
    }
}
