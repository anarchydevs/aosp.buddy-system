using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System.Linq;
using System.Threading.Tasks;

namespace CityBuddy
{
    public class ToggleState : IState
    {

        public static bool ChangingState = false;
        public static IPCChannel IPCChannel { get; private set; }


        public IState GetNextState()
        {
            if (ChangingState == true)
                return new AttackState();

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("PullState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("PullState::OnStateExit");
        }

        public void Tick()
        {
            Dynel citycontroller = DynelManager.AllDynels
                .Where(c => c.Name == "City Controller")
                .Where(c => c.DistanceFrom(DynelManager.LocalPlayer) < 10f)
                .FirstOrDefault();

            if (citycontroller != null)
                Logic(citycontroller);
        }

        private static void Logic(Dynel cc)
        {
            Task.Factory.StartNew(
                async () =>
                {
                    await Task.Delay(3000);
                    CityBotOpenCC(cc);
                    await Task.Delay(3000);
                    CityBotToggleCloak();
                    await Task.Delay(1500);
                    ChangingState = true;
                });
        }

        private static void CityBotToggleCloak()
        {
            Chat.WriteLine("Toggling cloak");
            Network.Send(new ToggleCloakMessage()
            {
                Unknown1 = 49152,
            });
            CityBuddy.cloakTime = CityBuddy.gameTime.AddSeconds(3660);

            Chat.WriteLine($"current time {CityBuddy.gameTime}");
            Chat.WriteLine($"Time set {CityBuddy.cloakTime}");
        }

        private static void CityBotOpenCC(Dynel citycontroller)
        {
            Chat.WriteLine("Opening City Controller");
            citycontroller.Use();
        }
    }
}
