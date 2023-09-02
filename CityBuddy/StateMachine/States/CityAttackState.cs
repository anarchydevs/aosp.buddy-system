using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using CityBuddy.IPCMessages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages;
using System;
using System.Linq;

namespace CityBuddy
{
    public class CityAttackState : IState
    {
        private SimpleChar _target;
        private Dynel shipentrance;

        public IState GetNextState()
        {
            Corpse _bossCorpse = DynelManager.Corpses
                    .Where(c => c.Name.Contains("General"))
               .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
               .FirstOrDefault();

            _target = DynelManager.NPCs.Where(c => c.Health > 0 && c.DistanceFrom(DynelManager.LocalPlayer) < 40f)
                   .OrderByDescending(c => c.Name.Contains("Hacker"))
                   .FirstOrDefault();

            if (!CityBuddy._settings["Enable"].AsBool())
                return new IdleState();

            if (DynelManager.LocalPlayer.Identity == CityBuddy.Leader
                && !DynelManager.NPCs.Any(c => c.Health > 0)
                && !CityBuddy.CityUnderAttack
                && (CityController.CloakState == CloakStatus.Unknown || CityController.CanToggleCloak()))
            {
                return new CityControllerState();
            }

            if (_bossCorpse != null) 
            {
                CityBuddy.CityUnderAttack = false;

                if (!CityBuddy.CityUnderAttack && _target == null)
                {
                    return new BossLootState();
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

            if (Playfield.ModelIdentity.Instance == CityBuddy.SerenityIslands)
            {
                MovementController.Instance.SetDestination(CityBuddy._serenityGaurdPos);
            }

            if (Playfield.ModelIdentity.Instance == CityBuddy.PlayadelDesierto)
            {
                MovementController.Instance.SetDestination(CityBuddy._playadelGaurdPos);
            }

            //Chat.WriteLine("City state");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("Exit city state");
        }

        public void Tick()
        {
            try
            {
                shipentrance = DynelManager.AllDynels.Where(c => c.Name == "Door").FirstOrDefault();

                _target = DynelManager.NPCs.Where(c => c.Health > 0 && c.DistanceFrom(DynelManager.LocalPlayer) < 40f)
                    .OrderByDescending(c => c.Name.Contains("Hacker"))
                    .FirstOrDefault();

                Corpse _corpse = DynelManager.Corpses
                     .Where(c => !c.Name.Contains("General"))
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

                if (DynelManager.LocalPlayer.Identity == CityBuddy.Leader)
                {
                    if (_target != null)
                    {
                        if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 5f)
                        {
                            MovementController.Instance.SetDestination(_target.Position);
                        }
                    }
                       
                    else if (_corpse != null && _target == null && CityBuddy._settings["Corpses"].AsBool())
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpse.Position) > 5f)
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
                        if (Playfield.ModelIdentity.Instance == CityBuddy.SerenityIslands)
                        {
                            if (DynelManager.LocalPlayer.Position.Distance2DFrom(CityBuddy._serenityGaurdPos) > 10)
                            { MovementController.Instance.SetDestination(CityBuddy._serenityGaurdPos); }
                        }
                        if (Playfield.ModelIdentity.Instance == CityBuddy.PlayadelDesierto)
                        {
                            if (DynelManager.LocalPlayer.Position.Distance2DFrom(CityBuddy._playadelGaurdPos) > 10)
                            { MovementController.Instance.SetDestination(CityBuddy._playadelGaurdPos); }
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

            MovementController.Instance.SetDestination(CityBuddy._leaderPos);
        }
    }
}
