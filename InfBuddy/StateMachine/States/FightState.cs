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
        }

        public void Tick()
        {
            if (_target == null) { return; }

            //REASON: Edge case for attacking spirit?
            if (_target.Name == "NoName") { return; }
                
            if (!_missionsLoaded && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                _missionsLoaded = true;

            if (_target?.IsInLineOfSight == false)
                InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)_target?.Position);

            if (_target?.IsInAttackRange() == true && !DynelManager.LocalPlayer.IsAttackPending
                && !DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.Attack(_target);

            if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                Extensions.HandlePathing(_target);

            if (_target == null && Extensions.IsClear())
                HoldPosition();
        }
    }
}
