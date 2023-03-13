using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InfBuddy
{
    public class DefendSpiritState : PositionHolder, IState
    {
        private SimpleChar _target;
        private SimpleChar _charmMob;

        private static bool _charmMobAttacked = false;
        private static bool _missionsLoaded = false;

        private static double _charmMobAttacking;

        private List<Identity> _charmMobs = new List<Identity>();

        private double _mobStuckStartTime;
        public const double MobStuckTimeout = 1500f;

        public DefendSpiritState() : base(Constants.DefendPos, 3f, 1)
        {

        }

        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (Extensions.CanExit(_missionsLoaded))
                return new ExitMissionState();

            if (_target != null)
                return new FightState(_target);

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("DefendSpiritState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("DefendSpiritState::OnStateExit");
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

        private void HandleScan()
        {
            SimpleChar mob = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.Position.DistanceFrom(Constants.DefendPos) <= 30f)
                .OrderBy(c => c.HealthPercent)
                .ThenBy(c => c.Position.DistanceFrom(Constants.DefendPos))
                .FirstOrDefault(c => !InfBuddy._namesToIgnore.Contains(c.Name) && !_charmMobs.Contains(c.Identity));

            if (mob != null)
            {
                _target = mob;
                Chat.WriteLine($"Found target: {_target.Name}");
            }
            else if (DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65
                    && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted())
                HoldPosition();
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (!_missionsLoaded && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                _missionsLoaded = true;

            if (DynelManager.LocalPlayer.Profession == Profession.Trader || DynelManager.LocalPlayer.Profession == Profession.Bureaucrat)
                HandleCharmScan();

            HandleScan();
        }
    }
}
