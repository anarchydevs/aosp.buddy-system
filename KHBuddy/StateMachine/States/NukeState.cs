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
            //if (Time.NormalTime - KHBuddy._stateTimeOut > 1300f
            if (DynelManager.LocalPlayer.Profession == Profession.NanoTechnician)
            {
                if (!Team.IsInTeam)
                {
                    _settings["Toggle"] = false;
                    //Chat.WriteLine("Turning off bot, Idle for too long.");
                    return new IdleState();
                }
            }

            if (DynelManager.LocalPlayer.Profession == Profession.Enforcer)
            {
                for (int i = 0; i < sides.Length; i++)
                {
                    if (sides[i] == (SideSelection)_settings["SideSelection"].AsInt32() ||
                        (i > 0 && sides[3] == (SideSelection)_settings["SideSelection"].AsInt32()))
                    {
                        var _hecksCorpsesAtPosBeach = DynelManager.Corpses
                                .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                                    && x.DistanceFrom(DynelManager.LocalPlayer) <= 45f
                                    && x.Position.DistanceFrom(positions[i]) < 8f)
                                .ToList();

                        if (_hecksAtPos.Count == 0 && _hecksCorpsesAtPosBeach.Count >= 3)
                        {
                            _init = true;
                            _timer = Time.NormalTime;
                            PullState._counterVec = 0;
                            return new PullState();
                        }
                    }
                }

                if (SideSelection.EastAndWest == (SideSelection)_settings["SideSelection"].AsInt32())
                {
                    if (_doingEast)
                    {
                        _hecksCorpsesAtPosEast = DynelManager.Corpses
                            .Where(x => x.Name.Contains("Heckler") || x.Name.Contains("Voracious")
                                && x.DistanceFrom(DynelManager.LocalPlayer) <= 45f
                                && x.Position.DistanceFrom(new Vector3(1115.9f, 1.6f, 1064.3f)) < 8f)
                            .ToList();

                        if (_hecksAtPos.Count == 0 && _hecksCorpsesAtPosEast.Count >= 3)
                        {
                            _timer = Time.NormalTime;
                            _init = true;
                            _doingEast = false;
                            _doingWest = true;
                            IPCChannel.Broadcast(new MoveWestMessage());
                            MovementController.Instance.SetPath(Constants.PathToWest);
                            return new PullState();
                        }
                    }

                    if (_doingWest)
                    {
                        _hecksCorpsesAtPosWest = DynelManager.Corpses
                            .Where(x => x.Name.Contains("Heckler") || x.Name.Contains("Voracious")
                                && x.DistanceFrom(DynelManager.LocalPlayer) <= 45f
                                && x.Position.DistanceFrom(new Vector3(1043.2f, 1.6f, 1020.5f)) < 8f)
                            .ToList();

                        if (_hecksAtPos.Count == 0 && _hecksCorpsesAtPosWest.Count >= 3)
                        {
                            _timer = Time.NormalTime;
                            _doingWest = false;
                            _doingEast = true;
                            IPCChannel.Broadcast(new MoveEastMessage());
                            MovementController.Instance.SetPath(Constants.PathToEast);
                            return new PullState();
                        }
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

                if (SideSelection.East == (SideSelection)_settings["SideSelection"].AsInt32())
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(1090.2f, 28.1f, 1050.1f)) > 1f && !MovementController.Instance.IsNavigating)
                    {
                        MovementController.Instance.SetDestination(new Vector3(1090.2f, 28.1f, 1050.1f));
                    }
                }
                else if (SideSelection.West == (SideSelection)_settings["SideSelection"].AsInt32()
                    || SideSelection.EastAndWest == (SideSelection)_settings["SideSelection"].AsInt32())
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(1065.4f, 26.2f, 1033.5f)) > 1f && !MovementController.Instance.IsNavigating)
                    {
                        MovementController.Instance.SetDestination(new Vector3(1065.4f, 26.2f, 1033.5f));
                    }
                }
            }
            //KHBuddy._stateTimeOut = Time.NormalTime;
            Chat.WriteLine("NukeState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("NukeState::OnStateExit");
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

                    Spell.Find(270786, out Spell mongobuff);

                    if (_hecksAtPos.Count >= 1)
                    {
                        if (!Spell.HasPendingCast && mongobuff.IsReady && Time.NormalTime > _refreshMongoTimer + RefreshMongoTime)
                        {
                            mongobuff.Cast();
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
