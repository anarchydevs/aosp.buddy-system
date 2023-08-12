using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using KHBuddy.IPCMessages;
using OSTBuddy;
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
            if (Time.NormalTime - KHBuddy._stateTimeOut > 1300f
                && DynelManager.LocalPlayer.Profession == Profession.NanoTechnician)
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
            KHBuddy._stateTimeOut = Time.NormalTime;
            Chat.WriteLine("NukeState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("NukeState::OnStateExit");
        }

        public void Tick()
        {
            _hecksAtPos = DynelManager.NPCs
                .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                    && x.DistanceFrom(DynelManager.LocalPlayer) <= 10f
                    && x.IsAlive && x.IsInLineOfSight
                    && !x.IsMoving
                    && x.FightingTarget != null)
                .ToList();

            if (DynelManager.LocalPlayer.Profession == Profession.NanoTechnician)
            {
                if (KHBuddy.SideSelection.Beach == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32())
                {
                    List<SimpleChar> _hecksAtPosBeach = DynelManager.NPCs
                            .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                                && x.DistanceFrom(DynelManager.LocalPlayer) <= 43f
                                && x.IsAlive && x.IsInLineOfSight && x.IsAttacking
                                && !x.IsMoving
                                && x.FightingTarget.Identity != DynelManager.LocalPlayer.Identity
                                && x.Position.DistanceFrom(new Vector3(901.9f, 4.4f, 299.6f)) < 10f)
                            .ToList();

                    if (_hecksAtPosBeach.Count >= 1)
                    {
                        if (DynelManager.LocalPlayer.FightingTarget == null && !KHBuddy.Sitting  &&
                            (DynelManager.LocalPlayer.NanoPercent >= 31 || DynelManager.LocalPlayer.HealthPercent >= 66))
                        {
                            DynelManager.LocalPlayer.Attack(_hecksAtPosBeach.FirstOrDefault());
                            KHBuddy._stateTimeOut = Time.NormalTime;
                        }
                        if (DynelManager.LocalPlayer.NanoPercent < 30 || DynelManager.LocalPlayer.HealthPercent < 65)
                        {
                            DynelManager.LocalPlayer.StopAttack();
                        }
                    }
                }

                if (KHBuddy.SideSelection.West == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32()
                    || KHBuddy.SideSelection.EastAndWest == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32())
                {
                    List<SimpleChar> _hecksAtPosWest = DynelManager.NPCs
                            .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                                && x.DistanceFrom(DynelManager.LocalPlayer) <= 60f
                                && x.IsAlive && x.IsInLineOfSight && x.IsAttacking
                                && !x.IsMoving
                                && x.FightingTarget.Identity != DynelManager.LocalPlayer.Identity
                                && x.Position.DistanceFrom(new Vector3(1043.2f, 1.6f, 1020.5f)) < 10f)
                            .ToList();


                    if (_hecksAtPosWest.Count >= 1)
                    {
                        if (DynelManager.LocalPlayer.FightingTarget == null && !KHBuddy.Sitting  &&
                            (DynelManager.LocalPlayer.NanoPercent >= 31 || DynelManager.LocalPlayer.HealthPercent >= 66))
                        {
                            DynelManager.LocalPlayer.Attack(_hecksAtPosWest.FirstOrDefault());
                            KHBuddy._stateTimeOut = Time.NormalTime;
                        }
                        if (DynelManager.LocalPlayer.NanoPercent < 30 || DynelManager.LocalPlayer.HealthPercent < 65)
                        {
                            DynelManager.LocalPlayer.StopAttack();
                        }
                    }
                }

                if (KHBuddy.SideSelection.East == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32()
                    || KHBuddy.SideSelection.EastAndWest == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32())
                {
                    List<SimpleChar> _hecksAtPosEast = DynelManager.NPCs
                            .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                                && x.DistanceFrom(DynelManager.LocalPlayer) <= 60f
                                && x.IsAlive && x.IsInLineOfSight && x.IsAttacking
                                && !x.IsMoving
                                && x.FightingTarget.Identity != DynelManager.LocalPlayer.Identity
                                && x.Position.DistanceFrom(new Vector3(1115.9f, 1.6f, 1064.3f)) < 10f)
                            .ToList();

                    if (_hecksAtPosEast.Count >= 1)
                    {
                        if (DynelManager.LocalPlayer.FightingTarget == null && !KHBuddy.Sitting  &&
                            (DynelManager.LocalPlayer.NanoPercent >= 31 || DynelManager.LocalPlayer.HealthPercent >= 66))
                        {
                            DynelManager.LocalPlayer.Attack(_hecksAtPosEast.FirstOrDefault());
                            KHBuddy._stateTimeOut = Time.NormalTime;
                        }
                        if (DynelManager.LocalPlayer.NanoPercent < 30 || DynelManager.LocalPlayer.HealthPercent < 65)
                        {
                            DynelManager.LocalPlayer.StopAttack();
                        }
                    }
                }
            }

            if (DynelManager.LocalPlayer.Profession == Profession.Enforcer)
            {
                Spell.Find(270786, out Spell mongobuff);

                //if (absorb == null)
                //    absorb = Spell.List.Where(x => x.Nanoline == NanoLine.AbsorbACBuff).OrderBy(x => x.StackingOrder).FirstOrDefault();

                if (_hecksAtPos.Count >= 1)
                {
                    if (!Spell.HasPendingCast && mongobuff.IsReady && Time.NormalTime > _refreshMongoTimer + RefreshMongoTime)
                    {
                        mongobuff.Cast();
                        _refreshMongoTimer = Time.NormalTime;
                    }
                    //if (!Spell.HasPendingCast && absorb.IsReady && Time.NormalTime > _refreshAbsorbTimer + RefreshAbsorbTime)
                    //{
                    //    absorb.Cast();
                    //    _refreshAbsorbTimer = Time.NormalTime;
                    //}
                }
            }
        }
    }
}
