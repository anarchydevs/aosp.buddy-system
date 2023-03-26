using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class FightState : IState
    {
        public const double _fightTimeout = 45f;

        private double _fightStartTime;
        public static float _tetherDistance;


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
                || !DB2Buddy._settings["Toggle"].AsBool()
                || (Time.NormalTime > _fightStartTime + _fightTimeout))
            {
                _target = null;
                return new WarpState();
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

            if (Extensions.GetLeader(DB2Buddy.Leader) != null)
            {
                if (Extensions.CanAttack())
                {
                    if (_target.Buffs.Contains(253953) == false
                        && _target.Buffs.Contains(NanoLine.ShovelBuffs) == false
                        && _target.Buffs.Contains(302745) == false
                        && _target.IsPlayer == false)
                    {
                        if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= DB2Buddy.Config.CharSettings[Game.ClientInst].AttackRange)
                        {
                            DynelManager.LocalPlayer.Attack(_target);
                            Chat.WriteLine($"Attacking {_target.Name}.");
                            _fightStartTime = Time.NormalTime;
                        }
                    }
                }
                else
                {
                     if (DB2Buddy._mob.Count >= 1)
                    {
                        if (DynelManager.LocalPlayer.FightingTarget != null)
                        {
                            if (DB2Buddy._mob.FirstOrDefault().Health == 0) { return; }

                            _target = DB2Buddy._mob.FirstOrDefault();
                            DynelManager.LocalPlayer.Attack(_target);
                            Chat.WriteLine($"Switching to target {_target.Name}.");
                            _fightStartTime = Time.NormalTime;
                            return;
                        }
                    }
                }
            }
            
            
            

        }
    }
}
