using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AXPBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f) { return null; }

            if (AXPBuddy._settings["Toggle"].AsBool() && Team.IsInTeam && Team.IsRaid)
            {
                if (Playfield.ModelIdentity.Instance == Constants.APFHubId)
                {
                    return new EnterSectorState();
                }
                if (Playfield.ModelIdentity.Instance == Constants.S13Id)
                {
                    if (AXPBuddy.ModeSelection.Leech == (AXPBuddy.ModeSelection)AXPBuddy._settings["ModeSelection"].AsInt32())
                        //if (AXPBuddy._died || AXPBuddy._settings["Merge"].AsBool())
                            return new LeechState();

                    if (AXPBuddy.ModeSelection.Roam == (AXPBuddy.ModeSelection)AXPBuddy._settings["ModeSelection"].AsInt32())
                        //if (AXPBuddy._died || AXPBuddy._settings["Merge"].AsBool() )
                            return new RoamState();

                    if (AXPBuddy.ModeSelection.Gather == (AXPBuddy.ModeSelection)AXPBuddy._settings["ModeSelection"].AsInt32())
                        //if (AXPBuddy._died || AXPBuddy._settings["Merge"].AsBool() )
                            return new GatherState();

                    //if (AXPBuddy._died || AXPBuddy._settings["Merge"].AsBool() )
                    //    return new PatrolState();
                }

            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("IdleState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("IdleState::OnStateExit");
        }

        public void Tick()
        {
            if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f) { return; }
        }
    }
}
