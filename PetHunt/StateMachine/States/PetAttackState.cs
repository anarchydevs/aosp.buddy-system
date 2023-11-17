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

        private double attackStarted;

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
            {

                return;
            }
                

            List<SimpleChar> switchList = null;

            bool validTargetConditions =
                !_target.Buffs.Contains(253953) &&
                !_target.Buffs.Contains(NanoLine.ShovelBuffs) &&
                !_target.Buffs.Contains(302745) &&
                !_target.IsPlayer && !_target.IsPet && 
                _target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) 
                <= PetHunt.Config.CharSettings[DynelManager.LocalPlayer.Name].HuntRange;

            bool isPetAttacking = DynelManager.LocalPlayer.Pets.Any(pet => pet.Character.IsAttacking);

            bool isPetEngaged = DynelManager.LocalPlayer.Pets.Any(pet => pet.Character.FightingTarget != null);

            bool isPetAttackPending = DynelManager.LocalPlayer.Pets.Any(pet => pet.Character.IsPathing);
            bool isAttackPet = DynelManager.LocalPlayer.Pets.Any(pet => pet.Type == PetType.Attack || pet.Type == PetType.Support);


            if (Time.AONormalTime > attackStarted + 1)
            {
                if (isAttackPet)
                {
                    if (!isPetAttacking && !isPetEngaged && !isPetAttackPending && validTargetConditions)
                    {
                        DynelManager.LocalPlayer.Pets.Attack(_target.Identity);
                        _fightStartTime = Time.NormalTime;
                        Chat.WriteLine($"Sent pet(s) to attack {_target.Name}");
                    }
                }
                attackStarted = Time.AONormalTime;
            }
        }

        public void OnStateExit()
        {
            DynelManager.LocalPlayer.Pets.Follow();
        }

        public static bool IsNull(SimpleChar _target)
        {
            return _target == null
                || _target?.IsValid == false
                || _target?.Health == 0;
        }

    }
}
