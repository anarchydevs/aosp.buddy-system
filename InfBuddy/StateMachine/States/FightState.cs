using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace InfBuddy
{
    public class FightState : PositionHolder, IState
    {
        public const double FightTimeout = 70f;

        private SimpleChar _target;

        private double _fightStartTime;

        private static bool _missionsLoaded = false;
        private static bool _initLOS = false;

        public FightState(SimpleChar target) : base(Constants.DefendPos, 3f, 1)
        {
            _target = target;
        }

        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (Extensions.CanExit(_missionsLoaded))
                return new ExitMissionState();

            if (Extensions.IsNull(_target)
                || Time.NormalTime > _fightStartTime + FightTimeout)
            {
                if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                    return new RoamState();

                return new DefendSpiritState();
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

            _missionsLoaded = false;
            _initLOS = false;
        }
        public void LineOfSightLogic()
        {
            if (_target?.IsInLineOfSight == false && !_initLOS)
            {
                InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)_target?.Position);
                _initLOS = true;
            }
            else if (_target?.IsInLineOfSight == true && _target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) < 4f
                && InfBuddy.NavMeshMovementController.IsNavigating && _initLOS)
            {
                InfBuddy.NavMeshMovementController.Halt();
                _initLOS = false;
            }
        }

        public void Tick()
        {
            if (_target == null) { return; }

            //REASON: Edge case for some reason randomly hitting a null reference, the SimpleChar is not null but the Accel and various others are.
            if (_target.Name == "NoName") { return; }
                
            if (!_missionsLoaded && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                _missionsLoaded = true;

            LineOfSightLogic();

            if (_target?.IsInAttackRange() == true && !DynelManager.LocalPlayer.IsAttackPending
                && !DynelManager.LocalPlayer.IsAttacking && _target.Name != "Guardian Spirit of Purification")
                DynelManager.LocalPlayer.Attack(_target);

            if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32()
                || Extensions.GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(Extensions.CharacterWieldedWeapon.Melee)
                || Extensions.GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(Extensions.CharacterWieldedWeapon.MartialArts))
                    Extensions.HandlePathing(_target);

            //Do we need this? try rmove
            //if (Extensions.IsClear())
            //    HoldPosition();
        }
    }
}
