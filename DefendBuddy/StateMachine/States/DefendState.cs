using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace DefendBuddy
{
    public class DefendState : PositionHolder, IState
    {
        private SimpleChar _target;
        private SimpleChar _charmMob;

        private static bool _charmMobAttacked = false;

        private static double _charmMobAttacking;

        private List<Identity> _charmMobs = new List<Identity>();

        public DefendState() : base(Constants._posToDefend, 3f, 1)
        {

        }

        public IState GetNextState()
        {
            if (_target != null)
                return new FightState(_target);

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("DefendState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("DefendState::OnStateExit");
        }

        private bool Rooted()
        {
            if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Root)
                || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.AOERoot)) { return true; }

            return false;
        }

        public void Tick()
        {
            if (Extensions.GetLeader(DefendBuddy.Leader) != null)
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

                if (DefendBuddy._mob.Count >= 1)
                {
                    if (DefendBuddy._mob.FirstOrDefault().Health == 0) { return; }

                    _target = DefendBuddy._mob.FirstOrDefault();
                    Chat.WriteLine($"Found target: {_target.Name}.");
                }
                else if (DefendBuddy._bossMob.Count >= 1)
                {
                    if (DefendBuddy._bossMob.FirstOrDefault().Health == 0) { return; }

                    _target = DefendBuddy._bossMob.FirstOrDefault();
                    Chat.WriteLine($"Found target: {_target.Name}.");
                }

                if (DefendBuddy._mob.Count == 0 && DefendBuddy._bossMob.Count == 0 && DynelManager.LocalPlayer.HealthPercent >= 66
                    && DynelManager.LocalPlayer.NanoPercent >= 66
                    && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Rooted())
                    HoldPosition();
            }
        }
    }
}
