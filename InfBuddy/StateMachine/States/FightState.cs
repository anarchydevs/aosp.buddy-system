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
            if (Game.IsZoning) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (Extensions.CanExit(_missionsLoaded))
                return new ExitMissionState();

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId
                 && InfBuddy._settings["Looting"].AsBool()
                && _corpse != null)
                return new LootingState();

            if (Extensions.IsNull(_target)
                || Time.NormalTime > _fightStartTime + FightTimeout)
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
           // Chat.WriteLine("FightState::OnStateExit");

            _missionsLoaded = false;
            _initLOS = false;
        }
        public void LineOfSightLogic()
        {


            if (_target?.IsInLineOfSight == false && !_initLOS 
                && InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
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
            if (Game.IsZoning || _target == null) { return; }

            _corpse = DynelManager.Corpses
                .Where(c => c.Name.Contains("Remains of "))
                .FirstOrDefault();

            //REASON: Edge case for some reason randomly hitting a null reference, the SimpleChar is not null but the Accel and various others are.
            //if (_target.Name == "NoName") { return; }

            if (!_missionsLoaded && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                _missionsLoaded = true;

            LineOfSightLogic();

            if //(_target?.IsInAttackRange() == true && 
                 (_target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) < 20f
                && !DynelManager.LocalPlayer.IsAttackPending
                && !DynelManager.LocalPlayer.IsAttacking/* && _target.Name != "Guardian Spirit of Purification"*/)
                DynelManager.LocalPlayer.Attack(_target);

            if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                Extensions.HandlePathing(_target);



           
        }
    }
}
