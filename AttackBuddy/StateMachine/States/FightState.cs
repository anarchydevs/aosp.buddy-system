using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AttackBuddy
{
    public class FightState : IState
    {
        public const double _fightTimeout = 45f;

        private double _fightStartTime;
        public static float _tetherDistance;

        public static List<int> _ignoreTargetIdentity = new List<int>();

        private SimpleChar _target;

        public FightState(SimpleChar target)
        {
            _target = target;
        }

        public IState GetNextState()
        {

            if (AttackBuddy._settings["Enable"].AsBool())
            {
                if (Extensions.IsNull(_target))
                {
                    _target = null;
                    return new ScanState();
                }
            }
            else
            {
                return new IdleState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("FightState::OnStateEnter");

            _fightStartTime = Time.NormalTime;
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("FightState::OnStateExit");

            if (DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.StopAttack();
        }

        public void Tick()
        {
            if (_target == null)
                return;

            if (Extensions.ShouldStopAttack())
            {
                DynelManager.LocalPlayer.StopAttack();
                Chat.WriteLine($"Stopping attack.");
                return;
            }

            if (Extensions.GetLeader(AttackBuddy.Leader) != null)
            {
                if (Extensions.CanAttack())
                {
                    bool validTargetConditions =
                        !_target.Buffs.Contains(253953) &&
                        !_target.Buffs.Contains(NanoLine.ShovelBuffs) &&
                        !_target.Buffs.Contains(302745) &&
                        !_target.IsPlayer &&
                        _target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= AttackBuddy.Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange;

                    if (validTargetConditions)
                    {
                        DynelManager.LocalPlayer.Attack(_target);
                        //Chat.WriteLine($"Attacking {_target.Name}.");

                        //if (Targeting.TargetChar != null)
                        //{
                        //    Chat.WriteLine($"{Targeting.TargetChar?.Health}");
                        //}

                        _fightStartTime = Time.NormalTime;
                    }
                }
                else if (Extensions.IsBoss(_target))
                {
                    bool shouldSwitch =
                        _target != AttackBuddy._switchMobPrecision.FirstOrDefault() &&
                        _target != AttackBuddy._switchMobCharging.FirstOrDefault() &&
                        _target != AttackBuddy._switchMobShield.FirstOrDefault();

                    if (shouldSwitch)
                    {
                        List<SimpleChar> switchList = null;

                        if (AttackBuddy._switchMobPrecision.Count >= 1)
                            switchList = AttackBuddy._switchMobPrecision;
                        else if (AttackBuddy._switchMobCharging.Count >= 1)
                            switchList = AttackBuddy._switchMobCharging;
                        else if (AttackBuddy._switchMobShield.Count >= 1)
                            switchList = AttackBuddy._switchMobShield;
                        else if (AttackBuddy._switchMob.Count >= 1)
                            switchList = AttackBuddy._switchMob;
                        else if (AttackBuddy._mob.Count >= 1)
                            switchList = AttackBuddy._mob;

                        if (switchList != null && DynelManager.LocalPlayer.FightingTarget != null)
                        {
                            SimpleChar switchTarget = switchList.FirstOrDefault();
                            if (switchTarget != null && switchTarget.Health > 0)
                            {
                                _target = switchTarget;
                                DynelManager.LocalPlayer.Attack(_target);
                                //Chat.WriteLine($"Switching to _target {_target.Name}.");
                                _fightStartTime = Time.NormalTime;
                            }
                        }
                    }
                }
                else if (AttackBuddy._switchMob.Count >= 1 && _target.Name != AttackBuddy._switchMob.FirstOrDefault().Name)
                {
                    if (DynelManager.LocalPlayer.FightingTarget != null)
                    {
                        SimpleChar switchTarget = AttackBuddy._switchMob.FirstOrDefault();
                        if (switchTarget != null && switchTarget.Health > 0)
                        {
                            _target = switchTarget;
                            DynelManager.LocalPlayer.Attack(_target);
                            //Chat.WriteLine($"Switching to _target {_target.Name}.");
                            _fightStartTime = Time.NormalTime;
                        }
                    }
                }
            }

            if (Extensions.ShouldTaunt(_target)
                && AttackBuddy._settings["Taunt"].AsBool())
            {
                if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > AttackBuddy.Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange)
                {
                    TauntingTools.HandleTaunting(_target);
                }
            }
        }
    }
}
