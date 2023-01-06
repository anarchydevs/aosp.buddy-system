using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using InfBuddy.IPCMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfBuddy
{
    public class RoamState : PositionHolder, IState
    {
        private SimpleChar _target;
        private SimpleChar _charmMob;

        private static bool _charmMobAttacked = false;
        private static bool _missionsLoaded = false;

        private static double _charmMobAttacking;

        private List<Identity> _charmMobs = new List<Identity>();

        public RoamState() : base(Constants.DefendPos, 2f, 1)
        {

        }

        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.InfernoId)
            {
                InfBuddy._settings["Toggle"] = false;
                InfBuddy.Toggle = false;
                return new IdleState();
            }

            if (Extensions.CanExit(_missionsLoaded))
                return new ExitMissionState();

            if (_target != null)
                return new FightState(_target);

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("RoamState::OnStateEnter");

            InfBuddy._stateTimeOut = Time.NormalTime;
        }

        public void OnStateExit()
        {
            Chat.WriteLine("RoamState::OnStateExit");

            InfBuddy._currentTarget = Identity.None;
            _missionsLoaded = false;
        }

        private void HandleScan()
        {
            SimpleChar mob = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.IsInLineOfSight
                    && c.Position.DistanceFrom(Constants.DefendPos) <= 80f)
                .OrderBy(c => c.HealthPercent)
                .ThenBy(c => c.Position.DistanceFrom(Constants.DefendPos))
                .FirstOrDefault(c => !InfBuddy._namesToIgnore.Contains(c.Name) && !_charmMobs.Contains(c.Identity));

            if (mob != null && !InfBuddy._namesToIgnore.Contains(mob.Name) 
                && !Team.Members.Where(c => c.Character != null && (c.Character.HealthPercent < 66 || c.Character.NanoPercent < 66)).Any())
            {
                _target = mob;
                Chat.WriteLine($"Found target: {_target.Name}");
            }
            else if (!Team.Members.Where(c => c.Character != null && (c.Character.HealthPercent < 66 || c.Character.NanoPercent < 66)).Any()
                && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted())
                HoldPosition();
        }

        private void HandleCharmScan()
        {
            _charmMob = DynelManager.NPCs
                .Where(c => c.Buffs.Contains(NanoLine.CharmOther) || c.Buffs.Contains(NanoLine.Charm_Short))
                .FirstOrDefault();

            if (_charmMob != null)
            {
                if (!_charmMobs.Contains(_charmMob.Identity))
                    _charmMobs.Add(_charmMob.Identity);

                if (Time.NormalTime > _charmMobAttacking + 8
                    && _charmMobAttacked == true)
                {
                    _charmMobAttacked = false;
                    _charmMobs.Remove(_charmMob.Identity);
                    _target = _charmMob;
                    Chat.WriteLine($"Found target: {_target.Name}.");
                }

                if (_charmMob.FightingTarget != null && _charmMob.IsAttacking
                    && _charmMobs.Contains(_charmMob.Identity)
                    && Team.Members.Select(c => c.Identity).Any(x => _charmMob.FightingTarget.Identity == x)
                    && _charmMobAttacked == false)
                {
                    _charmMobAttacking = Time.NormalTime;
                    _charmMobAttacked = true;
                }
            }
        }

        public void Tick()
        {
            if (!_missionsLoaded && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                _missionsLoaded = true;

            if (Time.NormalTime > InfBuddy._stateTimeOut + 210f)
            {
                InfBuddy._stateTimeOut = Time.NormalTime;
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(184.5f, 1.0f, 242.9f));
                InfBuddy.NavMeshMovementController.AppendNavMeshDestination(new Vector3(181.3f, 1.0f, 245.6f));
            }

            if (DynelManager.LocalPlayer.Profession == Profession.Trader || DynelManager.LocalPlayer.Profession == Profession.Bureaucrat)
                HandleCharmScan();

            if (DynelManager.LocalPlayer.Identity != InfBuddy.Leader)
            {
                InfBuddy._leader = Team.Members
                    .Where(c => c.Character?.Health > 0
                        && c.Character?.IsValid == true
                        && c.IsLeader)
                    .FirstOrDefault()?.Character;

                if (InfBuddy._leader != null)
                {
                    if (InfBuddy._leader?.FightingTarget != null)
                    {
                        SimpleChar targetMob = DynelManager.NPCs
                            .Where(c => c.Health > 0
                                && c.Identity == (Identity)InfBuddy._leader?.FightingTarget?.Identity)
                            .FirstOrDefault(c => !InfBuddy._namesToIgnore.Contains(c.Name) && !_charmMobs.Contains(c.Identity));

                        if (targetMob != null && !InfBuddy._namesToIgnore.Contains(targetMob.Name))
                        {
                            _target = targetMob;
                            Chat.WriteLine($"Found target: {_target.Name}");
                        }
                    }
                    else if (DynelManager.NPCs.Any(c => c.FightingTarget != null && c.DistanceFrom(DynelManager.LocalPlayer) < 20f))
                    {
                        SimpleChar mob = DynelManager.NPCs
                            .Where(c => c.Health > 0
                                && c.IsInLineOfSight
                                && c.Position.DistanceFrom(Constants.DefendPos) <= 20f)
                            .OrderBy(c => c.HealthPercent)
                            .ThenBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                            .FirstOrDefault(c => !InfBuddy._namesToIgnore.Contains(c.Name) && !_charmMobs.Contains(c.Identity));

                        if (mob != null && !InfBuddy._namesToIgnore.Contains(mob.Name))
                        {
                            _target = mob;
                            Chat.WriteLine($"Found target: {_target.Name}");
                        }
                    }
                    else if (!Team.Members.Where(c => c.Character != null && (c.Character.HealthPercent < 66 || c.Character.NanoPercent < 66)).Any()
                        && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted()
                        && DynelManager.LocalPlayer.Position.DistanceFrom((Vector3)InfBuddy._leader?.Position) > 3f)
                        InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)InfBuddy._leader?.Position);
                }
                else
                    HandleScan();
            }
            else
                HandleScan();
        }
    }
}
