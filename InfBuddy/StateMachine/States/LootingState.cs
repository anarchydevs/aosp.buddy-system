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

        private static Corpse _corpse;

        private static Vector3 _corpsePos = Vector3.Zero;

        private double looting;

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId && _corpse == null)
                {
                    if (InfBuddy.ModeSelection.Normal == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                    {
                        Constants.DefendPos = new Vector3(165.6f, 2.2f, 186.4f);
                        return new DefendSpiritState();
                    }

                    if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                        return new RoamState();

                    if (InfBuddy.ModeSelection.Leech == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                        return new LeechState();
                }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Moving to corpse");
            looting = Time.NormalTime;
           
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Done looting");
            //_initCorpse = false;
        }

        public void Tick()
        {
           
            _corpse = DynelManager.Corpses
                .Where(c => c.Name.Contains("Remains of "))
                .FirstOrDefault();

            _corpsePos = (Vector3)_corpse?.Position;

            //Path to corpse
            if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpsePos) > 5f)
            InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)_corpse?.Position);
                    

            //if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpsePos) < 5f && Time.NormalTime > looting + 5f)
            //{
            //            _initCorpse = true;           
            //}


        }
    }
}