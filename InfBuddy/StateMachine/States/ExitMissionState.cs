using AOSharp.Core;
using AOSharp.Core.UI;

namespace InfBuddy
{
    public class ExitMissionState : IState
    {
        private static bool _init = false;
        private double _scheduledExecutionTime = 0;

        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.InfernoId)
            {
                if (InfBuddy._settings["DoubleReward"].AsBool() && !InfBuddy.DoubleReward)
                {
                    InfBuddy.DoubleReward = true;
                    return new MoveToQuestGiverState();
                }

               else
                {
                    InfBuddy.DoubleReward = false;
                    return new ReformState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Exit Mission");

            int _time = 0;

            if (InfBuddy._settings["DoubleReward"].AsBool() && !InfBuddy.DoubleReward)
                _time = 1;
            else
                _time = 5;

            _scheduledExecutionTime = Time.NormalTime + _time;
            _init = false;

            foreach (Mission mission in Mission.List)
                if (mission.DisplayName.Contains("The Purification"))
                    mission.Delete();
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("ExitMissionState::OnStateExit");

            _init = false;
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

            if (!_init && Time.NormalTime >= _scheduledExecutionTime)
            {
                _init = true;
                InfBuddy._stateTimeOut = Time.NormalTime;

                if (InfBuddy._settings["Leech"].AsBool())
                {
                    DynelManager.LocalPlayer.Position = Constants.LeechMissionExit;
                    InfBuddy.NavMeshMovementController.AppendDestination(Constants.ExitFinalPos);
                }
                else
                {
                    InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.ExitPos);
                    InfBuddy.NavMeshMovementController.AppendDestination(Constants.ExitFinalPos);
                }
            }

            //if (InfBuddy.ModeSelection.Leech != (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32()
            //    && _init
            //    && Time.NormalTime > InfBuddy._stateTimeOut + 15f)
            //{
            //    InfBuddy._stateTimeOut = Time.NormalTime;

            //    InfBuddy.NavMeshMovementController.Halt();
            //    InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.QuestStarterBeforePos);
            //    InfBuddy.NavMeshMovementController.AppendDestination(Constants.ExitPos);
            //    InfBuddy.NavMeshMovementController.AppendDestination(Constants.ExitFinalPos);
            //}
        }
    }
}
