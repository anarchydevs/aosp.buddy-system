using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;

namespace InfBuddy
{
    public class FightState : PositionHolder, IState
    {
        public const double FightTimeout = 70f;

        private SimpleChar _target;
        private SimpleChar _primevalSpinetooth;
        private static Corpse _corpse;


        private double _fightStartTime;

        private static bool _missionsLoaded = false;
        private static bool _initLOS = false;

        public FightState(SimpleChar target) : base(Constants.DefendPos, 3f, 1)
        {
            _target = target;
        }

        public IState GetNextState()
        {
            _corpse = DynelManager.Corpses
                .Where(c => c.Name.Contains("Remains of "))
                .FirstOrDefault();

            if (Game.IsZoning) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId)
            {
                if (Extensions.IsClear() || Extensions.CanExit(_missionsLoaded))
                    return new ExitMissionState();

                if (Extensions.IsNull(_target)
                    || Time.NormalTime > _fightStartTime + FightTimeout)
                    return new IdleState();

                if (InfBuddy._settings["Looting"].AsBool()
                    && _corpse != null
                    && Extensions.IsNull(_target))
                    return new LootingState();
            }

            if (Playfield.ModelIdentity.Instance != Constants.NewInfMissionId)
                return new IdleState();

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

            _missionsLoaded = false;
            _initLOS = false;
        }
        public void LineOfSightLogic()
        {
            if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32()
                && _target?.IsInLineOfSight == false && !_initLOS)
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
            if (Game.IsZoning || !Team.IsInTeam) { return; }

            if (Team.IsInTeam)
            {
                foreach (TeamMember member in Team.Members)
                {
                    if (!ReformState._teamCache.Contains(member.Identity))
                        ReformState._teamCache.Add(member.Identity);
                }
            }

            if (Game.IsZoning || _target == null) { return; }

            _primevalSpinetooth = DynelManager.NPCs
             .Where(c => c.Health > 0
                 && c.Name.Contains("Primeval Spinetooth")
                 && !c.Name.Contains("Remains of "))
             .FirstOrDefault();

            if (!_missionsLoaded && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                _missionsLoaded = true;

            LineOfSightLogic();

            if (_target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) < 20f
                && !DynelManager.LocalPlayer.IsAttackPending
                && !DynelManager.LocalPlayer.IsAttacking/* && _target.Name != "Guardian Spirit of Purification"*/)
                DynelManager.LocalPlayer.Attack(_target);

            if (_primevalSpinetooth != null
                && DynelManager.LocalPlayer.FightingTarget == null
                && !DynelManager.LocalPlayer.IsAttackPending)
                DynelManager.LocalPlayer.Attack(_primevalSpinetooth);

            if (!_target.IsMoving && _target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 20f)
                InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)_target?.Position);

            if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                Extensions.HandlePathing(_target);
        }
    }
}
