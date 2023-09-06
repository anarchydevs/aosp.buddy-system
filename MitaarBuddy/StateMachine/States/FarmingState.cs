using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;

namespace MitaarBuddy
{
    public class FarmingState : IState
    {
        public static bool _initCorpse = false;
        public static bool _atCorpse = false;
        private static double _timeToLeave;

        private static Corpse _sinuhCorpse;

        public static Vector3 _sinuhCorpsePos = Vector3.Zero;

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20
                && !Team.Members.Any(c => c.Character == null))
            {
                return new ReformState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            _atCorpse = false;
            _initCorpse = false;
            Chat.WriteLine("Pause for looting, 30 sec");
            _timeToLeave = Time.NormalTime + 30; // Schedule leave 30 seconds from now
        }

        public void OnStateExit()
        {
            _atCorpse = false;
            _initCorpse = false;

            //Chat.WriteLine("Farming done");
        }

        public void Tick()
        {
            if (Playfield.ModelIdentity.Instance == Constants.MitaarId)
            {
                Dynel Device = DynelManager.AllDynels
               .Where(c => c.Name == "Strange Alien Device")
               .FirstOrDefault();

                _sinuhCorpse = DynelManager.Corpses
                             .Where(c => c.Name.Contains("Remains of Technomaster Sinuh"))
                                 .FirstOrDefault();

                

                if (_sinuhCorpse != null)
                {
                    _sinuhCorpsePos = (Vector3)_sinuhCorpse?.Position;

                    //Path to corpse
                    if (!_atCorpse && DynelManager.LocalPlayer.Position.DistanceFrom(_sinuhCorpsePos) > 1.0f)
                    {
                        MovementController.Instance.SetDestination(_sinuhCorpsePos);

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

                    if ( _initCorpse )
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._strangeAlienDevice) > 1)
                            MovementController.Instance.SetDestination(Constants._strangeAlienDevice);
                    }
                }

                if (_sinuhCorpse == null)
                {

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._strangeAlienDevice) > 1)
                    {
                        MovementController.Instance.SetDestination(Constants._strangeAlienDevice);
                    }

                    _initCorpse = true;
                    _atCorpse = true;
                }

                if (Device != null && _initCorpse && _atCorpse)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._strangeAlienDevice) < 1
                    && Extensions.CanProceed())
                    {
                        Device.Use();
                    }
                }
            }
        }
    }
}