using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DB2Buddy
{
    public class ReformState : IState
    {
        private const float ReformTimeout = 5;
        private const float DisbandDelay = 5;

        private static double _reformStartedTime;

        private ReformPhase _phase;

        //private static bool _init = false;

        public static List<Identity> _teamCache = new List<Identity>();
        private static List<Identity> _invitedList = new List<Identity>();

        private string previousErrorMessage = string.Empty;

        public IState GetNextState()
        {
            if (Extensions.TimedOut(_reformStartedTime, ReformTimeout))
                return new EnterState();

            if (_phase == ReformPhase.Completed)
            {
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

            if (DynelManager.LocalPlayer.Identity != DB2Buddy.Leader)
            {
                Team.TeamRequest += OnTeamRequest;
                _phase = ReformPhase.Waiting;
            }

            FarmingState._initCorpse = false;
        }

        public void OnStateExit()
        {

            Chat.WriteLine("Done Reforming");
            _invitedList.Clear();
            _teamCache.Clear();

            //_init = false;
            FarmingState._initCorpse = false;

        }

        public void Tick()
        {
            try
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

                        if (player.Identity == DB2Buddy.Leader) { continue; }

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
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    previousErrorMessage = errorMessage;
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

        private int GetLineNumber(Exception ex)
        {
            var lineNumber = 0;

            var lineMatch = Regex.Match(ex.StackTrace ?? "", @":line (\d+)$", RegexOptions.Multiline);

            if (lineMatch.Success)
                lineNumber = int.Parse(lineMatch.Groups[1].Value);

            return lineNumber;
        }
    }
}
