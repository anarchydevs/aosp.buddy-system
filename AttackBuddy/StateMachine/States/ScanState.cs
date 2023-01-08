using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace AttackBuddy
{
    public class ScanState : IState
    {
        private SimpleChar _target;
        private SimpleChar _charmMob;

        private static bool _charmMobAttacked = false;

        private static double _charmMobAttacking;

        private List<Identity> _charmMobs = new List<Identity>();

        public IState GetNextState()
        {
            if (_target != null || !AttackBuddy._settings["Toggle"].AsBool())
                return new FightState(_target);

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("ScanState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("ScanState::OnStateExit");
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
            if (Extensions.ShouldStopAttack())
            {
                DynelManager.LocalPlayer.StopAttack();
                return;
            }

            if (Extensions.GetLeader(AttackBuddy.Leader) != null)
            {
                if (DynelManager.LocalPlayer.Profession == Profession.Trader || DynelManager.LocalPlayer.Profession == Profession.Bureaucrat)
                    HandleCharmScan();

                if (AttackBuddy._mob.Count >= 1)
                {
                    if (AttackBuddy._mob.FirstOrDefault().Health == 0) { return; }

                    _target = AttackBuddy._mob.FirstOrDefault();
                    Chat.WriteLine($"Found target: {_target.Name}.");
                }
                else if (AttackBuddy._bossMob.Count >= 1)
                {
                    if (AttackBuddy._bossMob.FirstOrDefault().Health == 0) { return; }

                    _target = AttackBuddy._bossMob.FirstOrDefault();
                    Chat.WriteLine($"Found target: {_target.Name}.");
                }
                else if (AttackBuddy._switchMobPrecision.Count >= 1)
                {
                    if (AttackBuddy._switchMobPrecision.FirstOrDefault().Health == 0) { return; }

                    _target = AttackBuddy._switchMobPrecision.FirstOrDefault();
                    Chat.WriteLine($"Found target: {_target.Name}.");
                }
                else if (AttackBuddy._switchMobCharging.Count >= 1)
                {
                    if (AttackBuddy._switchMobCharging.FirstOrDefault().Health == 0) { return; }

                    _target = AttackBuddy._switchMobCharging.FirstOrDefault();
                    Chat.WriteLine($"Found target: {_target.Name}.");
                }
                else if (AttackBuddy._switchMobShield.Count >= 1)
                {
                    if (AttackBuddy._switchMobShield.FirstOrDefault().Health == 0) { return; }

                    _target = AttackBuddy._switchMobShield.FirstOrDefault();
                    Chat.WriteLine($"Found target: {_target.Name}.");
                }
            }
        }
    }
}
