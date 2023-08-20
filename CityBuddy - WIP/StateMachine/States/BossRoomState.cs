using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CityBuddy
{
    public class BossRoomState : IState
    {
        private Dynel _downButton;
        private Dynel _exitDevice;

        private SimpleChar _target;
        private static Corpse _corpse;

        public IState GetNextState()
        {
            _exitDevice = DynelManager.AllDynels.FirstOrDefault(c => c.Name == "Exit to ICC");

            _target = DynelManager.NPCs
                .Where(c => c.Health > 0 && !CityBuddy._ignores.Contains(c.Name))
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .ThenBy(c => c.HealthPercent)
                .FirstOrDefault();

            _corpse = DynelManager.Corpses.OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position)).FirstOrDefault();

            if (!CityBuddy._settings["Toggle"].AsBool() || !Playfield.IsDungeon
                || DynelManager.LocalPlayer.Room.Name != "AI_bossroom")
                return new IdleState();

            if (_exitDevice != null && _corpse == null && _target == null)
            {
                return new ButtonExitState();
            }

            return null;
        }

        public void OnStateEnter()
        {

            if (_downButton != null && !ButtonExitState._downButtonLocation.Contains(_downButton.Position))
            {
                ButtonExitState._downButtonLocation.Add(_downButton.Position);
                Chat.WriteLine("Added down button location");
            }

            if (DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.StopAttack();

            Chat.WriteLine("Boss room state");
        }

        public void OnStateExit()
        {
            CityBuddy.NavMeshMovementController.Halt();

            if (DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.StopAttack();

            Chat.WriteLine("Exit Boss boom state");
        }

        public void Tick()
        {
            try
            {
                if (Game.IsZoning || !Team.IsInTeam) { return; }

                _exitDevice = DynelManager.AllDynels.FirstOrDefault(c => c.Name == "Exit Device");

                _target = DynelManager.NPCs
               .Where(c => c.Health > 0 && !CityBuddy._ignores.Contains(c.Name) && c.IsInLineOfSight)
               .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
               .ThenBy(c => c.HealthPercent)
               .FirstOrDefault();

                _corpse = DynelManager.Corpses
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .FirstOrDefault();


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
                                CityBuddy.NavMeshMovementController.SetNavMeshDestination(_target.Position);
                            }

                            else if (_target == null && _corpse != null)
                            {
                                if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpse.Position) > 2f)
                                {
                                    CityBuddy.NavMeshMovementController.SetNavMeshDestination(_corpse.Position);
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
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + CityBuddy.GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != CityBuddy.previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    CityBuddy.previousErrorMessage = errorMessage;
                }
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
