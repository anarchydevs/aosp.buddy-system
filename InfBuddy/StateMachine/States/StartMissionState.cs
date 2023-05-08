using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;
using System.Threading.Tasks;

namespace InfBuddy
{
    public class StartMissionState : IState
    {
        private static bool _init = false;

        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            if (Extensions.HasDied())
                return new DiedState();

            if (Extensions.IsAtStarterPos() && !InfBuddy.NavMeshMovementController.IsNavigating
                && !DynelManager.NPCs.Any(c => c.Name == Constants.QuestStarterName)
                && DynelManager.NPCs.Any(c => c.Name == Constants.SpiritNPCName))
            {
                if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                    return new RoamState();

                Constants.DefendPos = new Vector3(165.6f, 2.2f, 186.4f);
                return new DefendSpiritState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("StartMissionState::OnStateEnter");

            InfBuddy._stateTimeOut = Time.NormalTime;
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("StartMissionState::OnStateExit");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            Dynel _yutto = DynelManager.NPCs
                .Where(c => c.Name == Constants.QuestStarterName)
                .FirstOrDefault();

            if (_yutto != null && Extensions.IsAtStarterPos()
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
        }
    }
}
