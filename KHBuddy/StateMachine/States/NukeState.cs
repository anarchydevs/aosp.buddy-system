using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using KHBuddy.IPCMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using static AOSharp.Core.Battlestation;
using static KHBuddy.KHBuddy;

namespace KHBuddy
{
    public class NukeState : IState
    {
        public const double RefreshMongoTime = 8f;
        public const double RefreshAbsorbTime = 11f;

        public double _refreshMongoTimer;
        public double _refreshAbsorbTimer;

        public static List<Corpse> _hecksCorpsesAtPosEast;
        public static List<Corpse> _hecksCorpsesAtPosWest;
        public static List<Corpse> _hecksCorpsesAtPosBeach;
        public static List<SimpleChar> _hecksAtPos;

        public static List<Vector3> positions = new List<Vector3>
        {
            new Vector3(901.9f, 4.4f, 299.6f), //Beach
            new Vector3(1043.2f, 1.6f, 1020.5f), //West
            new Vector3(1115.9f, 1.6f, 1064.3f) //East
        };

        private SideSelection[] sides =
        {
            SideSelection.Beach,
            SideSelection.West,
            SideSelection.East,
            SideSelection.EastAndWest
        };

        public IState GetNextState()
        {
            if (DynelManager.LocalPlayer.Profession == Profession.NanoTechnician && !Team.IsInTeam)
            {
                _settings["Toggle"] = false;
                return new IdleState();
            }

            if (DynelManager.LocalPlayer.Profession == Profession.Enforcer)
            {
                SideSelection currentSelection = (SideSelection)_settings["SideSelection"].AsInt32();

                //Chat.WriteLine($"Current Selection: {currentSelection}");

                List<SimpleChar> _hecks = DynelManager.NPCs
                    .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                        && x.DistanceFrom(DynelManager.LocalPlayer) <= 20f
                        && x.IsAlive && x.IsInLineOfSight)
                    .OrderBy(x => x.DistanceFrom(DynelManager.LocalPlayer))
                    .ToList();

                if (_hecks.Count > 0) 
                {
                    return null;
                }

                _init = true;
                //PullState._counterVec = 0;

                if (currentSelection == SideSelection.EastAndWest)
                {
                    //_timer = Time.NormalTime;
                    //_init = true;

                    if (_doingEast)
                    {
                        _doingEast = false;
                        _doingWest = true;
                        IPCChannel.Broadcast(new MoveWestMessage());
                        MovementController.Instance.SetPath(Constants.PathToWest);
                        Chat.WriteLine("Nuke state, setting _doingWest true");
                    }
                    else if (_doingWest)
                    {
                        _doingWest = false;
                        _doingEast = true;
                        IPCChannel.Broadcast(new MoveEastMessage());
                        MovementController.Instance.SetPath(Constants.PathToEast);
                        Chat.WriteLine("Nuke state, setting _doingEast true");
                    }

                    return new PullState();
                }

                for (int i = 0; i < sides.Length; i++)
                {
                    if (sides[i] == currentSelection || (i > 0 && sides[3] == currentSelection))
                    {
                        //_init = true;
                        //_timer = Time.NormalTime;
                        //PullState._counterVec = 0;
                        return new PullState();
                    }
                }
            }

            return null;
        }


