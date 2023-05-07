using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VortexxBuddy
{
    public class FarmingState : IState
    {
        public static bool _initCorpse = false;
        public static bool _atCorpse = false;

        private static Corpse _vortexxCorpse;


        public static Vector3 _vortexxCorpsePos = Vector3.Zero;

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.XanHubId)
                return new ReformState();

            return null;
        }

        public void OnStateEnter()
        {
        }

        public void OnStateExit()
        {
            _atCorpse = false;
            _initCorpse = false;
        }

        public void Tick()
        {

            Dynel Beacon = DynelManager.AllDynels
               .Where(c => c.Name == "Dust Brigade Beacon")
               .FirstOrDefault();

            _vortexxCorpse = DynelManager.Corpses
                   .Where(c => c.Name.Contains("Remains of Ground Chief Vortexx"))
                       .FirstOrDefault();

            _vortexxCorpsePos = (Vector3)_vortexxCorpse?.Position;

            //Path to corpse
            if (!_atCorpse && DynelManager.LocalPlayer.Position.DistanceFrom(_vortexxCorpsePos) > 1)
            {
                VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(_vortexxCorpsePos);

                _atCorpse = true;
            }

            if (!_initCorpse && _atCorpse)
            {
                Chat.WriteLine("Pause for looting, 10 sec");
                Task.Factory.StartNew(
                    async () =>
                    {
                        await Task.Delay(10000);
                        Chat.WriteLine("Done, Leaving");

                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._BeaconPos) > 1)
                        VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._BeaconPos);

                    });

                _initCorpse = true;
            }

            if (Beacon != null && _initCorpse && 
                DynelManager.LocalPlayer.Position.DistanceFrom(Constants._BeaconPos) < 3
                && Extensions.CanProceed())
            {
                Beacon.Use();
            }

        }
    }
}