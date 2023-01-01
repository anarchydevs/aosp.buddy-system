using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AXPBuddy
{
    public class ReformState : IState
    {
        private const float ReformTimeout = 100;
        private const float DisbandDelay = 10;

        private static double _reformStartedTime;

        private ReformPhase _phase;

        private static bool _init = false;

        public static List<Identity> _teamCache = new List<Identity>();
        private static List<Identity> _invitedList = new List<Identity>();

        public IState GetNextState()
        {
            if (Extensions.TimedOut(_reformStartedTime, ReformTimeout))
                return new EnterSectorState();

            if (_phase == ReformPhase.Completed)
            {
                if (!Team.IsRaid && Team.IsLeader
                    && !_init)
                {
                    _init = true;
                    Team.ConvertToRaid();
                }

                if (Team.Members.Count >= _teamCache.Count())
                    return new EnterSectorState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("ReformState::OnStateEnter");

            _reformStartedTime = Time.NormalTime;
            AXPBuddy._passedCorrectionPos = false;

            if (AXPBuddy._settings["Merge"].AsBool() || DynelManager.LocalPlayer.Identity != AXPBuddy.Leader)
            {
                Team.TeamRequest += OnTeamRequest;
                _phase = ReformPhase.Waiting;
                Chat.WriteLine("ReformPhase.Waiting");
            }
            else
            {
                _phase = ReformPhase.Disbanding;
                Chat.WriteLine("ReformPhase.Disbanding");
            }
        }

        public void OnStateExit()
        {
            Chat.WriteLine("ReformState::OnStateExit");

            _invitedList.Clear();
            _teamCache.Clear();

            _init = false;

            if (AXPBuddy._settings["Merge"].AsBool())
                Team.TeamRequest -= OnTeamRequest;
        }

        public void Tick()
        {
            if (_phase == ReformPhase.Disbanding && Time.NormalTime > _reformStartedTime + DisbandDelay)
            {
                _phase = ReformPhase.Inviting;
                Chat.WriteLine("ReformPhase.Inviting");
            }

            if (_phase == ReformPhase.Inviting && _invitedList.Count() < _teamCache.Count())
            {
                foreach (SimpleChar player in DynelManager.Players.Where(c => !_invitedList.Contains(c.Identity) && _teamCache.Contains(c.Identity)))
                {
                    //if (_invitedList.Contains(player.Identity))
                    //    continue;

                    //Team.Invite(player.Identity);

                    //_invitedList.Add(player.Identity);
                    //Chat.WriteLine($"Inviting {player.Name}");

                    if (_invitedList.Contains(player.Identity)) { continue; }

                    _invitedList.Add(player.Identity);

                    if (player.Identity == AXPBuddy.Leader) { continue; }

                    Team.Invite(player.Identity);
                    Chat.WriteLine($"Inviting {player.Name}");
                }
            }

            if (_phase == ReformPhase.Inviting && Team.IsInTeam && _invitedList.Count() >= _teamCache.Count())
            {
                _phase = ReformPhase.Completed;
                Chat.WriteLine("ReformPhase.Completed");
            }
        }

        private void OnTeamRequest(object s, TeamRequestEventArgs e)
        {
            if (_teamCache.Contains(e.Requester))
            {
                _phase = ReformPhase.Completed;
                e.Accept();
            }
        }

        private enum ReformPhase
        {
            Disbanding,
            Inviting,
            Waiting,
            Completed
        }
    }
}
