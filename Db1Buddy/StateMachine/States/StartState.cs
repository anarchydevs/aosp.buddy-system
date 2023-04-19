using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System.Linq;
using System.Threading;


namespace Db1Buddy
{
    public class StartState : IState
    {
        private static SimpleChar _maskedCommando;

        public IState GetNextState()
        {
            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._startPosition) < 10f)
                return new GetBuffState();

            return null;
        }

        public void OnStateEnter()
        {
           Chat.WriteLine("StartState");
        }

        public void OnStateExit()
        {

                
            
            Chat.WriteLine("Exit StartState");
        }

        public void Tick()
        {
            if (!Team.IsInTeam || Game.IsZoning) { return; }

            foreach (TeamMember member in Team.Members)
            {
                if (!ReformState._teamCache.Contains(member.Identity))
                    ReformState._teamCache.Add(member.Identity);
            }

            _maskedCommando = DynelManager.NPCs
               .Where(c => c.Health > 0
                       && c.Name.Contains("Masked Commando")
                       && !c.Name.Contains("Remains of Masked Commando"))
                   .FirstOrDefault();

            if (Playfield.ModelIdentity.Instance == 6003 )
            {
               

                if (_maskedCommando != null && !Team.Members.Any(c => c.Character == null))
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                        DynelManager.LocalPlayer.Attack(_maskedCommando); 
                }

                
                    if (_maskedCommando == null && !MovementController.Instance.IsNavigating 
                    && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._startPosition) > 5f)
                        Db1Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPosition);
                


                //if (DynelManager.LocalPlayer.Identity != Db1Buddy.Leader)
                //{

                //    Db1Buddy._leader = Team.Members
                //           .Where(c => c.Character?.Health > 0
                //               && c.Character?.IsValid == true
                //               && c.IsLeader)
                //           .FirstOrDefault()?.Character;


                //    if (Db1Buddy._leader != null)
                //    {

                //        Db1Buddy._leaderPos = (Vector3)Db1Buddy._leader?.Position;

                //        if (DynelManager.LocalPlayer.Position.DistanceFrom(Db1Buddy._leaderPos) > 1f)
                //            Db1Buddy.NavMeshMovementController.SetNavMeshDestination(Db1Buddy._leaderPos);
                //    }
                //}
            }
        }
    }
}