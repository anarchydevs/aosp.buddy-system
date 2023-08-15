using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AXPBuddy
{
    public class EnterSectorState : IState
    {
        private const int MinWait = 3;
        private const int MaxWait = 7;

        public IState GetNextState()
        {
            if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f) { return null; }

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

            if (Playfield.ModelIdentity.Instance == Constants.UnicornHubId)
                return new DiedState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("EnterSectorState::OnStateEnter");

            if (DynelManager.LocalPlayer.Identity == AXPBuddy.Leader)
            {
                Task.Factory.StartNew(
                   async () =>
                   {
                       await Task.Delay(2000);
                       AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13EntrancePos);
                   });
            }
            else
            {
                int randomWait = Extensions.Next(MinWait, MaxWait);
                Chat.WriteLine($"Idling for {randomWait} seconds..");

                Task.Factory.StartNew(
                   async () =>
                   {
                       await Task.Delay(randomWait * 1000);
                       AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13EntrancePos);
                   });
            }
        }

        public void OnStateExit()
        {
            Chat.WriteLine("EnterSectorState::OnStateExit");
        }

        public void Tick()
        {
            if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f) { return; }
        }
    }
}