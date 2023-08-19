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

        //Bosses
        private SimpleChar _masterofBiologicalMetamorphoses;
        private SimpleChar _masterofTimeandSpace;
        private SimpleChar _masterofPsyMod;
        private SimpleChar _masterofSilence;
        private SimpleChar _coccoonAttendant;

        private Dynel _alienCoccoon;
        private Dynel _chaHeru;
        private Dynel _regenerationConduit;
        private Dynel _nanovoider;
        private Dynel _attackMobs;
        private Dynel _allMobs;

        private Dynel _downButton;
        private Dynel _exitDevice;

        private static Corpse _corpse;

        public IState GetNextState()
        {
            _nanovoider = DynelManager.NPCs.FirstOrDefault(c => c.Name == "Nanovoider");
            _exitDevice = DynelManager.AllDynels.FirstOrDefault(c => c.Name == "Exit Device");
            _allMobs = DynelManager.NPCs.Where(c => c.Health > 0 && !c.Name.Contains("Nanovoider") && c.IsInLineOfSight).FirstOrDefault();
            _corpse = DynelManager.Corpses.OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position)).FirstOrDefault();

            if (!CityBuddy._settings["Toggle"].AsBool() || !Playfield.IsDungeon 
                || DynelManager.LocalPlayer.Room.Name != "Mothership_bossroom")
                return new IdleState();

            
                if (_exitDevice != null && _corpse == null && _allMobs == null)
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

            Chat.WriteLine("BossRoomState");
        }

        public void OnStateExit()
        {
            CityBuddy.NavMeshMovementController.Halt();

            if (DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.StopAttack();

            Chat.WriteLine("Exit BossRoomState");
        }

        public void Tick()
        {
            try
            {
                if (Game.IsZoning || !Team.IsInTeam) { return; }

                _masterofBiologicalMetamorphoses = DynelManager.NPCs.Where(c => c.Health > 0
                && c.Name == "Master of Biological Metamorphoses")
                    .FirstOrDefault();

                _regenerationConduit = DynelManager.NPCs
                .Where(c => c.Health > 0 && c.Name.Contains("Regeneration Conduit"))
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .FirstOrDefault();

                _masterofTimeandSpace = DynelManager.NPCs.Where(c => c.Health > 0
                && c.Name == "Master of Time and Space")
                    .FirstOrDefault();

                _masterofPsyMod = DynelManager.NPCs.Where(c => c.Health > 0
                && c.Name == "Master of PsyMod")
                    .FirstOrDefault();

                _masterofSilence = DynelManager.NPCs.Where(c => c.Health > 0
                && (c.Name == "Master of Silence" || c.Name == "Master of Nanovoid"))
                    .FirstOrDefault();

                _nanovoider = DynelManager.NPCs.FirstOrDefault(c => c.Name == "Nanovoider");

                _coccoonAttendant = DynelManager.NPCs.Where(c => c.Health > 0
                && c.Name.Contains("Coccoon Attendant - Cha'Heru"))
                    .FirstOrDefault();

                _alienCoccoon = DynelManager.NPCs.Where(c => c.Health > 0
                && c.Name.Contains("Alien Coccoon"))
                    .FirstOrDefault();

                //Cha'Heru
                _chaHeru = DynelManager.NPCs.Where(c => c.Health > 0
                && c.Name.Contains("Cha'Heru"))
                    .FirstOrDefault();

                _exitDevice = DynelManager.AllDynels.FirstOrDefault(c => c.Name == "Exit Device");

                _attackMobs = DynelManager.NPCs
                .Where(c => c.Health > 0
                            && !(c.Name.Contains("Master of") || c.Name.Contains("Defense System")
                            || c.Name.Contains("Regeneration Conduit") || c.Name.Contains("Nanovoider"))
                            && c.IsInLineOfSight)
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .ThenBy(c => c.HealthPercent)
                .FirstOrDefault();

                _allMobs = DynelManager.NPCs
                .Where(c => c.Health > 0
                            && !c.Name.Contains("Nanovoider")
                            && c.IsInLineOfSight)
                .FirstOrDefault();

                _corpse = DynelManager.Corpses
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .FirstOrDefault();

                if (Team.Members.Any(c => c.Character != null))
                {
                    switch (GetVoidSelection())
                    {
                        case VoidType.BiologicalMetamorphoses:
                            HandleBiologicalMetamorphosesVoid();
                            break;
                        case VoidType.TimeandSpace:
                            HandleTimeandSpaceVoid();
                            break;
                        case VoidType.PsyMod:
                            HandlePsyModVoid();
                            break;
                        case VoidType.Silence:
                            HandleSilenceVoid();
                            break;
                        case VoidType.Others:
                            HandleOtherVoid();
                            break;
                    }
                }

                if (_corpse != null && _allMobs == null)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpse.Position) > 5)
                    {
                        CityBuddy.NavMeshMovementController.SetNavMeshDestination(_corpse.Position);
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

        private enum VoidType
        {
            BiologicalMetamorphoses,
            TimeandSpace,
            PsyMod,
            Silence,
            Others
        }

        // Determine the selected void based on the objects available
        private VoidType GetVoidSelection()
        {
            if (_masterofBiologicalMetamorphoses != null)
            {
                return VoidType.BiologicalMetamorphoses;
            }

            if (_masterofTimeandSpace != null)
            {
                return VoidType.TimeandSpace;
            }

            if (_masterofPsyMod != null)
            {
                return VoidType.PsyMod;
            }

            if (_masterofSilence != null)
            {
                return VoidType.Silence;
            }

            // If none of the above, return Others
            return VoidType.Others;
        }

        // Define the void handlers
        private void HandleBiologicalMetamorphosesVoid()
        {
            
            if (_attackMobs != null)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(_attackMobs.Position) > 5)
                {
                    CityBuddy.NavMeshMovementController.SetNavMeshDestination(_attackMobs.Position);
                }
                else
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttacking
                        && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_attackMobs);
                    }
                }
            }
            else if (_regenerationConduit != null)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(_regenerationConduit.Position) > 5)
                {
                    CityBuddy.NavMeshMovementController.SetNavMeshDestination(_regenerationConduit.Position);
                }
                else
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttacking
                        && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_regenerationConduit);
                    }
                }
            }
            else
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(_masterofBiologicalMetamorphoses.Position) > 5)
                {
                    CityBuddy.NavMeshMovementController.SetNavMeshDestination(_masterofBiologicalMetamorphoses.Position);
                }
                else
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttacking
                        && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_masterofBiologicalMetamorphoses);
                    }
                }
            }
        }

        private void HandleTimeandSpaceVoid()
        {
            if (_attackMobs != null)
            {
                if (DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttacking
                    && !DynelManager.LocalPlayer.IsAttackPending)
                {
                    DynelManager.LocalPlayer.Attack(_attackMobs);
                }
            }
            else
            {
                if (DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsAttacking
                    && !DynelManager.LocalPlayer.IsAttackPending)
                {
                    DynelManager.LocalPlayer.Attack(_masterofTimeandSpace);
                }
            }
        }

        private void HandlePsyModVoid()
        {
            if (_attackMobs != null)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(_attackMobs.Position) > 5)
                {
                    CityBuddy.NavMeshMovementController.SetNavMeshDestination(_attackMobs.Position);
                }
                else
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttacking
                        && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_attackMobs);
                    }
                }
            }
            else
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(_masterofPsyMod.Position) > 5)
                {
                    CityBuddy.NavMeshMovementController.SetNavMeshDestination(_masterofPsyMod.Position);
                }
                else
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttacking
                        && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_masterofPsyMod);
                    }
                }
            }
        }

        private void HandleSilenceVoid()
        {
            if (_attackMobs != null)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(_attackMobs.Position) > 5)
                {
                    CityBuddy.NavMeshMovementController.SetNavMeshDestination(_attackMobs.Position);
                }
                else
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttacking
                        && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_attackMobs);
                    }
                }
            }
            else if (_attackMobs == null)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(_masterofSilence.Position) > 5)
                {
                    CityBuddy.NavMeshMovementController.SetNavMeshDestination(_masterofSilence.Position);
                }
                else
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null
                        && !DynelManager.LocalPlayer.IsAttacking
                        && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_masterofSilence);
                    }
                }
            }
        }

        private void HandleOtherVoid()
        {
            if (_coccoonAttendant != null || _alienCoccoon != null || _chaHeru != null)
            {
                if (_attackMobs != null)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_attackMobs.Position) > 5)
                    {
                        CityBuddy.NavMeshMovementController.SetNavMeshDestination(_attackMobs.Position);
                    }
                    else
                    {
                        if (DynelManager.LocalPlayer.FightingTarget == null
                            && !DynelManager.LocalPlayer.IsAttacking
                            && !DynelManager.LocalPlayer.IsAttackPending)
                        {
                            DynelManager.LocalPlayer.Attack(_attackMobs);
                        }
                    }
                }
                else if (_attackMobs == null)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_coccoonAttendant.Position) > 5)
                    {
                        CityBuddy.NavMeshMovementController.SetNavMeshDestination(_coccoonAttendant.Position);
                    }
                    else
                    {
                        if (DynelManager.LocalPlayer.FightingTarget == null
                            && !DynelManager.LocalPlayer.IsAttacking
                            && !DynelManager.LocalPlayer.IsAttackPending)
                        {
                            DynelManager.LocalPlayer.Attack(_coccoonAttendant);
                        }
                    }
                }
                else if (_alienCoccoon != null && _attackMobs == null)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_alienCoccoon.Position) > 5)
                    {
                        CityBuddy.NavMeshMovementController.SetNavMeshDestination(_alienCoccoon.Position);
                    }
                    else
                    {
                        if (DynelManager.LocalPlayer.FightingTarget == null
                            && !DynelManager.LocalPlayer.IsAttacking
                            && !DynelManager.LocalPlayer.IsAttackPending)
                        {
                            DynelManager.LocalPlayer.Attack(_alienCoccoon);
                        }
                    }
                }
            }
        }
    }
}
