using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using Shared;
using System.Collections.Generic;
using System.Linq;

namespace RoamBuddy
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
                || (Time.NormalTime > _fightStartTime + _fightTimeout && _target?.MaxHealth <= 999999))
            {
                _target = null;

                if (RoamBuddy._settings["Looting"].AsBool())
                {
                    List<SimpleChar> mobs = DynelManager.NPCs
                        .Where(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= RoamBuddy.AttackRange
                            && !Constants._ignores.Contains(c.Name) && c.IsAlive && c.IsInLineOfSight)
                        .ToList();

                    if (mobs?.Count == 0)
                    {
                        return new LootState();
                    }
                    else
                        return new RoamState();
                }
                else
                    return new RoamState();
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

            if (Extensions.GetLeader(RoamBuddy.Leader) != null)
            {
                if (Extensions.ShouldStopAttack())
                {
                    DynelManager.LocalPlayer.StopAttack();
                    return;
                }

                if (Extensions.CanAttack())
                {
                    if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= RoamBuddy.Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange)
                    {
                        if (RoamBuddy.ModeSelection.Path == (RoamBuddy.ModeSelection)RoamBuddy._settings["ModeSelection"].AsInt32())
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
                        if (RoamBuddy._switchMob.Count >= 1)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget != null)
                            {
                                //Idk why this is here some correction no doubt
                                if (RoamBuddy._switchMob.FirstOrDefault().Health == 0) { return; }

                                _target = RoamBuddy._switchMob.FirstOrDefault();
                                DynelManager.LocalPlayer.Attack(_target);
                                Chat.WriteLine($"Switching to target {_target.Name}.");
                                _fightStartTime = Time.NormalTime;
                                return;
                            }
                        }
                        else if (RoamBuddy._mob.Count >= 1)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget != null)
                            {
                                //Idk why this is here some correction no doubt
                                if (RoamBuddy._mob.FirstOrDefault().Health == 0) { return; }

                                _target = RoamBuddy._mob.FirstOrDefault();
                                DynelManager.LocalPlayer.Attack(_target);
                                Chat.WriteLine($"Switching to target {_target.Name}.");
                                _fightStartTime = Time.NormalTime;
                                return;
                            }
                        }
                    }
                    else if (RoamBuddy._switchMob.Count >= 1 && _target.Name != RoamBuddy._switchMob.FirstOrDefault().Name)
                    {
                        if (DynelManager.LocalPlayer.FightingTarget != null)
                        {
                            //Idk why this is here some correction no doubt
                            if (RoamBuddy._switchMob.FirstOrDefault().Health == 0) { return; }

                            _target = RoamBuddy._switchMob.FirstOrDefault();
                            DynelManager.LocalPlayer.Attack(_target);
                            Chat.WriteLine($"Switching to target {_target.Name}.");
                            _fightStartTime = Time.NormalTime;
                            return;
                        }
                    }
                }
            }

            if (Extensions.ShouldTaunt(_target)
                && RoamBuddy.ModeSelection.Taunt == (RoamBuddy.ModeSelection)RoamBuddy._settings["ModeSelection"].AsInt32())
            {
                if (Extensions.GetLeader(RoamBuddy.Leader) != null)
                {
                    if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > RoamBuddy.Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange)
                    {
                        TauntingTools.HandleTaunting(_target);
                    }
                }
            }

            if (!_target.IsMoving &&
                _target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > RoamBuddy.Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange
                && RoamBuddy.ModeSelection.Path == (RoamBuddy.ModeSelection)RoamBuddy._settings["ModeSelection"].AsInt32())
            {
                MovementController.Instance.SetDestination(_target.Position);
            }
        }
    }
}
