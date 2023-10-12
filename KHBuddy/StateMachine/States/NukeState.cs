using AOSharp.Common.GameData;
using AOSharp.Core;
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

        private KHBuddy.SideSelection[] sides =
        {
            SideSelection.Beach,
            SideSelection.West,
            SideSelection.East,
            SideSelection.EastAndWest
        };

        //Spell absorb = null;

        public IState GetNextState()
        {
            // Handle specific professions
            if (DynelManager.LocalPlayer.Profession == Profession.NanoTechnician && !Team.IsInTeam)
            {
                _settings["Toggle"] = false;
                return new IdleState();
            }

            if (DynelManager.LocalPlayer.Profession != Profession.Enforcer)
                return null;

            SideSelection currentSelection = (SideSelection)_settings["SideSelection"].AsInt32();

            for (int i = 0; i < sides.Length; i++)
            {
                if (sides[i] == currentSelection || (i > 0 && sides[3] == currentSelection))
                {
                    if (ShouldEnterPullState(positions[i]))
                    {
                        _init = true;
                        _timer = Time.NormalTime;
                        PullState._counterVec = 0;
                        return new PullState();
                    }
                }
            }

            if (currentSelection == SideSelection.EastAndWest)
            {
                Vector3 currentPosition = _doingEast ? new Vector3(1115.9f, 1.6f, 1064.3f) : new Vector3(1043.2f, 1.6f, 1020.5f);

                if (ShouldEnterPullState(currentPosition))
                {
                    _timer = Time.NormalTime;
                    _init = true;

                    if (_doingEast)
                    {
                        _doingEast = false;
                        _doingWest = true;
                        IPCChannel.Broadcast(new MoveWestMessage());
                        MovementController.Instance.SetPath(Constants.PathToWest);
                    }
                    else if (_doingWest)
                    {
                        _doingWest = false;
                        _doingEast = true;
                        IPCChannel.Broadcast(new MoveEastMessage());
                        MovementController.Instance.SetPath(Constants.PathToEast);
                    }

                    return new PullState();
                }
            }

            return null;
        }

        private bool ShouldEnterPullState(Vector3 position)
        {
            var hecklerCorpses = DynelManager.Corpses
                .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                    && x.DistanceFrom(DynelManager.LocalPlayer) <= 45f
                    && x.Position.DistanceFrom(position) < 8f)
                .ToList();

            return hecklerCorpses.Count >= 3;
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
            //Chat.WriteLine("NukeState::OnStateEnter");
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
                    _hecksAtPos = DynelManager.NPCs
                   .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                       && x.DistanceFrom(DynelManager.LocalPlayer) <= 20f
                       && x.IsAlive && x.IsInLineOfSight
                       && !x.IsMoving
                       && x.FightingTarget != null)
                   .ToList();

                    Spell.Find(270786, out Spell mongoDemolish);

                    if (_hecksAtPos.Count >= 1)
                    {
                        if (!Spell.HasPendingCast && mongoDemolish.IsReady && Time.NormalTime > _refreshMongoTimer + RefreshMongoTime)
                        {
                            mongoDemolish.Cast();
                            _refreshMongoTimer = Time.NormalTime;
                        }
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
    }
}
