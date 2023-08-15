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
        // Timeout for the reform process
        private const float ReformTimeout = 70;
        // Delay before disbanding the team
        private const float DisbandDelay = 5;

        private double _reformStartedTime;
        private ReformPhase _phase;

        private bool _init = false;
        public static HashSet<Identity> _teamCache = new HashSet<Identity>(); // Store the team members' identities
        public static HashSet<Identity> _invitedList = new HashSet<Identity>(); // Store the identities of players who have been invited

        public IState GetNextState()
        {
            // If the player is in the Unicorn Hub, return the DiedState
            if (Playfield.ModelIdentity.Instance == Constants.UnicornHubId)
                return new DiedState();

            // Check if the reform process has timed out, return EnterSectorState to start over
            if (Extensions.TimedOut(_reformStartedTime, ReformTimeout))
            {
                return new EnterSectorState();
            }

            // Check if the reform process is completed
            if (_phase == ReformPhase.Completed)
            {
                // If the team is not a raid and the local player is the leader and not already initialized
                // convert the team to a raid
                if (!Team.IsRaid && DynelManager.LocalPlayer.Identity == AXPBuddy.Leader && !_init)
                {
                    _init = true;
                    Team.ConvertToRaid();
                }

                // If the number of team members with valid characters is equal to the cached team members' count,
                // return EnterSectorState to proceed to the next state
                if (Team.Members.Count(c => c.Character != null) == _teamCache.Count)
                {
                    return new EnterSectorState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            // Called when entering the state
            Chat.WriteLine("ReformState::OnStateEnter");

            _reformStartedTime = Time.NormalTime;

            if (AXPBuddy._settings["Merge"].AsBool() || DynelManager.LocalPlayer.Identity != AXPBuddy.Leader)
            {
                // If merge setting is enabled or the local player is not the leader, subscribe to the TeamRequest event
                Team.TeamRequest += OnTeamRequest;
                _phase = ReformPhase.Waiting;
                //Chat.WriteLine("ReformPhase.Waiting");
            }
            else
            {
                // If the local player is the leader and merge setting is disabled, disband the team
                _phase = ReformPhase.Disbanding;
                //Chat.WriteLine("ReformPhase.Disbanding");
            }
        }

        public void OnStateExit()
        {
            // Called when exiting the state
            Chat.WriteLine("ReformState::OnStateExit");

            _invitedList.Clear();
            _teamCache.Clear();

            _init = false;

            if (AXPBuddy._settings["Merge"].AsBool() || DynelManager.LocalPlayer.Identity != AXPBuddy.Leader)
            {
                // If merge setting is enabled or the local player is not the leader, unsubscribe from the TeamRequest event
                Team.TeamRequest -= OnTeamRequest;
            }
        }

        public void Tick()
        {
            // Called on every tick
            if (Game.IsZoning)
            {
                return;
            }

            if (_phase == ReformPhase.Disbanding && Time.NormalTime > _reformStartedTime + DisbandDelay)
            {
                // If the disband delay has passed, start inviting players
                _phase = ReformPhase.Inviting;
                //Chat.WriteLine("ReformPhase.Inviting");
            }

            if (_phase == ReformPhase.Inviting && _invitedList.Count < _teamCache.Count)
            {
                // If still inviting players and some players are not invited yet, invite them
                var playersToInvite = _teamCache.Except(_invitedList);
                foreach (SimpleChar player in DynelManager.Players.Where(c => c.IsInPlay && playersToInvite.Contains(c.Identity)))
                {
                    _invitedList.Add(player.Identity);

                    if (player.Identity == AXPBuddy.Leader)
                    {
                        continue;
                    }

                    Team.Invite(player.Identity);
                    //Chat.WriteLine($"Inviting {player.Name}");
                }
            }

            if (_phase == ReformPhase.Inviting && Team.IsInTeam && Team.Members.Count(c => c.Character != null) == _teamCache.Count && _invitedList.Count == _teamCache.Count)
            {
                // If all team members are invited and the team is formed, mark the reform process as completed
                _phase = ReformPhase.Completed;
                //Chat.WriteLine("ReformPhase.Completed");
            }
        }

        private void OnTeamRequest(object s, TeamRequestEventArgs e)
        {
            // Event handler for TeamRequest event
            // If the requester is in the cached team members, accept the request and mark the reform process as completed
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
