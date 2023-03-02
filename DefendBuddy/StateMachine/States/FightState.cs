using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace DefendBuddy
{
    public class FightState : PositionHolder, IState
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

        public FightState(SimpleChar target) : base(Constants._posToDefend, 3f, 1)
        {
            _target = target;
        }

        public IState GetNextState()
        {
            if (Extensions.IsNull(_target) ||
               (Time.NormalTime > _fightStartTime + _fightTimeout && _target?.MaxHealth <= 999999))
            {
                _target = null;

                if (DefendBuddy._settings["Looting"].AsBool())
                {
                    List<SimpleChar> mobs = DynelManager.NPCs
                        .Where(c => c.Position.DistanceFrom(Constants._posToDefend) <= DefendBuddy.AttackRange
                            && !Constants._ignores.Contains(c.Name) && c.IsAlive && c.IsInLineOfSight)
                        .ToList();

                    if (mobs?.Count == 0)
                        return new LootState();
                    else
                        return new DefendState();
                }
                else
                    return new DefendState();
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

            _aggToolCounter = 0;

            if (DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.StopAttack();
        }

        public void Tick()
        {
            if (_target == null)
                return;

            if (Extensions.GetLeader(DefendBuddy.Leader) != null)
            {
                if (Extensions.ShouldStopAttack())
                {
                    DynelManager.LocalPlayer.StopAttack();
                    return;
                }

                if (Extensions.CanAttack())
                {
                    if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= DefendBuddy.Config.CharSettings[Game.ClientInst].AttackRange)
                    {
                        if (DefendBuddy.ModeSelection.Path == (DefendBuddy.ModeSelection)DefendBuddy._settings["ModeSelection"].AsInt32())
                            MovementController.Instance.Halt();

                        DynelManager.LocalPlayer.Attack(_target);
                        Chat.WriteLine($"Attacking {_target.Name}.");
                        _fightStartTime = Time.NormalTime;
                    }
                }
                else
                {
                    if (_target.MaxHealth >= 1000000)
                    {
                        if (DefendBuddy._switchMob.Count >= 1)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget != null)
                            {
                                //Idk why this is here some correction no doubt
                                if (DefendBuddy._switchMob.FirstOrDefault().Health == 0) { return; }

                                _target = DefendBuddy._switchMob.FirstOrDefault();
                                DynelManager.LocalPlayer.Attack(_target);
                                Chat.WriteLine($"Switching to target {_target.Name}.");
                                _fightStartTime = Time.NormalTime;
                                return;
                            }
                        }
                        else if (DefendBuddy._mob.Count >= 1)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget != null)
                            {
                                //Idk why this is here some correction no doubt
                                if (DefendBuddy._mob.FirstOrDefault().Health == 0) { return; }

                                _target = DefendBuddy._mob.FirstOrDefault();
                                DynelManager.LocalPlayer.Attack(_target);
                                Chat.WriteLine($"Switching to target {_target.Name}.");
                                _fightStartTime = Time.NormalTime;
                                return;
                            }
                        }
                    }
                    else if (DefendBuddy._switchMob.Count >= 1 && _target.Name != DefendBuddy._switchMob.FirstOrDefault().Name)
                    {
                        if (DynelManager.LocalPlayer.FightingTarget != null)
                        {
                            //Idk why this is here some correction no doubt
                            if (DefendBuddy._switchMob.FirstOrDefault().Health == 0) { return; }

                            _target = DefendBuddy._switchMob.FirstOrDefault();
                            DynelManager.LocalPlayer.Attack(_target);
                            Chat.WriteLine($"Switching to target {_target.Name}.");
                            _fightStartTime = Time.NormalTime;
                            return;
                        }
                    }
                }
            }

            if (Extensions.ShouldTaunt(_target)
                && DefendBuddy.ModeSelection.Taunt == (DefendBuddy.ModeSelection)DefendBuddy._settings["ModeSelection"].AsInt32())
            {
                if (Extensions.GetLeader(DefendBuddy.Leader) != null)
                {

                    if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > DefendBuddy.Config.CharSettings[Game.ClientInst].AttackRange)
                    {
                        if (_aggToolCounter >= 5)
                        {
                            //_ignoreTargetIdentity.Add(_target.Identity.Instance);
                            MovementController.Instance.SetDestination(_target.Position);
                        }
                        else if (Inventory.Find(83920, 83920, out Item aggroTool)) //Aggression Enhancer 
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

            if (!_target.IsMoving &&
                _target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > DefendBuddy.Config.CharSettings[Game.ClientInst].AttackRange
                && DefendBuddy.ModeSelection.Path == (DefendBuddy.ModeSelection)DefendBuddy._settings["ModeSelection"].AsInt32())
            {
                MovementController.Instance.SetDestination(_target.Position);
            }
        }
    }
}
