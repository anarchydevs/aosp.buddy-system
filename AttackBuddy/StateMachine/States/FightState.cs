using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttackBuddy
{
    public class FightState : IState
    {
        public const double _fightTimeout = 45f;

        private double _fightStartTime;
        public static float _tetherDistance;

        public static int _aggToolCounter = 0;

        public static List<Identity> corpseToLootIdentity = new List<Identity>();
        public static List<Corpse> corpsesToLoot = new List<Corpse>();
        public static List<Identity> lootedCorpses = new List<Identity>();

        public static List<int> _ignoreTargetIdentity = new List<int>();

        private SimpleChar _target;

        public FightState(SimpleChar target)
        {
            _target = target;
        }

        public IState GetNextState()
        {
            if (Extensions.IsNull(_target)
                || !AttackBuddy._settings["Toggle"].AsBool()
                || (Time.NormalTime > _fightStartTime + _fightTimeout && _target?.MaxHealth <= 999999))
            {
                _target = null;
                return new ScanState();
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

            //_target.Buffs.contans(shovebuffs)
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
                    if (_target.Buffs.Contains(253953) == false
                        && _target.Buffs.Contains(NanoLine.ShovelBuffs) == false
                        && _target.Buffs.Contains(302745) == false
                        && _target.IsPlayer == false) 
                    {
                        if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= AttackBuddy.Config.CharSettings[Game.ClientInst].AttackRange)
                        {
                            DynelManager.LocalPlayer.Attack(_target);
                            Chat.WriteLine($"Attacking {_target.Name}.");

                            //if (Targeting.TargetChar != null)
                            //{
                            //    Chat.WriteLine($"{Targeting.TargetChar?.Health}");
                            //}

                            _fightStartTime = Time.NormalTime;
                        }
                    }
                }
                else
                {
                    if (Extensions.IsBoss(_target))
                    {
                        if (AttackBuddy._switchMobPrecision.Count >= 1)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget != null)
                            {
                                if (_target != AttackBuddy._switchMobPrecision.FirstOrDefault()
                                     && _target != AttackBuddy._switchMobCharging.FirstOrDefault() && _target != AttackBuddy._switchMobShield.FirstOrDefault())
                                {
                                    if (AttackBuddy._switchMobPrecision.FirstOrDefault().Health == 0) { return; }

                                    _target = AttackBuddy._switchMobPrecision.FirstOrDefault();
                                    DynelManager.LocalPlayer.Attack(_target);
                                    Chat.WriteLine($"Switching to target {_target.Name}.");
                                    _fightStartTime = Time.NormalTime;
                                    return;
                                }
                            }
                        }
                        else if (AttackBuddy._switchMobCharging.Count >= 1)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget != null)
                            {
                                if (_target != AttackBuddy._switchMobPrecision.FirstOrDefault()
                                    && _target != AttackBuddy._switchMobCharging.FirstOrDefault() && _target != AttackBuddy._switchMobShield.FirstOrDefault())
                                {
                                    if (AttackBuddy._switchMobCharging.FirstOrDefault().Health == 0) { return; }

                                    _target = AttackBuddy._switchMobCharging.FirstOrDefault();
                                    DynelManager.LocalPlayer.Attack(_target);
                                    Chat.WriteLine($"Switching to target {_target.Name}.");
                                    _fightStartTime = Time.NormalTime;
                                    return;
                                }
                            }
                        }
                        else if (AttackBuddy._switchMobShield.Count >= 1)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget != null)
                            {
                                if (_target != AttackBuddy._switchMobPrecision.FirstOrDefault()
                                       && _target != AttackBuddy._switchMobCharging.FirstOrDefault() && _target != AttackBuddy._switchMobShield.FirstOrDefault())
                                {
                                    if (AttackBuddy._switchMobShield.FirstOrDefault().Health == 0) { return; }

                                    _target = AttackBuddy._switchMobShield.FirstOrDefault();
                                    DynelManager.LocalPlayer.Attack(_target);
                                    Chat.WriteLine($"Switching to target {_target.Name}.");
                                    _fightStartTime = Time.NormalTime;
                                    return;
                                }
                            }
                        }
                        else if (AttackBuddy._switchMob.Count >= 1)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget != null)
                            {
                                if (AttackBuddy._switchMob.FirstOrDefault().Health == 0) { return; }

                                _target = AttackBuddy._switchMob.FirstOrDefault();
                                DynelManager.LocalPlayer.Attack(_target);
                                Chat.WriteLine($"Switching to target {_target.Name}.");
                                _fightStartTime = Time.NormalTime;
                                return;
                            }
                        }
                        else if (AttackBuddy._mob.Count >= 1)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget != null)
                            {
                                if (AttackBuddy._mob.FirstOrDefault().Health == 0) { return; }

                                _target = AttackBuddy._mob.FirstOrDefault();
                                DynelManager.LocalPlayer.Attack(_target);
                                Chat.WriteLine($"Switching to target {_target.Name}.");
                                _fightStartTime = Time.NormalTime;
                                return;
                            }
                        }
                    }
                    else if (AttackBuddy._switchMob.Count >= 1 && _target.Name != AttackBuddy._switchMob.FirstOrDefault().Name)
                    {
                        if (DynelManager.LocalPlayer.FightingTarget != null)
                        {
                            if (AttackBuddy._switchMob.FirstOrDefault().Health == 0) { return; }

                            _target = AttackBuddy._switchMob.FirstOrDefault();
                            DynelManager.LocalPlayer.Attack(_target);
                            Chat.WriteLine($"Switching to target {_target.Name}.");
                            _fightStartTime = Time.NormalTime;
                            return;
                        }
                    }
                }
            }

            if (Extensions.ShouldTaunt(_target)
                && AttackBuddy._settings["Taunt"].AsBool())
            {
                if (Extensions.GetLeader(AttackBuddy.Leader) != null)
                {
                    if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > AttackBuddy.Config.CharSettings[Game.ClientInst].AttackRange)
                    {
                        if (Inventory.Find(83920, 83919, out Item aggroTool)) //Aggression Enhancer 
                        {
                            if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                            {
                                aggroTool.Use(_target, true);
                                return;
                            }
                        }
                        else if (Inventory.Find(83919, 83919, out Item aggroMultiTool)) //Aggression Multiplier
                        {
                            if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                            {
                                aggroMultiTool.Use(_target, true);
                                return;
                            }
                        }
                        else if (Inventory.Find(152029, 152029, out Item JealousyTool)) //Aggression Enhancer (Jealousy Augmented) 
                        {
                            if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                            {
                                JealousyTool.Use(_target, true);
                                return;
                            }
                        }
                        else if (Inventory.Find(152028, 152028, out Item JealousyMultiTool)) //Aggression Multiplier (Jealousy Augmented) 
                        {
                            if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                            {
                                JealousyMultiTool.Use(_target, true);
                                return;
                            }
                        }
                        else if (Inventory.Find(244655, 244655, out Item scorpioTool)) //Scorpio's Aim of Anger
                        {
                            if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                            {
                                scorpioTool.Use(_target, true);
                                return;
                            }
                        }
                        else if (Inventory.Find(253186, 253186, out Item EmertoLow))//Codex of the Insulting Emerto
                        {
                            if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                            {
                                EmertoLow.Use(_target, true);
                                return;
                            }
                        }
                        else if (Inventory.Find(253187, 253187, out Item EmertoHigh))//Codex of the Insulting Emerto
                        {
                            if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                            {
                                EmertoHigh.Use(_target, true);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
