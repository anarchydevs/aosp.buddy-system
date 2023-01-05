using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfBuddy
{
    public class ReformState : IState
    {
        private const float ReformTimeout = 100;
        private const float DisbandDelay = 22;

        private double _reformStartedTime;
        private double _inviting;

        private ReformPhase _phase;

        private static List<Identity> _teamCache = new List<Identity>();
        private static List<Identity> _invitedList = new List<Identity>();

        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (Extensions.TimedOut(_reformStartedTime, ReformTimeout))
                return new MoveToQuestGiverState();

            if (_phase == ReformPhase.Completed && Time.NormalTime > _reformStartedTime + DisbandDelay + 5f)
                if (Team.Members.Count >= _teamCache.Count())
                    return new MoveToQuestGiverState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("ReformState::OnStateEnter");

            _reformStartedTime = Time.NormalTime;

            if (InfBuddy._settings["Merge"].AsBool() || DynelManager.LocalPlayer.Identity != InfBuddy.Leader)
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

            if (InfBuddy._settings["Merge"].AsBool())
                Team.TeamRequest -= OnTeamRequest;
        }

        public void Tick()
        {
            if (Team.IsInTeam)
            {
                foreach (TeamMember member in Team.Members)
                {
                    if (!_teamCache.Contains(member.Identity))
                        _teamCache.Add(member.Identity);
                }
            }

            if (_phase == ReformPhase.Disbanding)
            {
                if (Team.IsInTeam && (Time.NormalTime > _reformStartedTime + DisbandDelay
                    || Time.NormalTime > _reformStartedTime + ReformTimeout))
                    Team.Disband();

                if (!Team.IsInTeam)
                {
                    _inviting = Time.NormalTime;
                    _phase = ReformPhase.Inviting;
                    Chat.WriteLine("ReformPhase.Inviting");
                }
            }

            if (_phase == ReformPhase.Inviting && _invitedList.Count() < _teamCache.Count() && Time.NormalTime > _inviting + 3f)
            {
                foreach (SimpleChar player in DynelManager.Players.Where(c => !_invitedList.Contains(c.Identity) && _teamCache.Contains(c.Identity)))
                {
                    if (_invitedList.Contains(player.Identity)) { continue; }

                    _invitedList.Add(player.Identity);

                    if (player.Identity == InfBuddy.Leader) { continue; }

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

                //Task.Factory.StartNew(
                //    async () =>
                //    {
                //        await Task.Delay(2000);
                //        _phase = ReformPhase.Completed;
                //        e.Accept();
                //    });
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
