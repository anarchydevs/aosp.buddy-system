using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Configuration;
using System.Linq;

namespace ALBBuddy
{
    public class FightState : IState
    {
        public static int _aggToolCounter = 0;
        public static int _attackTimeout = 0;

        private SimpleChar _target;

        private double _fightStartTime;
        public const double FightTimeout = 45f;

        public FightState(SimpleChar target)
        {
            _target = target;
        }

        public IState GetNextState()
        {
            //if (Extensions.HasDied())
            //    return new DiedState();

            if (Extensions.IsNull(_target)
                || Time.NormalTime > _fightStartTime + FightTimeout)
            {
                if (ALBBuddy.ModeSelection.Roam == (ALBBuddy.ModeSelection)ALBBuddy._settings["ModeSelection"].AsInt32())
                    return new RoamState();

                return new PatrolState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine($"FightState::OnStateEnter");

            _fightStartTime = Time.NormalTime;
            ALBBuddy.NavMeshMovementController.Halt();
        }

        public void OnStateExit()
        {
            Chat.WriteLine("FightState::OnStateExit");

            _aggToolCounter = 0;
            _attackTimeout = 0;
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (DynelManager.LocalPlayer.Identity != ALBBuddy.Leader)
            {
                if (ALBBuddy._leaderPos != Vector3.Zero && DynelManager.LocalPlayer.Position.DistanceFrom(ALBBuddy._leaderPos) > 1.2f
                    && DynelManager.LocalPlayer.MovementState != MovementState.Sit && !Extensions.Rooted())
                {
                    ALBBuddy.NavMeshMovementController.SetNavMeshDestination(ALBBuddy._leaderPos);
                }
            }

            if (_target == null) { return; }

            if (_target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 19f)
            {
                HandlePathing(_target);

                if (DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttacking && !DynelManager.LocalPlayer.IsAttackPending)
                {
                    DynelManager.LocalPlayer.Attack(_target);
                    Chat.WriteLine($"Attacking {_target.Name}.");
                }
            }
            else if (DynelManager.LocalPlayer.Identity == ALBBuddy.Leader 
                && ALBBuddy.ModeSelection.Normal == (ALBBuddy.ModeSelection)ALBBuddy._settings["ModeSelection"].AsInt32())
                HandleTaunting(_target);
        }

        public static CharacterWieldedWeapon GetWieldedWeapons(SimpleChar local) => (CharacterWieldedWeapon)local.GetStat(Stat.EquippedWeapons);

        public static void HandlePathing(SimpleChar target)
        {
            if (ALBBuddy.NavMeshMovementController.IsNavigating && target.IsInLineOfSight)
            {
                if (target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 4f
                    && (GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Melee)
                    || GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.MartialArts)))
                    ALBBuddy.NavMeshMovementController.Halt();

                if (target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 11f
                    && GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Ranged))
                    ALBBuddy.NavMeshMovementController.Halt();
            }

            if (target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 4f
                && (GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Melee)
                || GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.MartialArts)))
            {
                ALBBuddy.NavMeshMovementController.SetNavMeshDestination(target.Position);
            }

            if (target.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 11f
                && GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Ranged))
            {
                ALBBuddy.NavMeshMovementController.SetNavMeshDestination(target.Position);
            }
        }

        public static void HandleTaunting(SimpleChar target)
        {
            if (_aggToolCounter >= 2)
            {
                //Reason: If we get stuck on a mob path to it
                if (_attackTimeout >= 1)
                {
                    ALBBuddy.NavMeshMovementController.SetDestination(target.Position);
                    _attackTimeout = 0;
                    _aggToolCounter = 0;
                    return;
                }

                _attackTimeout++;
                _aggToolCounter = 0;
            }
            else if (Inventory.Find(83920, 83919, out Item aggroTool)) //Aggression Enhancer 
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    ALBBuddy.NavMeshMovementController.Halt();
                    aggroTool.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
            else if (Inventory.Find(83919, 83919, out Item aggroMultiTool)) //Aggression Multiplier
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    ALBBuddy.NavMeshMovementController.Halt();
                    aggroMultiTool.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
            else if (Inventory.Find(152029, 152029, out Item JealousyTool)) //Aggression Enhancer (Jealousy Augmented) 
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    JealousyTool.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
            else if (Inventory.Find(152028, 152028, out Item JealousyMultiTool)) //Aggression Multiplier (Jealousy Augmented) 
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    JealousyMultiTool.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
            else if (Inventory.Find(244655, 244655, out Item scorpioTool)) //Scorpio's Aim of Anger
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    scorpioTool.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
            else if (Inventory.Find(253186, 253186, out Item EmertoLow))//Codex of the Insulting Emerto
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    ALBBuddy.NavMeshMovementController.Halt();
                    EmertoLow.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
            else if (Inventory.Find(253187, 253187, out Item EmertoHigh))//Codex of the Insulting Emerto
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    ALBBuddy.NavMeshMovementController.Halt();
                    EmertoHigh.Use(target, true);
                    _aggToolCounter++;
                    return;
                }
            }
        }

        [Flags]
        public enum CharacterWieldedWeapon
        {
            Fists = 0x0,            // 0x00000000000000000000b Fists / invalid
            MartialArts = 0x01,             // 0x00000000000000000001b martialarts / fists
            Melee = 0x02,             // 0x00000000000000000010b
            Ranged = 0x04,            // 0x00000000000000000100b
            Bow = 0x08,               // 0x00000000000000001000b
            Smg = 0x10,               // 0x00000000000000010000b
            Edged1H = 0x20,           // 0x00000000000000100000b
            Blunt1H = 0x40,           // 0x00000000000001000000b
            Edged2H = 0x80,           // 0x00000000000010000000b
            Blunt2H = 0x100,          // 0x00000000000100000000b
            Piercing = 0x200,         // 0x00000000001000000000b
            Pistol = 0x400,           // 0x00000000010000000000b
            AssaultRifle = 0x800,     // 0x00000000100000000000b
            Rifle = 0x1000,           // 0x00000001000000000000b
            Shotgun = 0x2000,         // 0x00000010000000000000b
            Grenade = 0x8000,     // 0x00000100000000000000b // 0x00001000000000000000b grenade / martial arts
            MeleeEnergy = 0x4000,     // 0x00001000000000000000b // 0x00000100000000000000b
            RangedEnergy = 0x10000,   // 0x00010000000000000000b
            Grenade2 = 0x20000,        // 0x00100000000000000000b
            HeavyWeapons = 0x40000,   // 0x01000000000000000000b
        }
    }
}