using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;

namespace VortexxBuddy
{
    public class FarmingState : IState
    {
        public static bool _initCorpse = false;
        public static bool _atCorpse = false;
        private static double _timeToLeave;

        private static Corpse _vortexxCorpse;


        public static Vector3 _vortexxCorpsePos = Vector3.Zero;

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                 && !Team.Members.Any(c => c.Character == null))
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
            VortexxBuddy.VortexxCorpse = false;
            Chat.WriteLine("Pause for looting, 30 sec");
            _timeToLeave = Time.NormalTime + 30; // Schedule leave 30 seconds from now
        }

        public void Tick()
        {

            Dynel Beacon = DynelManager.AllDynels
               .Where(c => c.Name == "Dust Brigade Beacon")
               .FirstOrDefault();

            _vortexxCorpse = DynelManager.Corpses
                   .Where(c => c.Name.Contains("Remains of Ground Chief Vortexx"))
                       .FirstOrDefault();



            //Path to corpse
            if (_vortexxCorpse != null)
            {
                _vortexxCorpsePos = (Vector3)_vortexxCorpse?.Position;
                if (!_atCorpse && DynelManager.LocalPlayer.Position.DistanceFrom(_vortexxCorpsePos) > 1)
                {
                    VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(_vortexxCorpsePos);

                    _atCorpse = true;
                }

                if (_atCorpse && !_initCorpse)
                {
                    // Check if it's time to disband
                    if (Time.NormalTime >= _timeToLeave)
                    {
                        Chat.WriteLine("Done, leaving");
                        _initCorpse = true; // Prevent this block from running again
                    }
                }

                if (_initCorpse)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._BeaconPos) > 1)
                        MovementController.Instance.SetDestination(Constants._BeaconPos);
                }
            }

            if (_vortexxCorpse == null)
            {

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._BeaconPos) > 1)
                {
                    MovementController.Instance.SetDestination(Constants._BeaconPos);
                }

                _initCorpse = true;
                _atCorpse = true;
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