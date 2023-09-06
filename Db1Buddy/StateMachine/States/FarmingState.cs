using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;

namespace Db1Buddy
{
    public class FarmingState : IState
    {
        public static bool _initCorpse = false;
        private static double _timeToDisband;
        private static Corpse _mikkelsenCorpse;

        public static Vector3 _mikkelsenCorpsePos = Vector3.Zero;

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.PWId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) <= 10f)
                return new ReformState();

            return null;
        }

        public void OnStateEnter()
        {
            Db1Buddy.MikkelsenCorpse = false;
            Chat.WriteLine("Pause for looting, 30 sec");
            _timeToDisband = Time.NormalTime + 30; // Schedule disband 30 seconds from now
        }

        public void OnStateExit()
        {
            // Cleanup actions
        }

        public void Tick()
        {
            _mikkelsenCorpse = DynelManager.Corpses
                         .Where(c => c.Name.Contains("Remains of Ground Chief Mikkelsen"))
                             .FirstOrDefault();

            _mikkelsenCorpsePos = (Vector3)_mikkelsenCorpse?.Position;

            //Path to corpse
            if (DynelManager.LocalPlayer.Position.DistanceFrom(_mikkelsenCorpsePos) > 3.0f)
                MovementController.Instance.SetDestination(_mikkelsenCorpsePos);

            if (!_initCorpse && Team.IsInTeam && Playfield.ModelIdentity.Instance == 6003)
            {
                // Check if it's time to disband
                if (Time.NormalTime >= _timeToDisband)
                {
                    Chat.WriteLine("Done, Disbanding");
                    Team.Disband();
                    _initCorpse = true; // Prevent this block from running again
                }
            }
        }
    }
}
