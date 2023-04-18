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
        public static bool _initCorpse = false;

        public static Corpse _corpse;

        public static Vector3 _corpsePos = Vector3.Zero;

        private double looting;

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
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
        }

        public void Tick()
        {
            _corpse = DynelManager.Corpses
                .FirstOrDefault();

            _corpsePos = (Vector3)_corpse?.Position;

            //Path to corpse
            if (_corpse != null && DynelManager.LocalPlayer.Position.DistanceFrom(_corpsePos) > 3f)
            {
                Task.Factory.StartNew(
                    async () =>
                    {
                        InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)_corpse?.Position);
                        await Task.Delay(2000);
                    });
               
            }

            if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpsePos) < 5f)
            {
                Task.Factory.StartNew(
                    async () =>
                    {
                        await Task.Delay(2000);
                    });
            }


        }
    }
}