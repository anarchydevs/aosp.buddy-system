using AOSharp.Common.GameData;
using AOSharp.Core;
using System.Collections.Generic;
using System.Linq;

namespace ALBBuddy
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
                return new EnterAlbtraumState();

            if (_phase == ReformPhase.Completed)
            {

                if (Team.Members.Where(c => c.Character != null).ToList().Count == _teamCache.Count())
                    return new EnterAlbtraumState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            _reformStartedTime = Time.NormalTime;

            ALBBuddy._passedStartPos = false;
            ALBBuddy._passedFirstPos = false;
            ALBBuddy._passedSecondPos = false;
            ALBBuddy._passedThirdPos = false;
            ALBBuddy._passedForthPos = false;
            ALBBuddy. _passedFifthPos = false;
            ALBBuddy._passedSixthPos = false;
            ALBBuddy._passedSeventhPos = false;
            ALBBuddy._passedEighthPos = false;
            ALBBuddy._passedNinethPos = false;
            ALBBuddy._passedTenthPos = false;
            ALBBuddy._passedLastPos = false;

            if (DynelManager.LocalPlayer.Identity != ALBBuddy.Leader)
            {
                Team.TeamRequest += OnTeamRequest;
                _phase = ReformPhase.Waiting;
            }
            else
            {
                _phase = ReformPhase.Disbanding;
            }
        }

        public void OnStateExit()
        {
            _invitedList.Clear();
            _teamCache.Clear();

            _init = false;

            if (DynelManager.LocalPlayer.Identity != ALBBuddy.Leader)
                Team.TeamRequest -= OnTeamRequest;
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Playfield.ModelIdentity.Instance == Constants.Inferno)
            {
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

                        if (player.Identity == ALBBuddy.Leader) { continue; }

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
