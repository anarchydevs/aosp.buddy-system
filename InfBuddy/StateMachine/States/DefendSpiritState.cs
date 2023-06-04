﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace InfBuddy
{
    public class DefendSpiritState : PositionHolder, IState
    {
        private SimpleChar _target;
        private SimpleChar _charmMob;
        private SimpleChar _primevalSpinetooth;
        private static Corpse _corpse;

        public static Vector3 _corpsePos = Vector3.Zero;

        private static bool _charmMobAttacked = false;
        public static bool _missionsLoaded = false;

        private static double _charmMobAttacking;

        private List<Identity> _charmMobs = new List<Identity>();

        private double _mobStuckStartTime;
        public const double MobStuckTimeout = 600f;

        public DefendSpiritState() : base(Constants.DefendPos, 3f, 1)
        { }

        public IState GetNextState()
        {
            _corpse = DynelManager.Corpses
                .Where(c => c.Name.Contains("Remains of "))
                .FirstOrDefault();

            if (Game.IsZoning) { return null; }

            _primevalSpinetooth = DynelManager.NPCs
            .Where(c => c.Health > 0
                && c.Name.Contains("Primeval Spinetooth")
                && !c.Name.Contains("Remains of "))
            .FirstOrDefault();

            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId)
            {
                if (_target != null)
                    return new FightState(_target);

                if (_primevalSpinetooth != null)
                    return new FightState(_primevalSpinetooth);

                if (Extensions.IsNull(_target)
                    && Time.NormalTime > _mobStuckStartTime + MobStuckTimeout)
                {
                    foreach (Mission mission in Mission.List)
                        if (mission.DisplayName.Contains("The Purification"))
                            mission.Delete();

                    return new ExitMissionState();
                }

                if (Extensions.IsClear() || Extensions.CanExit(_missionsLoaded))
                    return new ExitMissionState();

                if (InfBuddy._settings["Looting"].AsBool()
                    && _corpse != null)
                    return new LootingState();
            }

            if (Playfield.ModelIdentity.Instance != Constants.NewInfMissionId)
                return new IdleState();

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return new SitState();

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("DefendSpiritState::OnStateEnter");
            _mobStuckStartTime = Time.NormalTime;
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("DefendSpiritState::OnStateExit");
            _missionsLoaded = false;
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
                    //Chat.WriteLine($"Found target: {_target.Name}.");
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

        private void HandleScan()
        {
            SimpleChar mob = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.Position.DistanceFrom(Constants.DefendPos) <= 20f
                    && !c.Name.Contains("Guardian Spirit of Purification"))
                .OrderBy(c => c.HealthPercent)
                .ThenBy(c => c.Position.DistanceFrom(Constants.DefendPos))
                .FirstOrDefault(c => !InfBuddy._namesToIgnore.Contains(c.Name) && !_charmMobs.Contains(c.Identity));

            if (mob != null)
            {
                _target = mob;
                //Chat.WriteLine($"Found target: {_target.Name}");

            }
            else if (DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65
                    && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted())
                HoldPosition();
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

            if (DynelManager.LocalPlayer.Profession == Profession.Trader || DynelManager.LocalPlayer.Profession == Profession.Bureaucrat)
                HandleCharmScan();

            HandleScan();


        }
    }
}
