using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AXPBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (AXPBuddy._settings["Toggle"].AsBool())
            {
                if (AXPBuddy.Ready)
                {
                    if (Team.IsRaid)
                    {
                        if (Playfield.ModelIdentity.Instance == Constants.APFHubId)
                        {
                            return new EnterSectorState();
                        }

                        if (Playfield.ModelIdentity.Instance == Constants.S13Id)
                        {
                            switch ((AXPBuddy.ModeSelection)AXPBuddy._settings["ModeSelection"].AsInt32())
                            {
                                case AXPBuddy.ModeSelection.Leech:
                                    if (AXPBuddy._settings["Merge"].AsBool() || (!Team.Members.Any(c => c.Character == null)))
                                    {
                                        return new LeechState();
                                    }
                                    break;

                                case AXPBuddy.ModeSelection.Path:
                                    if (AXPBuddy._settings["Merge"].AsBool() || (!Team.Members.Any(c => c.Character == null)))
                                    {
                                        return new PathState();
                                    }
                                    break;

                                default:
                                    if (AXPBuddy._settings["Merge"].AsBool() || (!Team.Members.Any(c => c.Character == null)))
                                    {
                                        return new PullState();
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        return new ReformState();
                    }
                }

                if (Playfield.ModelIdentity.Instance == Constants.UnicornHubId)
                    return new DiedState();

                if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                    return new SitState();
            }



            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Idle");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("IdleState::OnStateExit");
        }

        public void Tick()
        {

        }
    }
}
