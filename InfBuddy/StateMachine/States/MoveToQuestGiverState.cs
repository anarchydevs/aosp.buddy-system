using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;

namespace InfBuddy
{
    public class MoveToQuestGiverState : IState
    {
        private const int _minWait = 2;
        private const int _maxWait = 4;
        private static float _entropy = 1.34f;

        private static bool _init = false;

        private double _scheduledExecutionTime = 0;
        private int randomWait = 0;

        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (!InfBuddy.NavMeshMovementController.IsNavigating && Extensions.IsAtYutto())
                return new GrabMissionState();

            return null;
        }

        public void OnStateEnter()
        {
            if (!Extensions.IsAtYutto())
            {
                Vector3 randoPos = Constants.QuestGiverPos;
                randoPos.AddRandomness((int)_entropy);

                randomWait = Extensions.Next(_minWait, _maxWait);

                if (DynelManager.LocalPlayer.Identity == InfBuddy.Leader || InfBuddy._settings["Merge"].AsBool())
                    randomWait = 1;

                _scheduledExecutionTime = Time.NormalTime + randomWait;

                Chat.WriteLine(" Moving to Yutto");
            }
        }

        public void OnStateExit()
        {
            _init = false;
        }

        public void Tick()
        {
            if (Game.IsZoning || !Team.IsInTeam) { return; }

            if (_scheduledExecutionTime <= Time.NormalTime && !_init)
            {
                _init = true;
                InfBuddy._stateTimeOut = Time.NormalTime;

                Vector3 randoPos = Constants.QuestGiverPos;
                randoPos.AddRandomness((int)_entropy);

                InfBuddy.NavMeshMovementController.SetNavMeshDestination(randoPos);
            }

            if (!Extensions.IsAtYutto()
                && _init
                && InfBuddy.NavMeshMovementController.IsNavigating
                && Time.NormalTime > InfBuddy._stateTimeOut + 40f)
            {
                Vector3 randoPos = Constants.QuestGiverPos;
                randoPos.AddRandomness((int)_entropy);

                InfBuddy._stateTimeOut = Time.NormalTime;

                InfBuddy.NavMeshMovementController.Halt();
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(2743.0f, 24.6f, 3312.0f));
                InfBuddy.NavMeshMovementController.AppendNavMeshDestination(randoPos);
            }
        }
    }
}