using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace AXPBuddy
{
    public class EnterSectorState : IState
    {
        private Stopwatch _leaderTimer = new Stopwatch();
        private Stopwatch _followerTimer = new Stopwatch();
        private int _followerRandomWait = 0;

        private const int MinWait = 3;
        private const int MaxWait = 7;

        public IState GetNextState()
        {
            //if (Game.IsZoning || Time.NormalTime < AXPBuddy._lastZonedTime + 2f) { return null; }

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
            Chat.WriteLine("Enter S13");

        }

        public void OnStateExit()
        {
            //Chat.WriteLine("EnterSectorState::OnStateExit");
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (DynelManager.LocalPlayer.Identity == AXPBuddy.Leader)
            {
                if (!_leaderTimer.IsRunning)
                {
                    _leaderTimer.Start();
                }

                if (_leaderTimer.ElapsedMilliseconds >= 2000)
                {
                    _leaderTimer.Reset();
                    AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13EntrancePos);
                }
            }
            else
            {
                if (!_followerTimer.IsRunning)
                {
                    _followerRandomWait = Extensions.Next(MinWait, MaxWait);
                    Chat.WriteLine($"Idling for {_followerRandomWait} seconds..");
                    _followerTimer.Start();
                }

                if (_followerTimer.ElapsedMilliseconds >= _followerRandomWait * 1000)
                {
                    _followerTimer.Reset();
                    AXPBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13EntrancePos);
                }
            }
        }
    }
}