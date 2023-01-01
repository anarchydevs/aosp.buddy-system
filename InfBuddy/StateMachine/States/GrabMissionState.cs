using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfBuddy
{
    public class GrabMissionState : IState
    {
        private static bool _init = false;

        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                return new MoveToEntranceState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("GrabMissionState::OnStateEnter");

            InfBuddy._stateTimeOut = Time.NormalTime;
        }

        public void OnStateExit()
        {
            Chat.WriteLine("GrabMissionState::OnStateExit");
        }

        public void Tick()
        {
            Dynel _yutto = DynelManager.NPCs
                .Where(c => c.Name == Constants.QuestGiverName)
                .FirstOrDefault();

            if (_yutto != null && Extensions.IsAtYutto()
                && !_init)
            {
                _init = true;

                Task.Factory.StartNew(
                    async () =>
                    {
                        NpcDialog.Open(_yutto);
                        await Task.Delay(10000);
                        _init = false;
                    });
            }

            if (!Extensions.IsAtYutto() && Time.NormalTime > InfBuddy._stateTimeOut + 30f)
            {
                InfBuddy._stateTimeOut = Time.NormalTime;

                InfBuddy.NavMeshMovementController.Halt();
                //DynelManager.LocalPlayer.Position = new Vector3(DynelManager.LocalPlayer.Position.X, DynelManager.LocalPlayer.Position.Y, DynelManager.LocalPlayer.Position.Z - 4f);
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(2769.6f, 24.6f, 3319.9f));
                InfBuddy.NavMeshMovementController.AppendNavMeshDestination(Constants.QuestGiverPos);
            }
        }
    }
}
