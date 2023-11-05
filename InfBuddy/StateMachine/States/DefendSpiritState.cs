using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace InfBuddy
{
    public class DefendSpiritState : PositionHolder, IState
    {
        private SimpleChar _target;
        private SimpleChar _charmMob;
        private SimpleChar _primevalSpinetooth;
        private static Corpse _corpse;

        public static Vector3 _corpsePos = Vector3.Zero;

        private static bool _charmMobAttacked = false;
        //public static bool _missionsLoaded = false;
        private static bool _initLOS = false;

        private static double _charmMobAttacking;

        private List<Identity> _charmMobs = new List<Identity>();

        private double _mobStuckStartTime;
        public const double MobStuckTimeout = 600f;

        public DefendSpiritState() : base(Constants.DefendPos, 3f, 1)
        { }

        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            bool missionExists = Mission.List.Exists(m => m.DisplayName.Contains("The Purification Ritual"));

            _corpse = DynelManager.Corpses
                .Where(c => c.Name.Contains("Remains of "))
                .FirstOrDefault();

            if (Extensions.HasDied())
            {
                return new DiedState();
            }
                
            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId)
            {
                if (InfBuddy._settings["Looting"].AsBool() && _corpse != null
                   && Extensions.IsNull(_target))
                {
                    return new LootingState();
                }

                if (Extensions.IsClear() || !missionExists)
                {
                    if (InfBuddy._settings["Looting"].AsBool()  && _corpse != null)
                    {
                        return new LootingState();
                    }
                    else
                    {
                        return new ExitMissionState();
                    }
                }

                if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                {
                    return new RoamState();
                }
            }

            if (Playfield.ModelIdentity.Instance != Constants.NewInfMissionId)
            {
                return new IdleState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Defending");

            _mobStuckStartTime = Time.NormalTime;
        }

        public void OnStateExit() {}

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Team.IsInTeam)
            {
                foreach (TeamMember member in Team.Members)
                {
                    if (!ReformState._teamCache.Contains(member.Identity))
                        ReformState._teamCache.Add(member.Identity);
                }
            }

            _target = DynelManager.NPCs
            .Where(c => c.Health > 0 && !InfBuddy._namesToIgnore.Contains(c.Name))
            .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
            .FirstOrDefault();

            if (_target != null)
            {
                float distanceToTarget = _target.Position.DistanceFrom(DynelManager.LocalPlayer.Position);

                if (distanceToTarget < 20f
                    && !DynelManager.LocalPlayer.IsAttackPending
                    && !DynelManager.LocalPlayer.IsAttacking)
                {
                    DynelManager.LocalPlayer.Attack(_target);
                }

                if (!_target.IsMoving && distanceToTarget > 20f
                     && _target.Name.Contains("Primeval Spinetooth"))
                {
                    InfBuddy.NavMeshMovementController.SetNavMeshDestination(_target.Position);
                }
            }
            
            if (DynelManager.LocalPlayer.HealthPercent > 65 || DynelManager.LocalPlayer.NanoPercent > 65
                    || DynelManager.LocalPlayer.MovementState != MovementState.Sit || !Extensions.Rooted()
                    || DynelManager.LocalPlayer.Position.Distance2DFrom(Constants.DefendPos) > 5)
            {
                HoldPosition();
            }
        }
        public void LineOfSightLogic()
        {
            if (_target?.IsInLineOfSight == true && _target?.Position.DistanceFrom(DynelManager.LocalPlayer.Position) < 4f
                && InfBuddy.NavMeshMovementController.IsNavigating && _initLOS)
            {
                InfBuddy.NavMeshMovementController.Halt();
                _initLOS = false;
            }
        }
    }
}
