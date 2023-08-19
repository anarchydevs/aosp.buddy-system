﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using CityBuddy.IPCMessages;
using System;
using System.Linq;

namespace CityBuddy
{
    public class CityAttackState : IState
    {

        public static TeamMember selectedMember = null;

        private static Random rand = new Random();

        private SimpleChar _target;
        private static Corpse _corpse;
        private Dynel shipentrance;

        public IState GetNextState()
        {
            shipentrance = DynelManager.AllDynels.Where(c => c.Name == "Door").FirstOrDefault();

            //if (DynelManager.LocalPlayer.Identity == CityBuddy.Leader
            //    && Time.NormalTime > CityBuddy._cloakTime + 3660f
            //    && !DynelManager.NPCs.Any(c => c.Health > 0))
            //{
            //    return new CityControllerState();
            //}


            if (shipentrance != null)
            {
                if (Team.IsInTeam && selectedMember == null && DynelManager.LocalPlayer.Identity == CityBuddy.Leader
                && !Team.Members.Any(c => c.Character == null))
                {
                    int randomIndex = rand.Next(Team.Members.Count);
                    selectedMember = Team.Members[randomIndex];

                    if (selectedMember != null)
                    {
                        CityBuddy.IPCChannel.Broadcast(new SelectedMemberUpdateMessage()
                        { SelectedMemberIdentity = selectedMember.Identity });
                    }
                }

                if (selectedMember != null && DynelManager.LocalPlayer.Identity == selectedMember.Identity)
                {
                    return new EnterState();
                }

                if (Team.Members.Count(c => c.Character == null) > 1)
                {
                    return new EnterState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            if (Playfield.ModelIdentity.Instance == CityBuddy.MontroyalCity)
            {
                MovementController.Instance.SetDestination(CityBuddy._montroyalGaurdPos);
            }
            
            Chat.WriteLine("City attack state");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit city attack state");
        }

        public void Tick()
        {
            shipentrance = DynelManager.AllDynels.Where(c => c.Name == "Door").FirstOrDefault();

            _target = DynelManager.NPCs.Where(c => c.Health > 0&& c.DistanceFrom(DynelManager.LocalPlayer) < 30f)
                .OrderByDescending(c => c.Name.Contains("Hacker"))
                .FirstOrDefault();

            _corpse = DynelManager.Corpses.OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position)).FirstOrDefault();

            if (_target != null)
            {
                if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) < 10f)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttacking
                        && !DynelManager.LocalPlayer.IsAttackPending
                        && _target.IsInLineOfSight)
                    {
                        DynelManager.LocalPlayer.Attack(_target);
                    }
                }
            }

            if (Team.Members.Any(c => c.Character != null))
            {
                if (DynelManager.LocalPlayer.Identity == CityBuddy.Leader)
                {

                    if (_target != null)
                    {
                        if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 2f)
                        {
                            MovementController.Instance.SetDestination(_target.Position);
                        }
                    }
                    else if (_corpse != null && _target == null)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpse.Position) > 5)
                        {
                            MovementController.Instance.SetDestination(_corpse.Position);
                        }
                    }
                    else if (shipentrance == null)
                    {
                        if (Playfield.ModelIdentity.Instance == CityBuddy.MontroyalCity)
                        {
                            if (DynelManager.LocalPlayer.Position.Distance2DFrom(CityBuddy._montroyalGaurdPos) > 10)
                            { MovementController.Instance.SetDestination(CityBuddy._montroyalGaurdPos); }
                        }
                    }
                }
            }

            if (DynelManager.LocalPlayer.Identity != CityBuddy.Leader)
            {
                CityBuddy._leader = GetLeaderCharacter();

                if (CityBuddy._leader != null)
                    PathToLeader();
            }
        }

        private SimpleChar GetLeaderCharacter()
        {
            return Team.Members
                .Where(c => c.Character?.Health > 0 && c.Character?.IsValid == true && c.Identity == CityBuddy.Leader)
                .FirstOrDefault()?.Character;
        }

        private void PathToLeader()
        {
            CityBuddy._leaderPos = (Vector3)CityBuddy._leader?.Position;

            if (CityBuddy._leaderPos == Vector3.Zero
                || DynelManager.LocalPlayer.Position.DistanceFrom(CityBuddy._leaderPos) <= 1.6f
                || DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return;

            CityBuddy.NavMeshMovementController.SetNavMeshDestination(CityBuddy._leaderPos);
        }
    }
}
