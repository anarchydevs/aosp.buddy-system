using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;
using System.Threading.Tasks;

namespace InfBuddy
{
    public class LootingState : IState
    {
        private static bool _initCorpse = false;
        public static bool _missionsLoaded = false;

        private static Corpse _corpse;

        private static Vector3 _corpsePos = Vector3.Zero;

        private double looting;

        public IState GetNextState()
        {
            if (_corpse == null || _initCorpse)
                return new IdleState();

            if (Extensions.CanExit(_missionsLoaded))
                return new ExitMissionState();

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("Moving to corpse");
            looting = Time.NormalTime;

        }

        public void OnStateExit()
        {
            //Chat.WriteLine("Done looting");
            _initCorpse = false;
            _missionsLoaded = false;
        }

        public void Tick()
        {

            _corpse = DynelManager.Corpses
                .Where(c => c.Name.Contains("Remains of "))
                .FirstOrDefault();

            _corpsePos = (Vector3)_corpse?.Position;

            if (!_missionsLoaded && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                _missionsLoaded = true;

            if (_corpse != null)//Path to corpse
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpsePos) > 5f)
                    InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)_corpse?.Position);


                if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpsePos) < 5f && Time.NormalTime > looting + 2f)
                {
                    _initCorpse = true;
                }
            }

        }
    }
}