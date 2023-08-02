using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
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

        public static int _aggToolCounter = 0;


        public static List<int> _ignoreTargetIdentity = new List<int>();

        private SimpleChar _target;

        public FightState(SimpleChar _target)
        {
            _target = _target;
        }

        public IState GetNextState()
        {

            if (!AttackBuddy._settings["Enable"].AsBool())
            {
                return new IdleState();
            }

            if (Extensions.IsNull(_target))
            //|| (Time.NormalTime > _fightStartTime + _fightTimeout && _target?.MaxHealth <= 999999))
            {
                _target = null;
                return new ScanState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("FightState::OnStateEnter");

            _fightStartTime = Time.NormalTime;
        }

        public void OnStateExit()
        {
            Chat.WriteLine("FightState::OnStateExit");

            if (DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.StopAttack();
        }

        public void Tick()
        {
            if (_target == null)
                return;

            //_target.Buffs.contans(shovebuffs)
            if (Extensions.ShouldStopAttack())
            {
                DynelManager.LocalPlayer.StopAttack();
                Chat.WriteLine($"Stopping attack.");
                return;
            }

            if (Extensions.GetLeader(AttackBuddy.Leader) == null)
                return;

            if (!Extensions.CanAttack())
                return;

            if (_target == null || _target.Buffs.Contains(253953) || _target.IsPlayer)
            {
                _target = GetValidAttackTarget();
                if (_target == null)
                    return;
            }

            if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= AttackBuddy.Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange)
            {
                DynelManager.LocalPlayer.Attack(_target);
                Chat.WriteLine($"Attacking {_target.Name}.");
                _fightStartTime = Time.NormalTime;
            }

            //if (Extensions.GetLeader(AttackBuddy.Leader) != null)
            //{
            //    if (Extensions.CanAttack())
            //    {
            //        if (_target.Buffs.Contains(253953) == false
            //            // && _target.Buffs.Contains(NanoLine.ShovelBuffs) == false
            //            //&& _target.Buffs.Contains(302745) == false
            //            && _target.IsPlayer == false)
            //        {
            //            if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= AttackBuddy.Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange)
            //            {
            //                DynelManager.LocalPlayer.Attack(_target);
            //                Chat.WriteLine($"Attacking {_target.Name}.");

            //                //if (Targeting.TargetChar != null)
            //                //{
            //                //    Chat.WriteLine($"{Targeting.TargetChar?.Health}");
            //                //}

            //                _fightStartTime = Time.NormalTime;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        if (Extensions.IsBoss(_target))
            //        {
            //            if (AttackBuddy._switchMobPrecision.Count >= 1)
            //            {
            //                if (DynelManager.LocalPlayer.FightingTarget != null)
            //                {
            //                    if (_target != AttackBuddy._switchMobPrecision.FirstOrDefault()
            //                         && _target != AttackBuddy._switchMobCharging.FirstOrDefault() && _target != AttackBuddy._switchMobShield.FirstOrDefault())
            //                    {
            //                        if (AttackBuddy._switchMobPrecision.FirstOrDefault().Health == 0) { return; }

            //                        _target = AttackBuddy._switchMobPrecision.FirstOrDefault();
            //                        DynelManager.LocalPlayer.Attack(_target);
            //                        Chat.WriteLine($"Switching to _target {_target.Name}.");
            //                        _fightStartTime = Time.NormalTime;
            //                        return;
            //                    }
            //                }
            //            }
            //            else if (AttackBuddy._switchMobCharging.Count >= 1)
            //            {
            //                if (DynelManager.LocalPlayer.FightingTarget != null)
            //                {
            //                    if (_target != AttackBuddy._switchMobPrecision.FirstOrDefault()
            //                        && _target != AttackBuddy._switchMobCharging.FirstOrDefault() && _target != AttackBuddy._switchMobShield.FirstOrDefault())
            //                    {
            //                        if (AttackBuddy._switchMobCharging.FirstOrDefault().Health == 0) { return; }

            //                        _target = AttackBuddy._switchMobCharging.FirstOrDefault();
            //                        DynelManager.LocalPlayer.Attack(_target);
            //                        Chat.WriteLine($"Switching to _target {_target.Name}.");
            //                        _fightStartTime = Time.NormalTime;
            //                        return;
            //                    }
            //                }
            //            }
            //            else if (AttackBuddy._switchMobShield.Count >= 1)
            //            {
            //                if (DynelManager.LocalPlayer.FightingTarget != null)
            //                {
            //                    if (_target != AttackBuddy._switchMobPrecision.FirstOrDefault()
            //                           && _target != AttackBuddy._switchMobCharging.FirstOrDefault() && _target != AttackBuddy._switchMobShield.FirstOrDefault())
            //                    {
            //                        if (AttackBuddy._switchMobShield.FirstOrDefault().Health == 0) { return; }

            //                        _target = AttackBuddy._switchMobShield.FirstOrDefault();
            //                        DynelManager.LocalPlayer.Attack(_target);
            //                        Chat.WriteLine($"Switching to _target {_target.Name}.");
            //                        _fightStartTime = Time.NormalTime;
            //                        return;
            //                    }
            //                }
            //            }
            //            else if (AttackBuddy._switchMob.Count >= 1)
            //            {
            //                if (DynelManager.LocalPlayer.FightingTarget != null)
            //                {
            //                    if (AttackBuddy._switchMob.FirstOrDefault().Health == 0) { return; }

            //                    _target = AttackBuddy._switchMob.FirstOrDefault();
            //                    DynelManager.LocalPlayer.Attack(_target);
            //                    Chat.WriteLine($"Switching to _target {_target.Name}.");
            //                    _fightStartTime = Time.NormalTime;
            //                    return;
            //                }
            //            }
            //            else if (AttackBuddy._mob.Count >= 1)
            //            {
            //                if (DynelManager.LocalPlayer.FightingTarget != null)
            //                {
            //                    if (AttackBuddy._mob.FirstOrDefault().Health == 0) { return; }

            //                    _target = AttackBuddy._mob.FirstOrDefault();
            //                    DynelManager.LocalPlayer.Attack(_target);
            //                    Chat.WriteLine($"Switching to _target {_target.Name}.");
            //                    _fightStartTime = Time.NormalTime;
            //                    return;
            //                }
            //            }
            //        }
            //        else if (AttackBuddy._switchMob.Count >= 1 && _target.Name != AttackBuddy._switchMob.FirstOrDefault().Name)
            //        {
            //            if (DynelManager.LocalPlayer.FightingTarget != null)
            //            {
            //                if (AttackBuddy._switchMob.FirstOrDefault().Health == 0) { return; }

            //                _target = AttackBuddy._switchMob.FirstOrDefault();
            //                DynelManager.LocalPlayer.Attack(_target);
            //                Chat.WriteLine($"Switching to _target {_target.Name}.");
            //                _fightStartTime = Time.NormalTime;
            //                return;
            //            }
            //        }
            //    }
            //}

            if (Extensions.ShouldTaunt(_target)
                && AttackBuddy._settings["Taunt"].AsBool())
            {
                if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > AttackBuddy.Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange)
                {
                    HandleTaunting(_target);
                }
            }
        }
        public static void HandleTaunting(SimpleChar _target)
        {
            Item item = null;

            if (Inventory.Find(83920, out item) || // Aggression Enhancer 
                Inventory.Find(83919, out item) || // Aggression Multiplier
                Inventory.Find(152029, out item) || // Aggression Enhancer (Jealousy Augmented) 
                Inventory.Find(152028, out item) || // Aggression Multiplier (Jealousy Augmented) 
                Inventory.Find(244655, out item) || // Scorpio's Aim of Anger
                Inventory.Find(253186, out item) || // Codex of the Insulting Emerto (Low)
                Inventory.Find(253187, out item))   // Codex of the Insulting Emerto (High)
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    item.Use(_target, true);
                }
            }
        }

        private SimpleChar GetValidAttackTarget()
        {
            SimpleChar _target = null;

            // Check if any valid _target is available in priority order

            if (AttackBuddy._switchMob.Count > 0 && AttackBuddy._switchMob.First().Health > 0)
            {
                _target = AttackBuddy._switchMob.First();
            }
            else if (AttackBuddy._mob.Count > 0 && AttackBuddy._mob.First().Health > 0)
            {
                _target = AttackBuddy._mob.First();
            }
            else if (AttackBuddy._bossMob.Count > 0 && AttackBuddy._bossMob.First().Health > 0)
            {
                _target = AttackBuddy._bossMob.First();
            }
            else if (AttackBuddy._switchMobPrecision.Count > 0 && AttackBuddy._switchMobPrecision.First().Health > 0)
            {
                _target = AttackBuddy._switchMobPrecision.First();
            }
            else if (AttackBuddy._switchMobCharging.Count > 0 && AttackBuddy._switchMobCharging.First().Health > 0)
            {
                _target = AttackBuddy._switchMobCharging.First();
            }
            else if (AttackBuddy._switchMobShield.Count > 0 && AttackBuddy._switchMobShield.First().Health > 0)
            {
                _target = AttackBuddy._switchMobShield.First();
            }

            return _target;
        }

    }
}
