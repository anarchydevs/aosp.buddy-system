using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;

namespace InfBuddy
{
    public class MoveToEntranceState : IState
    {
        private const int _minWait = 5;
        private const int _maxWait = 7;

        private static bool _init = false;

        private double _scheduledExecutionTime = 0;
        private int randomWait = 0;

        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId)
            {
                if (InfBuddy._settings["Leech"].AsBool())
                {
                    return new LeechState();
                }
                else
                {
                    if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                    {
                        if (DynelManager.LocalPlayer.Identity != InfBuddy.Leader)
                        {
                            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.QuestStarterPos) < 10f)
                            {
                                //&& Team.Members.Any(c => c.Character != null && c.IsLeader))
                                return new RoamState();
                            }
                            else if (!InfBuddy.NavMeshMovementController.IsNavigating)
                            {
                                InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.QuestStarterPos);
                            }
                        }
                        else
                        {
                            return new MoveToQuestStarterState();
                        }
                    }

                    if (InfBuddy.ModeSelection.Normal == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                    {
                        if (DynelManager.LocalPlayer.Identity != InfBuddy.Leader)
                        {
                            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.QuestStarterPos) < 10f)
                            {
                                //&& Team.Members.Any(c => c.Character != null && c.IsLeader))
                                return new DefendSpiritState();
                            }
                            else if (!InfBuddy.NavMeshMovementController.IsNavigating)
                            {
                                InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.QuestStarterPos);
                            }

                        }
                        else
                        {
                            return new MoveToQuestStarterState();
                        }
                    }
                }
            }
            return null;
        }

        public void OnStateEnter()
        {
            int randomWait = Extensions.Next(_minWait, _maxWait);

            if (DynelManager.LocalPlayer.Identity == InfBuddy.Leader)
                randomWait = 4;

            _scheduledExecutionTime = Time.NormalTime + randomWait;
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("MoveToEntranceState::OnStateExit");

            MovementController.Instance.Halt();

            _init = false;
        }

        public void Tick()
        {
            if (Playfield.ModelIdentity.Instance != Constants.InfernoId
                || Game.IsZoning || !Team.IsInTeam) { return; }

            if (_scheduledExecutionTime <= Time.NormalTime && !_init)
            {
                _init = true;
                InfBuddy._stateTimeOut = Time.NormalTime;

                InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.EntrancePos);
                InfBuddy.NavMeshMovementController.AppendDestination(Constants.EntranceFinalPos);
            }

            if (InfBuddy.NavMeshMovementController.IsNavigating
                && Time.NormalTime > InfBuddy._stateTimeOut + 35f)
            {
                InfBuddy._stateTimeOut = Time.NormalTime;

                InfBuddy.NavMeshMovementController.Halt();
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(2769.6f, 24.6f, 3319.9f));
                InfBuddy.NavMeshMovementController.AppendNavMeshDestination(Constants.EntrancePos);
                InfBuddy.NavMeshMovementController.AppendDestination(Constants.EntranceFinalPos);
            }
        }
    }
}
