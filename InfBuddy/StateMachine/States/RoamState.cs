using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace InfBuddy
{
    public class RoamState : PositionHolder, IState
    {
        private static Corpse _corpse;

        public RoamState() : base(Constants.DefendPos, 2f, 1)
        {

        }

        public IState GetNextState()
        {
            _corpse = DynelManager.Corpses
                .Where(c => c.Name.Contains("Remains of "))
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .FirstOrDefault();

            if (Game.IsZoning)
                return null;

            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId && !Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                return new ExitMissionState();

            if (Playfield.ModelIdentity.Instance != Constants.NewInfMissionId)
                return new IdleState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Roaming");
        }

        public void Tick()
        {
            if (Game.IsZoning) 
                return;

            if (Team.IsInTeam)
            {
                foreach (TeamMember member in Team.Members)
                {
                    if (!ReformState._teamCache.Contains(member.Identity))
                        ReformState._teamCache.Add(member.Identity);
                }
            }

            if (DynelManager.LocalPlayer.Identity != InfBuddy.Leader)
            {
                InfBuddy._leader = Team.Members
                        .Where(c => c.Character?.Health > 0
                            && c.Character?.IsValid == true
                            && c.Identity == InfBuddy.Leader)
                        .FirstOrDefault()?.Character;

                if (InfBuddy._leader != null)
                {
                    if (InfBuddy._leader?.FightingTarget != null)
                    {
                        SimpleChar targetMob = DynelManager.NPCs
                            .Where(c => c.Health > 0
                                && c.Identity == (Identity)InfBuddy._leader?.FightingTarget?.Identity)
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
                    else if (!Team.Members.Where(c => c.Character != null && (c.Character.HealthPercent < 66 || c.Character.NanoPercent < 66)).Any()
                        && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted()
                        && DynelManager.LocalPlayer.Position.DistanceFrom((Vector3)InfBuddy._leader?.Position) > 1.2f)
                    {
                        InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)InfBuddy._leader?.Position);
                    }
                }
            }
            else // is leader
            {
                SimpleChar mob = DynelManager.NPCs
                    .Where(c => c.Health > 0)
                    .OrderBy(c => c.HealthPercent)
                    .ThenBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                    .FirstOrDefault(c => !InfBuddy._namesToIgnore.Contains(c.Name));

                if (mob != null && mob.Position.DistanceFrom(DynelManager.LocalPlayer.Position) < 8
                            && mob.IsInLineOfSight)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null
                       && !DynelManager.LocalPlayer.IsAttacking
                       && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(mob);
                    }
                }
                else if (Team.Members.Any(c => c.Character != null) && !Spell.HasPendingCast && DynelManager.LocalPlayer.NanoPercent > 70
                && DynelManager.LocalPlayer.HealthPercent > 70 && Spell.List.Any(spell => spell.IsReady) && InfBuddy.Ready)
                {
                    if (mob != null)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(mob.Position) > 5 || !mob.IsInLineOfSight)
                        {
                            InfBuddy.NavMeshMovementController.SetNavMeshDestination(mob.Position);
                        }
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(mob.Position) < 2 && mob.IsInLineOfSight)
                        {
                            InfBuddy.NavMeshMovementController.Halt();
                        }
                    }
                    else if (mob == null && _corpse != null && InfBuddy._settings["Looting"].AsBool())
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpse.Position) > 5f)
                            InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)_corpse?.Position);
                    }
                    else if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.RoamPos) > 5)
                    {
                        InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.RoamPos);
                    }
                }
            }
        }
        public void OnStateExit()
        {
            //Chat.WriteLine("RoamState::OnStateExit");
        }
    }
}
