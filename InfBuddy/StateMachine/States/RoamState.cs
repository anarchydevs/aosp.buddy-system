using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace InfBuddy
{
    public class RoamState : PositionHolder, IState
    {
        private SimpleChar _target;
        private SimpleChar _charmMob;
        private static Corpse _corpse;

        private static bool _charmMobAttacked = false;
        private static bool _missionsLoaded = false;

        private static double _charmMobAttacking;

        private List<Identity> _charmMobs = new List<Identity>();

        public RoamState() : base(Constants.DefendPos, 2f, 1)
        {

        }

        public IState GetNextState()
        {
            _corpse = DynelManager.Corpses
                .Where(c => c.Name.Contains("Remains of "))
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .FirstOrDefault();

            if (Game.IsZoning) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId)
            {
                if (Extensions.CanExit(_missionsLoaded) || Extensions.IsClear())
                    return new ExitMissionState();
            }

            if (Playfield.ModelIdentity.Instance != Constants.NewInfMissionId)
                return new IdleState();

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return new SitState();

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("RoamState::OnStateEnter");

            InfBuddy._stateTimeOut = Time.NormalTime;
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Team.IsInTeam)
            {
                foreach (TeamMember member in Team.Members)
                {
                    if (!ReformState._teamCache.Contains(member.Identity))
                        ReformState._teamCache.Add(member.Identity);
                }
            }

            if (!_missionsLoaded && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                _missionsLoaded = true;

            if (Time.NormalTime > InfBuddy._stateTimeOut + 210f)
            {
                InfBuddy._stateTimeOut = Time.NormalTime;
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(184.5f, 1.0f, 242.9f));
                InfBuddy.NavMeshMovementController.AppendNavMeshDestination(new Vector3(181.3f, 1.0f, 245.6f));
            }

            if (DynelManager.LocalPlayer.Identity != InfBuddy.Leader)
            {
                InfBuddy._leader = Team.Members
                        .Where(c => c.Character?.Health > 0
                            && c.Character?.IsValid == true
                            && (c.Identity == InfBuddy.Leader || c.IsLeader))
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
                            DynelManager.LocalPlayer.Attack(targetMob);
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
            else
            {
                SimpleChar mob = DynelManager.NPCs
                    .Where(c => c.Health > 0)
                    .OrderBy(c => c.HealthPercent)
                    .ThenBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                    .FirstOrDefault(c => !InfBuddy._namesToIgnore.Contains(c.Name));

                if (mob != null)
                {
                    if (!mob.IsInAttackRange() || !mob.IsInLineOfSight)
                    {
                        InfBuddy.NavMeshMovementController.SetNavMeshDestination(mob.Position);
                    }
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(mob.Position) < 10 && mob.IsInLineOfSight)
                    {
                        InfBuddy.NavMeshMovementController.Halt();

                        if (DynelManager.LocalPlayer.FightingTarget == null
                                && !DynelManager.LocalPlayer.IsAttacking && !DynelManager.LocalPlayer.IsAttackPending)
                        {
                            DynelManager.LocalPlayer.Attack(mob);
                        }
                    }
                }
                else if (mob == null && _corpse != null && InfBuddy._settings["Looting"].AsBool())
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpse.Position) > 5f)
                        InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)_corpse?.Position);
                }
                else if (!Team.Members.Where(c => c.Character != null && (c.Character.HealthPercent < 66 || c.Character.NanoPercent < 66)).Any()
                && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted())
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.RoamPos) > 5)
                    {
                        InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.RoamPos);
                    }
                }
            }
        }
        public void OnStateExit()
        {
            //Chat.WriteLine("RoamState::OnStateExit");

            _missionsLoaded = false;
        }

    }
}
