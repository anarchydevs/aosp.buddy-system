using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class IdleState : IState
    {
        private static SimpleChar _aune;
        private static Corpse _auneCorpse;
        private static SimpleChar _redTower;
        private static SimpleChar _blueTower;

        public IState GetNextState()
        {
            _aune = DynelManager.NPCs
              .Where(c => c.Health > 0
                  && c.Name.Contains("Ground Chief Aune")
                  && !c.Name.Contains("Remains of "))
              .FirstOrDefault();

            _auneCorpse = DynelManager.Corpses
               .Where(c => c.Name.Contains("Remains of Ground Chief Aune"))
               .FirstOrDefault();

            _redTower = DynelManager.NPCs
             .Where(c => c.Health > 0
                 && c.Name.Contains("Strange Xan Artifact")
                 && !c.Name.Contains("Remains of ")
                 && c.Buffs.Contains(274119))
             .FirstOrDefault();

            _blueTower = DynelManager.NPCs
               .Where(c => c.Health > 0
                   && c.Name.Contains("Strange Xan Artifact")
                   && !c.Name.Contains("Remains of ")
                   && !c.Buffs.Contains(274119))
               .FirstOrDefault();

            if (!DB2Buddy._settings["Toggle"].AsBool())
            {
                DB2Buddy.NavMeshMovementController.Halt();
            }

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return new SitState();

            if (DB2Buddy._settings["Toggle"].AsBool())

            {
                if (Playfield.ModelIdentity.Instance == Constants.PWId)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 30f
                       && !Team.IsInTeam)
                        return new ReformState();

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 30f
                        && Team.IsInTeam
                        && Extensions.CanProceed())
                        return new EnterState();
                }

                if (Playfield.ModelIdentity.Instance == Constants.DB2Id)
                {
                    if (_auneCorpse != null
                            && Extensions.CanProceed()
                            && DB2Buddy._settings["Farming"].AsBool())
                        return new FarmingState();

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._atDoor) < 10f
                     && Team.IsInTeam)
                        return new PathToBossState();

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPosition) < 10f
                         && Team.IsInTeam)
                        return new FightState();

                    
                }
            }
        
            return null;
        }

    public void OnStateEnter()
    {
        Chat.WriteLine("IdleState");
    }

    public void OnStateExit()
    {
        Chat.WriteLine("Exit IdleState");
    }

    public void Tick()
    {

    }
}
}
