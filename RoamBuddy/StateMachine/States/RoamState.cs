using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace RoamBuddy
{
    public class RoamState : IState
    {
        private SimpleChar _target;
        private SimpleChar _charmMob;

        public static bool FirstWaypoint = false;

        public static int _counter = 0;

        public static Vector3 _nextWaypoint;

        private static double _timeOut;

        private static bool _charmMobAttacked = false;
        private static double _charmMobAttacking;

        private List<Identity> _charmMobs = new List<Identity>();

        public IState GetNextState()
        {
            if (_target != null)
            {
                return new FightState(_target);
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("RoamState::OnStateEnter");

            if (!FirstWaypoint)
            {
                List<Vector3> _waypointList = new List<Vector3>(RoamBuddy._waypoints);

                List<Vector3> closestWaypoint = _waypointList
                    .OrderBy(m => DynelManager.LocalPlayer.Position.DistanceFrom(m))
                    .ToList();

                _counter = _waypointList.IndexOf(closestWaypoint.FirstOrDefault());
                MovementController.Instance.SetDestination(closestWaypoint.FirstOrDefault());

                FirstWaypoint = true;
            }

            _timeOut = Time.NormalTime;
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("RoamState::OnStateExit");
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
            if (DynelManager.LocalPlayer.Profession == Profession.Trader || DynelManager.LocalPlayer.Profession == Profession.Bureaucrat)
                HandleCharmScan();

            if (Extensions.GetLeader(RoamBuddy.Leader) != null)
            {
                List<Vector3> _waypointList = new List<Vector3>(RoamBuddy._waypoints);

                Spell spell = Spell.List.FirstOrDefault(c => c.IsReady);

                _nextWaypoint = _waypointList
                    .Where(c => _counter <= _waypointList.Count && c == _waypointList.ElementAt(_counter))
                    .FirstOrDefault();

                if (Extensions.Rooted()) { return; }

                if (!Extensions.InCombat()
                    && (DynelManager.LocalPlayer.HealthPercent < 66 || DynelManager.LocalPlayer.NanoPercent < 66)) { return; }

                if (RoamBuddy._mob.Count >= 1)
                {
                    if (RoamBuddy._mob.FirstOrDefault().Health == 0) { return; }

                    MovementController.Instance.Halt();
                    _target = RoamBuddy._mob.FirstOrDefault();

                    Chat.WriteLine($"Found target: {_target.Name}.");
                }
                else if (RoamBuddy._bossMob.Count >= 1)
                {
                    if (RoamBuddy._bossMob.FirstOrDefault().Health == 0) { return; }

                    MovementController.Instance.Halt();
                    _target = RoamBuddy._bossMob.FirstOrDefault();

                    Chat.WriteLine($"Found target: {_target.Name}.");
                }


                if (Time.NormalTime - _timeOut > 7 && MovementController.Instance.IsNavigating)
                {
                    _timeOut = Time.NormalTime;
                    MovementController.Instance.Halt();

                    if (_counter == 0)
                    {
                        _counter = _waypointList.Count();
                        return;
                    }
                    else
                    {
                        _counter--;
                        return;
                    }
                }

                if (!MovementController.Instance.IsNavigating && spell != null && !Spell.HasPendingCast
                    && Time.NormalTime - _timeOut <= 7
                    && RoamBuddy._mob.Count == 0 && RoamBuddy._bossMob.Count == 0)
                {
                    if (_counter < _waypointList.Count - 1)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(_waypointList.ElementAt(_counter)) < 1f)
                        {
                            _counter++;
                            return;
                        }

                        _timeOut = Time.NormalTime;
                        MovementController.Instance.SetDestination(_nextWaypoint);
                    }
                    else
                    {
                        FightState._ignoreTargetIdentity.Clear();

                        _counter = 0;

                        _timeOut = Time.NormalTime;
                        MovementController.Instance.SetDestination(_nextWaypoint);
                    }
                }
            }
        }
    }
}
