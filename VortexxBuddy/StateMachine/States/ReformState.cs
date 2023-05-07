using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace VortexxBuddy
{
    public class ReformState : IState
    {
        private const float ReformTimeout = 100;
        private const float DisbandDelay = 10;
        private const float InviteDelay = 13;

        private static double _reformStartedTime;
        private double _inviting;

        private ReformPhase _phase;

        public static List<Identity> _teamCache = new List<Identity>();
        private static List<Identity> _invitedList = new List<Identity>();

        private static bool _init = false;

        public IState GetNextState()
        {
            if (Extensions.TimedOut(_reformStartedTime, ReformTimeout))
                return new IdleState();

            if (_phase == ReformPhase.Completed)
            {
                if (Team.Members.Where(c => c.Character != null).ToList().Count == _teamCache.Count())
                    return new IdleState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("Reforming");

            _reformStartedTime = Time.NormalTime;

            VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPos);

            if (DynelManager.LocalPlayer.Identity != VortexxBuddy.Leader)
            {
                Team.TeamRequest += OnTeamRequest;
                _phase = ReformPhase.Waiting;
            }

            VortexxBuddy._clearToEnter = false;
            FarmingState._initCorpse = false;
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("Done Reforming");
            _invitedList.Clear();
            _teamCache.Clear();

            _init = false;

            FarmingState._initCorpse = false;

        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Team.IsInTeam)
            {
                foreach (TeamMember member in Team.Members)
                {
                    if (!_teamCache.Contains(member.Identity))
                        _teamCache.Add(member.Identity);
                }
            }

            if (_phase == ReformPhase.Inviting
                && _invitedList.Count() < _teamCache.Count()
                && Time.NormalTime > _inviting + DisbandDelay
                && !_init)
            {
                _init = true;

                foreach (SimpleChar player in DynelManager.Players.Where(c => c.IsInPlay && !_invitedList.Contains(c.Identity) && _teamCache.Contains(c.Identity)))
                {
                    if (_invitedList.Contains(player.Identity)) { continue; }

                    _invitedList.Add(player.Identity);

                    if (player.Identity == VortexxBuddy.Leader) { continue; }

                    Team.Invite(player.Identity);
                   // Chat.WriteLine($"Inviting {player.Name}");
                }
            }

            if (_phase == ReformPhase.Disbanding)
            {
                if (!Team.IsInTeam)
                {
                    _inviting = Time.NormalTime;
                    _phase = ReformPhase.Inviting;
                    //Chat.WriteLine("ReformPhase.Inviting");
                }
                else if (Team.Members.Where(c => c.Character != null && c.Character.IsInPlay).ToList().Count == _teamCache.Count()
                        || Time.NormalTime > _reformStartedTime + ReformTimeout)
                    Team.Disband();
            }

            if (_phase == ReformPhase.Inviting
                && _invitedList.Count() == _teamCache.Count()
                && Time.NormalTime > _inviting + DisbandDelay
                && Team.Members.Where(c => c.Character != null).ToList().Count == _teamCache.Count()
                && _init)
            {
                _phase = ReformPhase.Completed;
                //Chat.WriteLine("ReformPhase.Completed");
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
