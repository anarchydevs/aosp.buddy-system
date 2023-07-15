using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class FarmingState : IState
    {
        public static bool _initCorpse = false;

        private Corpse _auneCorpse;

        public Vector3 _auneCorpsePos = Vector3.Zero;

        public IState GetNextState()
        {
            if (!DB2Buddy._settings["Toggle"].AsBool())
                DB2Buddy.NavMeshMovementController.Halt();

            if (Playfield.ModelIdentity.Instance == Constants.PWId)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 30f)
                    return new ReformState();
            }

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.first) < 60)
                    return new FellState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("FarmingState");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit FarmingState");
        }

        public void Tick()
        {
            _auneCorpse = DynelManager.Corpses
                         .Where(c => c.Name.Contains("Remains of Ground Chief Aune"))
                             .FirstOrDefault();

            foreach (TeamMember member in Team.Members)
            {
                if (!ReformState._teamCache.Contains(member.Identity))
                    ReformState._teamCache.Add(member.Identity);
            }

            if (_auneCorpse != null && !DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Stun)
                && DynelManager.LocalPlayer.Position.DistanceFrom(_auneCorpsePos) > 1.0f
                && !MovementController.Instance.IsNavigating)
            {
                _auneCorpsePos = (Vector3)_auneCorpse?.Position;
                DB2Buddy.NavMeshMovementController.SetNavMeshDestination(_auneCorpsePos);
            }

            if (!_initCorpse && Team.IsInTeam && Playfield.ModelIdentity.Instance == Constants.DB2Id
                && !MovementController.Instance.IsNavigating
                && DynelManager.LocalPlayer.Position.DistanceFrom(_auneCorpsePos) < 1.0f)
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