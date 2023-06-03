using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MitaarBuddy
{
    public class FarmingState : IState
    {
        public static bool _initCorpse = false;
        public static bool _atCorpse = false;

        private static Corpse _sinuhCorpse;

        public static Vector3 _sinuhCorpsePos = Vector3.Zero;

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20)
            {
                return new ReformState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            _atCorpse = false;
            _initCorpse = false;
            Chat.WriteLine("Farming");
        }

        public void OnStateExit()
        {
            _atCorpse = false;
            _initCorpse = false;

            Chat.WriteLine("Farming done");
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

                _sinuhCorpsePos = (Vector3)_sinuhCorpse?.Position;

                if (_sinuhCorpse != null)
                { //Path to corpse
                    if (!_atCorpse && DynelManager.LocalPlayer.Position.DistanceFrom(_sinuhCorpsePos) > 1.0f)
                    {
                        MovementController.Instance.SetDestination(_sinuhCorpsePos);

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

                                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._strangeAlienDevice) > 1)
                                    MovementController.Instance.SetDestination(Constants._strangeAlienDevice);

                            });

                        _initCorpse = true;
                    }
                }

                if (_sinuhCorpse == null)
                {
                    Chat.WriteLine("Pause for looting, 10 sec");
                    Task.Factory.StartNew(
                        async () =>
                        {
                            await Task.Delay(10000);
                            Chat.WriteLine("Done, Leaving");

                            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._strangeAlienDevice) > 1)
                                MovementController.Instance.SetDestination(Constants._strangeAlienDevice);

                        });

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