using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;
using System.Threading.Tasks;
using static MitaarBuddy.MitaarBuddy;

namespace MitaarBuddy
{
    public class FarmingState : IState
    {

        private static Corpse _sinuhCorpse;

        public static Vector3 _sinuhCorpsePos = Vector3.Zero;

        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) <= 10f)
                return new ReformState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Pause for looting, 20 sec");

        }

        public void OnStateExit()
        {

        }

        public void Tick()
        {
            _sinuhCorpse = DynelManager.Corpses
                         .Where(c => c.Name.Contains("Remains of Technomaster Sinuh"))
                             .FirstOrDefault();

            _sinuhCorpsePos = (Vector3)_sinuhCorpse?.Position;

            //Path to corpse
            if (DynelManager.LocalPlayer.Position.DistanceFrom(_sinuhCorpsePos) > 3.0f)
                MovementController.Instance.SetDestination(_sinuhCorpsePos);


            if (!_initCorpse && Team.IsInTeam && Playfield.ModelIdentity.Instance == 6017)
            {

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