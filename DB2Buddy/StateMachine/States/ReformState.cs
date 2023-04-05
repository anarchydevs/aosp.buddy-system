using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using DB2Buddy.IPCMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class ReformState : IState
    {
        private const float ReformTimeout = 70;
        private const float DisbandDelay = 7;
        private const float InviteDelay = 11;

        private double _reformStartedTime;
        private double _inviting;

        private ReformPhase _phase;

        private static List<Identity> _teamCache = new List<Identity>();
        private static List<Identity> _invitedList = new List<Identity>();
        private bool _init = false;

        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            if (_phase == ReformPhase.Completed)
            {
                return new IdleState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("ReformState::OnStateEnter");

            _reformStartedTime = Time.NormalTime;

            if (!Team.IsLeader)
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

            if (!Team.IsLeader)
                Team.TeamRequest -= OnTeamRequest;
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (!Team.IsLeader) { return; }

            if (_phase == ReformPhase.Inviting
                && _invitedList.Count() < _teamCache.Count()
                && Time.NormalTime > _inviting + DisbandDelay
                && !_init)
            {
                _init = true;

                DynelManager.LocalPlayer.Position = new Vector3(2124.7f, 0.6f, 2769.3f);
                DB2Buddy.NavMeshMovementController.SetMovement(MovementAction.Update);

                foreach (SimpleChar player in DynelManager.Players.Where(c => c.IsInPlay && !_invitedList.Contains(c.Identity) && _teamCache.Contains(c.Identity)))
                {
                    if (_invitedList.Contains(player.Identity)) { continue; }

                    _invitedList.Add(player.Identity);

                    if (player.Identity == DB2Buddy.Leader) { continue; }

                    Team.Invite(player.Identity);
                    Chat.WriteLine($"Inviting {player.Name}");
                }
            }

            if (_phase == ReformPhase.Disbanding)
            {
                if (!Team.IsInTeam && Playfield.ModelIdentity.Instance == 570)
                {
                    _inviting = Time.NormalTime;
                    _phase = ReformPhase.Inviting;
                    Chat.WriteLine("ReformPhase.Inviting");
                }
                else
                {
                    if (Team.IsInTeam)
                    {
                        foreach (TeamMember member in Team.Members)
                            if (!_teamCache.Contains(member.Identity))
                                _teamCache.Add(member.Identity);
                    }

                    Team.Disband();
                }
            }

            if (_phase == ReformPhase.Inviting
                && _invitedList.Count() == _teamCache.Count()
                && Time.NormalTime > _inviting + DisbandDelay
                && Team.Members.Where(c => c.Character != null).ToList().Count == _teamCache.Count()
                && _init)
            {
                _phase = ReformPhase.Completed;
                DB2Buddy.IPCChannel.Broadcast(new EnterMessage());
                Chat.WriteLine("ReformPhase.Completed");
            }
        }

        private void OnTeamRequest(object s, TeamRequestEventArgs e)
        {
            if (_teamCache.Contains(e.Requester))
            {
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
