using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using System;
using System.Linq;

namespace ALBBuddy
{
    public class FightState : IState
    {
        public static int _aggToolCounter = 0;
        public static int _attackTimeout = 0;

        private SimpleChar _target;

        private double _fightStartTime;
        public const double FightTimeout = 20f;

        public FightState(SimpleChar target)
        {
            _target = target;
        }

        public IState GetNextState()
        {
            if ( Extensions.IsNull(_target) 
                || Time.NormalTime > _fightStartTime + FightTimeout)
                return new PatrolState();

            return null;
        }

        public void OnStateEnter()
        {
            _fightStartTime = Time.NormalTime;
            ALBBuddy.NavMeshMovementController.Halt();
        }

        public void OnStateExit()
        {
            _aggToolCounter = 0;
            _attackTimeout = 0;
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Playfield.ModelIdentity.Instance == Constants.Albtraum)
            {
                if (DynelManager.LocalPlayer.Identity != ALBBuddy.Leader)
                {
                    if (ALBBuddy._leaderPos != Vector3.Zero && DynelManager.LocalPlayer.Position.DistanceFrom(ALBBuddy._leaderPos) > 7f
                        && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
                    {
                        ALBBuddy.NavMeshMovementController.SetNavMeshDestination(ALBBuddy._leaderPos);
                    }
                }

                if (_target == null) { return; }

                if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 10f)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null
                            && !DynelManager.LocalPlayer.IsAttacking
                            && !DynelManager.LocalPlayer.IsAttackPending
                            && AttackingTeam(_target))
                    {
                        DynelManager.LocalPlayer.Attack(_target);
                        Chat.WriteLine($"Attacking {_target.Name}.");
                    }
                }

                if (DynelManager.LocalPlayer.Identity == ALBBuddy.Leader
                        && _target.IsInLineOfSight
                        && DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttacking
                        && !DynelManager.LocalPlayer.IsAttackPending
                        && !AttackingTeam(_target))
                    HandleTaunting(_target);
            }
        }

        public static void HandleTaunting(SimpleChar target)
        {
            if (_aggToolCounter >= 2)
            {
                if (_attackTimeout >= 1)
                {
                    ALBBuddy.NavMeshMovementController.SetMovement(MovementAction.JumpStart);
                    ALBBuddy.NavMeshMovementController.SetNavMeshDestination(target.Position);
                    _attackTimeout = 0;
                    _aggToolCounter = 0;
                    return;
                }

                _attackTimeout++;
                _aggToolCounter = 0;
            }
            else if (Inventory.Find(83920, out Item aggroTool)) //Aggression Enhancer 
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    aggroTool.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
            else if (Inventory.Find(83919, out Item aggroMultiTool)) //Aggression Multiplier
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    aggroMultiTool.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
            else if (Inventory.Find(152029, out Item JealousyTool)) //Aggression Enhancer (Jealousy Augmented) 
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    JealousyTool.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
            else if (Inventory.Find(152028, out Item JealousyMultiTool)) //Aggression Multiplier (Jealousy Augmented) 
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    JealousyMultiTool.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
            else if (Inventory.Find(244655, out Item scorpioTool)) //Scorpio's Aim of Anger
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    scorpioTool.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
            else if (Inventory.Find(253186, out Item EmertoLow))//Codex of the Insulting Emerto
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    EmertoLow.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
            else if (Inventory.Find(253187, out Item EmertoHigh))//Codex of the Insulting Emerto
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    EmertoHigh.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
        }

        public static bool AttackingTeam(SimpleChar mob)
        {
            if (mob?.FightingTarget == null) { return true; }

            if (mob?.FightingTarget?.Name == "Guardian Spirit of Purification"
                || mob?.FightingTarget?.Name == "Rookie Alien Hunter"
                || mob?.FightingTarget?.Name == "Unicorn Service Tower Alpha"
                || mob?.FightingTarget?.Name == "Unicorn Service Tower Delta"
                || mob?.FightingTarget?.Name == "Unicorn Service Tower Gamma") { return true; }

            if (Team.IsInTeam)
            {
                return Team.Members.Select(m => m.Name).Contains(mob.FightingTarget?.Name)
                   || (mob?.FightingTarget?.IsPet == true
                        && Team.Members.Select(c => c.Identity.Instance).Any(c => c == mob?.FightingTarget?.PetOwnerId));
            }

            return mob.FightingTarget?.Name == DynelManager.LocalPlayer.Name
                || (mob?.FightingTarget?.IsPet == true
                    && mob.FightingTarget?.PetOwnerId == DynelManager.LocalPlayer.Identity.Instance);
        }

        public static bool TeamAttacking(SimpleChar mob)
        {
            if (Team.IsInTeam)
            {
                return Team.Members.Any(c => c.Character?.FightingTarget?.Identity == mob.Identity)
                    || (DynelManager.Characters.Any(c => c.IsPet
                        && Team.Members.Select(b => b.Identity.Instance).Contains(c.PetOwnerId)
                        && c.FightingTarget != null && c.FightingTarget.Identity == mob.Identity));
            }

            return DynelManager.LocalPlayer.FightingTarget?.Identity == mob.Identity
                || (DynelManager.Characters.Any(c => c.IsPet
                        && c.PetOwnerId == DynelManager.LocalPlayer.Identity.Instance
                        && c.FightingTarget != null && c.FightingTarget.Identity == mob.Identity));
        }

        //public bool AttackingTeam(SimpleChar mob)
        //{
        //    if (mob.FightingTarget == null) { return false; }

        //    if (Team.IsInTeam)
        //        return Team.Members.Select(m => m.Name).Contains(mob.FightingTarget?.Name)
        //                || (bool)mob.FightingTarget?.IsPet;

        //    return mob.FightingTarget?.Name == DynelManager.LocalPlayer.Name
        //        || (bool)mob.FightingTarget?.IsPet;
        //}
    }
}