        public void OnStateEnter()
        {
            if (_settings["Toggle"].AsBool()
                        && DynelManager.LocalPlayer.Profession == Profession.NanoTechnician)
            {

                if (SideSelection.East == (SideSelection)_settings["SideSelection"].AsInt32()
                    || SideSelection.EastAndWest == (SideSelection)_settings["SideSelection"].AsInt32())
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(1090.2f, 28.1f, 1050.1f)) > 1f && !MovementController.Instance.IsNavigating)
                    {
                        MovementController.Instance.SetDestination(new Vector3(1090.2f, 28.1f, 1050.1f));
                    }
                }
                else if (SideSelection.West == (SideSelection)_settings["SideSelection"].AsInt32())
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(1065.4f, 26.2f, 1033.5f)) > 1f && !MovementController.Instance.IsNavigating)
                    {
                        MovementController.Instance.SetDestination(new Vector3(1065.4f, 26.2f, 1033.5f));
                    }
                }
            }
            //KHBuddy._stateTimeOut = Time.NormalTime;
            Chat.WriteLine("Nuke State");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("NukeState::OnStateExit");
        }

        public void Tick()
        {
            try
            {
                if (DynelManager.LocalPlayer.Profession == Profession.NanoTechnician)
                {
                    float[] distances = { 10f, 43f, 60f, 60f };

                    for (int i = 0; i < sides.Length; i++)
                    {
                        if (sides[i] == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32() || (i > 0 && sides[3] == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32()))
                        {
                            var _hecksAtPos = DynelManager.NPCs
                                .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                                    && x.DistanceFrom(DynelManager.LocalPlayer) <= distances[i]
                                    && x.IsAlive && x.IsInLineOfSight && x.IsAttacking
                                    && !x.IsMoving
                                    && x.FightingTarget.Identity != DynelManager.LocalPlayer.Identity
                                    && x.Position.DistanceFrom(positions[i]) < 10f)
                                .ToList();

                            if (_hecksAtPos.Count >= 1 && DynelManager.LocalPlayer.FightingTarget == null //&& !KHBuddy.NeedsKit
                                && (DynelManager.LocalPlayer.NanoPercent >= 70 || DynelManager.LocalPlayer.HealthPercent >= 70))
                            {
                                if (DynelManager.LocalPlayer.FightingTarget == null
                                   && !DynelManager.LocalPlayer.IsAttacking
                                   && !DynelManager.LocalPlayer.IsAttackPending)
                                {
                                    DynelManager.LocalPlayer.Attack(_hecksAtPos.FirstOrDefault());
                                }

                                //KHBuddy._stateTimeOut = Time.NormalTime;
                            }

                            if (DynelManager.LocalPlayer.NanoPercent < 60 || DynelManager.LocalPlayer.HealthPercent < 60)
                            {
                                //KHBuddy.NeedsKit = true;

                                DynelManager.LocalPlayer.StopAttack();
                            }
                        }
                    }
                }

                if (DynelManager.LocalPlayer.Profession == Profession.Enforcer)
                {
                    // Find hecks within 20 units
                    _hecksAtPos = DynelManager.NPCs
                    .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                        && x.DistanceFrom(DynelManager.LocalPlayer) <= 20f
                        && x.IsAlive && x.IsInLineOfSight
                        && !x.IsMoving
                        && x.FightingTarget != null)
                    .ToList();

                    // Cast Mongo Demolish if conditions are met
                    Spell.Find(270786, out Spell mongoDemolish);

                    if (_hecksAtPos.Count >= 1)
                    {
                        if (!Spell.HasPendingCast && mongoDemolish.IsReady && Time.NormalTime > _refreshMongoTimer + RefreshMongoTime)
                        {
                            mongoDemolish.Cast();
                            _refreshMongoTimer = Time.NormalTime;
                        }
                    }

                    // Find hecks beyond 20 units but in line of sight
                    List<SimpleChar> _distantHecks = DynelManager.NPCs
                    .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                        && x.DistanceFrom(DynelManager.LocalPlayer) > 20f
                        && x.IsAlive && x.IsInLineOfSight)
                    .ToList();

                    // Taunt distant hecks
                    foreach (SimpleChar distantHeck in _distantHecks)
                    {
                        HandleTaunting(distantHeck);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + KHBuddy.GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != KHBuddy.previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    KHBuddy.previousErrorMessage = errorMessage;
                }
            }
        }

        public static void HandleTaunting(SimpleChar target)
        {
            Item item = null;

            if (Inventory.Find(83920, out item) ||  // Aggression Enhancer
                Inventory.Find(83919, out item) ||  // Aggression Multiplier
                Inventory.Find(152029, out item) || // Aggression Enhancer (Jealousy Augmented)
                Inventory.Find(152028, out item) || // Aggression Multiplier (Jealousy Augmented)
                Inventory.Find(244655, out item) || // Scorpio's Aim of Anger
                Inventory.Find(253186, out item) || // Codex of the Insulting Emerto (Low)
                Inventory.Find(253187, out item))   // Codex of the Insulting Emerto (High)
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    item.Use(target, true);
                }
            }
        }
    }
}
