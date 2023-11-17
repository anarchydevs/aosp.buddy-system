using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PetHunt
{
    public class PetAttackState : IState
    {
        public const double _fightTimeout = 45f;

        private double _fightStartTime;
        public static float _tetherDistance;

        public static List<int> _ignoreTargetIdentity = new List<int>();

        private SimpleChar _target;

        public PetAttackState(SimpleChar target)
        {
            _target = target;
        }

        public IState GetNextState()
        {

            if (PetHunt._settings["Enable"].AsBool())
            {
                if (IsNull(_target))
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
            Chat.WriteLine("Attacking");

            _fightStartTime = Time.NormalTime;
        }


        public void Tick()
        {
            if (_target == null)
                return;
            List<SimpleChar> switchList = null;

            bool validTargetConditions =
                !_target.Buffs.Contains(253953) &&
                !_target.Buffs.Contains(NanoLine.ShovelBuffs) &&
                !_target.Buffs.Contains(302745) &&
                !_target.IsPlayer &&
                _target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= PetHunt.Config.CharSettings[DynelManager.LocalPlayer.Name].HuntRange;

            bool isPetAttacking = DynelManager.LocalPlayer.Pets.Any(pet =>
                pet.Character.IsAttacking);

            bool isPetEngaged = DynelManager.LocalPlayer.Pets.Any(pet => pet.Character.FightingTarget != null);


            if (!isPetAttacking && !isPetEngaged  && validTargetConditions)
            {
                DynelManager.LocalPlayer.Pets.Attack(_target.Identity);
                _fightStartTime = Time.NormalTime;
            }


            if (PetHunt._switchMob.Count >= 1)
                switchList = PetHunt._switchMob;
            else if (PetHunt._mob.Count >= 1)
                switchList = PetHunt._mob;

            if (switchList != null && isPetEngaged && isPetAttacking)
            {
                SimpleChar switchTarget = switchList.FirstOrDefault();
                if (switchTarget != null && switchTarget.Health > 0)
                {
                    _target = switchTarget;
                    DynelManager.LocalPlayer.Pets.Attack(_target.Identity);
                    //Chat.WriteLine($"Switching to _target {_target.Name}.");
                    _fightStartTime = Time.NormalTime;
                }
            }
            else if (PetHunt._switchMob.Count >= 1 && _target.Name != PetHunt._switchMob.FirstOrDefault().Name)
            {
                if (isPetEngaged && isPetAttacking)
                {
                    SimpleChar switchTarget = PetHunt._switchMob.FirstOrDefault();
                    if (switchTarget != null && switchTarget.Health > 0)
                    {
                        _target = switchTarget;
                        DynelManager.LocalPlayer.Pets.Attack(_target.Identity);
                        //Chat.WriteLine($"Switching to _target {_target.Name}.");
                        _fightStartTime = Time.NormalTime;
                    }
                }
            }
        }


        public void OnStateExit()
        {
            
        }

        public static bool IsNull(SimpleChar _target)
        {
            return _target == null
                || _target?.IsValid == false
                || _target?.Health == 0;
        }

    }
}
