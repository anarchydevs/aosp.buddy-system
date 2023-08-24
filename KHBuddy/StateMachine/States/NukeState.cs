using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using KHBuddy.IPCMessages;
using OSTBuddy;
using System;
using System.Collections.Generic;
using System.Linq;

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

        Spell absorb = null;

        public IState GetNextState()
        {
            //if (Time.NormalTime - KHBuddy._stateTimeOut > 1300f
                if (DynelManager.LocalPlayer.Profession == Profession.NanoTechnician && !Team.IsInTeam)
            {
                KHBuddy._settings["Toggle"] = false;
                Chat.WriteLine("Turning off bot, Idle for too long.");
                return new IdleState();
            }

            if (KHBuddy.SideSelection.Beach == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32()
                && DynelManager.LocalPlayer.Profession == Profession.Enforcer)
            {
                _hecksCorpsesAtPosBeach = DynelManager.Corpses
                    .Where(x => x.Name.Contains("Heckler") || x.Name.Contains("Voracious")
                        && x.DistanceFrom(DynelManager.LocalPlayer) <= 45f
                        && x.Position.DistanceFrom(new Vector3(901.9f, 4.4f, 299.6f)) < 8f)
                    .ToList();

                if (_hecksAtPos.Count == 0 && _hecksCorpsesAtPosBeach.Count >= 3)
                {
                    KHBuddy._init = true;
                    KHBuddy._timer = Time.NormalTime;
                    PullState._counterVec = 0;
                    return new PullState();
                }
            }

            if (KHBuddy.SideSelection.East == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32()
                && DynelManager.LocalPlayer.Profession == Profession.Enforcer)
            {
                _hecksCorpsesAtPosEast = DynelManager.Corpses
                    .Where(x => x.Name.Contains("Heckler") || x.Name.Contains("Voracious")
                        && x.DistanceFrom(DynelManager.LocalPlayer) <= 45f
                        && x.Position.DistanceFrom(new Vector3(1115.9f, 1.6f, 1064.3f)) < 8f)
                    .ToList();

                if (_hecksAtPos.Count == 0 && _hecksCorpsesAtPosEast.Count >= 3)
                {
                    KHBuddy._init = true;
                    KHBuddy._timer = Time.NormalTime;
                    PullState._counterVec = 0;
                    return new PullState();
                }
            }

            if (KHBuddy.SideSelection.West == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32()
                && DynelManager.LocalPlayer.Profession == Profession.Enforcer)
            {
                _hecksCorpsesAtPosWest = DynelManager.Corpses
                    .Where(x => x.Name.Contains("Heckler") || x.Name.Contains("Voracious")
                        && x.DistanceFrom(DynelManager.LocalPlayer) <= 45f
                        && x.Position.DistanceFrom(new Vector3(1043.2f, 1.6f, 1020.5f)) < 8f)
                    .ToList();

                if (_hecksAtPos.Count == 0 && _hecksCorpsesAtPosWest.Count >= 3)
                {
                    KHBuddy._init = true;
                    KHBuddy._timer = Time.NormalTime;
                    PullState._counterVec = 0;
                    return new PullState();
                }
            }

            if (KHBuddy.SideSelection.EastAndWest == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32()
                && DynelManager.LocalPlayer.Profession == Profession.Enforcer)
            {
                if (KHBuddy._doingEast)
                {
                    _hecksCorpsesAtPosEast = DynelManager.Corpses
                        .Where(x => x.Name.Contains("Heckler") || x.Name.Contains("Voracious")
                            && x.DistanceFrom(DynelManager.LocalPlayer) <= 45f
                            && x.Position.DistanceFrom(new Vector3(1115.9f, 1.6f, 1064.3f)) < 8f)
                        .ToList();

                    if (_hecksAtPos.Count == 0 && _hecksCorpsesAtPosEast.Count >= 3)
                    {
                        KHBuddy._timer = Time.NormalTime;
                        KHBuddy._init = true;
                        KHBuddy._doingEast = false;
                        KHBuddy._doingWest = true;
                        KHBuddy.IPCChannel.Broadcast(new MoveWestMessage());
                        MovementController.Instance.SetPath(Constants.PathToWest);
                        return new PullState();
                    }
                }

                if (KHBuddy._doingWest)
                {
                    _hecksCorpsesAtPosWest = DynelManager.Corpses
                        .Where(x => x.Name.Contains("Heckler") || x.Name.Contains("Voracious")
                            && x.DistanceFrom(DynelManager.LocalPlayer) <= 45f
                            && x.Position.DistanceFrom(new Vector3(1043.2f, 1.6f, 1020.5f)) < 8f)
                        .ToList();

                    if (_hecksAtPos.Count == 0 && _hecksCorpsesAtPosWest.Count >= 3)
                    {
                        KHBuddy._timer = Time.NormalTime;
                        KHBuddy._doingWest = false;
                        KHBuddy._doingEast = true;
                        KHBuddy.IPCChannel.Broadcast(new MoveEastMessage());
                        MovementController.Instance.SetPath(Constants.PathToEast);
                        return new PullState();
                    }
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
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
                    List<Vector3> positions = new List<Vector3>
                {
                    new Vector3(901.9f, 4.4f, 299.6f),
                    new Vector3(1043.2f, 1.6f, 1020.5f),
                    new Vector3(1115.9f, 1.6f, 1064.3f)
                };

                    float[] distances = { 10f, 43f, 60f, 60f };
                    var sides = new KHBuddy.SideSelection[] { KHBuddy.SideSelection.Beach, KHBuddy.SideSelection.West, KHBuddy.SideSelection.East, KHBuddy.SideSelection.EastAndWest };

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

                            if (_hecksAtPos.Count >= 1 && DynelManager.LocalPlayer.FightingTarget == null && !KHBuddy.NeedsKit
                                && (DynelManager.LocalPlayer.NanoPercent >= 67 || DynelManager.LocalPlayer.HealthPercent >= 67))
                            {
                                DynelManager.LocalPlayer.Attack(_hecksAtPos.FirstOrDefault());
                                //KHBuddy._stateTimeOut = Time.NormalTime;
                            }

                            if (DynelManager.LocalPlayer.NanoPercent < 66 || DynelManager.LocalPlayer.HealthPercent < 66)
                            {
                                KHBuddy.NeedsKit = true;

                                DynelManager.LocalPlayer.StopAttack();
                            }
                        }
                    }
                }

                if (DynelManager.LocalPlayer.Profession == Profession.Enforcer)
                {
                    _hecksAtPos = DynelManager.NPCs
                   .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                       && x.DistanceFrom(DynelManager.LocalPlayer) <= 10f
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
