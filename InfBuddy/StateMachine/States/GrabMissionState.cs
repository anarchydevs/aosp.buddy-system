using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;

namespace InfBuddy
{
    public class GrabMissionState : IState
    {
        private static bool _init = false;
        private double _scheduledExecutionTime = 0;

        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                return new MoveToEntranceState();

            return null;
        }

        public void OnStateEnter()
        {
            InfBuddy._stateTimeOut = Time.NormalTime;
        }

        public void OnStateExit()
        {
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

            Dynel _yutto = DynelManager.NPCs
                .Where(c => c.Name == Constants.QuestGiverName)
                .FirstOrDefault();

            if (_yutto != null && Extensions.IsAtYutto() && !_init)
            {
                _init = true;
                _scheduledExecutionTime = Time.NormalTime + 10; // 10-second delay
                NpcDialog.Open(_yutto);
            }

            if (Time.NormalTime >= _scheduledExecutionTime && _init)
            {
                _init = false;
            }

            if (!Extensions.IsAtYutto() && Time.NormalTime > InfBuddy._stateTimeOut + 30)
            {
                InfBuddy._stateTimeOut = Time.NormalTime;
                InfBuddy.NavMeshMovementController.Halt();
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(2769.6f, 24.6f, 3319.9f));
                InfBuddy.NavMeshMovementController.AppendNavMeshDestination(Constants.QuestGiverPos);
            }
        }
    }
}





//using AOSharp.Common.GameData;
//using AOSharp.Core;
//using AOSharp.Core.UI;
//using System.Linq;

//namespace InfBuddy
//{
//    public class GrabMissionState : IState
//    {
//        private static bool _init = false;

//        public IState GetNextState()
//        {
//            if (Game.IsZoning) { return null; }

//            if (Extensions.HasDied())
//                return new DiedState();

//            if (Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
//                return new MoveToEntranceState();

//            return null;
//        }

//        public void OnStateEnter()
//        {
//            //Chat.WriteLine("GrabMissionState::OnStateEnter");

//            InfBuddy._stateTimeOut = Time.NormalTime;
//        }

//        public void OnStateExit()
//        {
//            //Chat.WriteLine("GrabMissionState::OnStateExit");
//        }

//        public void Tick()
//        {
//            if (Game.IsZoning || !Team.IsInTeam) { return; }

//            if (Game.IsZoning || !Team.IsInTeam) { return; }

//            if (Team.IsInTeam)
//            {
//                foreach (TeamMember member in Team.Members)
//                {
//                    if (!ReformState._teamCache.Contains(member.Identity))
//                        ReformState._teamCache.Add(member.Identity);
//                }
//            }

//            Dynel _yutto = DynelManager.NPCs
//                .Where(c => c.Name == Constants.QuestGiverName)
//                .FirstOrDefault();

//            if (_yutto != null && Extensions.IsAtYutto()
//                && !_init)
//            {
//                _init = true;

//                Task.Factory.StartNew(
//                    async () =>
//                    {
//                        NpcDialog.Open(_yutto);
//                        await Task.Delay(10000);
//                        _init = false;
//                    });
//            }

//            if (!Extensions.IsAtYutto()
//                && Time.NormalTime > InfBuddy._stateTimeOut + 30f)
//            {
//                InfBuddy._stateTimeOut = Time.NormalTime;

//                InfBuddy.NavMeshMovementController.Halt();
//                InfBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(2769.6f, 24.6f, 3319.9f));
//                InfBuddy.NavMeshMovementController.AppendNavMeshDestination(Constants.QuestGiverPos);
//            }
//        }
//    }
//}
