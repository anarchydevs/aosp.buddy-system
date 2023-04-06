using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace MitaarBuddy
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
                return new EnterState();

            if (_phase == ReformPhase.Completed)
            {
                //if (!Team.IsRaid && Team.IsLeader
                //    && !_init)
                //{
                //    _init = true;
                //    Team.ConvertToRaid();
                //}

                if (Team.Members.Where(c => c.Character != null).ToList().Count == _teamCache.Count())
                    return new EnterState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Reforming");

            _reformStartedTime = Time.NormalTime;

            MovementController.Instance.SetDestination(Constants._reneterPos);

            if (DynelManager.LocalPlayer.Identity != MitaarBuddy.Leader)
            {
                Team.TeamRequest += OnTeamRequest;
                _phase = ReformPhase.Waiting;
            }

            MitaarBuddy._initCorpse = false;
        }

        public void OnStateExit()
        {

            _invitedList.Clear();
            _teamCache.Clear();

            _init = false;

            MitaarBuddy._initCorpse = false;

        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (_phase == ReformPhase.Disbanding && Time.NormalTime > _reformStartedTime + DisbandDelay)
            {
                _phase = ReformPhase.Inviting;

            }

            if (_phase == ReformPhase.Inviting && _invitedList.Count() < _teamCache.Count())
            {
                foreach (SimpleChar player in DynelManager.Players.Where(c => c.IsInPlay && !_invitedList.Contains(c.Identity) && _teamCache.Contains(c.Identity)))
                {
                    if (_invitedList.Contains(player.Identity)) { continue; }

                    _invitedList.Add(player.Identity);

                    if (player.Identity == MitaarBuddy.Leader) { continue; }

                    Team.Invite(player.Identity);

                }
            }

            if (_phase == ReformPhase.Inviting
                && Team.IsInTeam
                && Team.Members.Where(c => c.Character != null).ToList().Count == _teamCache.Count()
                && _invitedList.Count() == _teamCache.Count())
            {
                _phase = ReformPhase.Completed;

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
