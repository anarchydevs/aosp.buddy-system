using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;
using System.Threading.Tasks;

namespace VortexxBuddy
{
    public class FarmingState : IState
    {
        public static bool _initCorpse = false;

        private static Corpse _vortexxCorpse;

        public static Vector3 _vortexxCorpsePos = Vector3.Zero;

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20f)
                return new ReformState();

            return null;
        }

        public void OnStateEnter()
        {
        }

        public void OnStateExit()
        {
        }

        public void Tick()
        {
            _vortexxCorpse = DynelManager.Corpses
                   .Where(c => c.Name.Contains("Remains of Ground Chief Vortexx"))
                       .FirstOrDefault();

            _vortexxCorpsePos = (Vector3)_vortexxCorpse?.Position;

            //Path to corpse
            if (DynelManager.LocalPlayer.Position.DistanceFrom(_vortexxCorpsePos) > 3.0f)
                MovementController.Instance.SetDestination(_vortexxCorpsePos);

            if (!_initCorpse && Team.IsInTeam && Playfield.ModelIdentity.Instance == Constants.VortexxId)
            {
                Chat.WriteLine("Pause for looting, 20 sec");
                Task.Factory.StartNew(
                    async () =>
                    {
                        await Task.Delay(20000);
                        Chat.WriteLine("Done, Disbanding");
                        Team.Disband();
                    });

                _initCorpse = true;
            }

        }
    }
}