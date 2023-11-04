using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CityBuddy
{
    public class CityAttackState : IState
    {
        private SimpleChar _target;
        private Dynel shipentrance;
        private bool _atStart = false;



        public IState GetNextState()
        {
            var validPlayfields = new[]
                {
                    CityBuddy.MontroyalCity,
                    CityBuddy.SerenityIslands,
                    CityBuddy.PlayadelDesierto
                };

            Corpse _bossCorpse = DynelManager.Corpses
                    .Where(c => c.Name.Contains("General"))
               .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
               .FirstOrDefault();

            _target = DynelManager.NPCs.Where(c => c.Health > 0 && c.DistanceFrom(DynelManager.LocalPlayer) < 40f)
                   .OrderByDescending(c => c.Name.Contains("Hacker"))
                   .FirstOrDefault();

            if (!CityBuddy._settings["Enable"].AsBool())
                return new IdleState();

            if (validPlayfields.Contains(Playfield.ModelIdentity.Instance))
            {
                if (_bossCorpse != null)
                {
                    CityBuddy.CityUnderAttack = false;

                    // Check if the corpse is new (not in the dictionary)
                    if (!CityBuddy.CityUnderAttack && _target == null && !BossLootState.bossCorpseDictionary.ContainsKey(_bossCorpse.Position))
                    {
                        return new BossLootState();
                    }
                }
                else if (shipentrance != null && CityBuddy._settings["Ship"].AsBool())
                {
                    return new WaitForShipState();
                }
                else if (DynelManager.LocalPlayer.Identity == CityBuddy.Leader
                    && !DynelManager.NPCs.Any(c => c.Health > 0)
                    && !CityBuddy.CityUnderAttack
                    && (CityController.CloakState == CloakStatus.Unknown || CityController.CanToggleCloak()))
                {
                    return new CityControllerState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            MoveToStart();

            Chat.WriteLine("City state");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("Exit city state");
        }

        public void Tick()
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
                if (_target != null && _atStart)
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
                else
                {
                    MoveToStart();
                    UpdateAtStartStatus();
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

            MovementController.Instance.SetDestination(CityBuddy._leaderPos);
        }

        public static void MoveToStart()
        {
            Dictionary<int, Vector3> destinationByPlayfield = new Dictionary<int, Vector3>
            {
                { CityBuddy.MontroyalCity, CityBuddy._montroyalGaurdPos },
                { CityBuddy.SerenityIslands, CityBuddy._serenityGaurdPos },
                { CityBuddy.PlayadelDesierto, CityBuddy._playadelGaurdPos }
            };

            int currentPlayfield = Playfield.ModelIdentity.Instance;

            if (destinationByPlayfield.TryGetValue(currentPlayfield, out Vector3 destination) &&
                DynelManager.LocalPlayer.Position.Distance2DFrom(destination) > 10)
            {
                MovementController.Instance.SetDestination(destination);
            }
        }

        private void UpdateAtStartStatus()
        {
            Vector3 currentPos = DynelManager.LocalPlayer.Position;
            int currentPlayfieldInstance = Playfield.ModelIdentity.Instance;

            _atStart =
                (currentPlayfieldInstance == CityBuddy.MontroyalCity && currentPos.Distance2DFrom(CityBuddy._montroyalGaurdPos) <= 10) ||
                (currentPlayfieldInstance == CityBuddy.SerenityIslands && currentPos.Distance2DFrom(CityBuddy._serenityGaurdPos) <= 10) ||
                (currentPlayfieldInstance == CityBuddy.PlayadelDesierto && currentPos.Distance2DFrom(CityBuddy._playadelGaurdPos) <= 10);
        }
    }
}
