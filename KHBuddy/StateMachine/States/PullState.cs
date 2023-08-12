using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using KHBuddy.IPCMessages;
using OSTBuddy;
using System.Collections.Generic;
using System.Linq;

namespace KHBuddy
{
    public class PullState : IState
    {
        public static int _counterVec = 0;

        public static bool CastedMongo = false;

        private double _lastFollowTime = Time.NormalTime;

        public static IPCChannel IPCChannel { get; private set; }

        public IState GetNextState()
        {
            List<SimpleChar> _hecks = DynelManager.NPCs
                .Where(x => x.Name.Contains("Heckler")
                    && x.DistanceFrom(DynelManager.LocalPlayer) <= 10f
                    && x.IsAlive && x.IsInLineOfSight)
                .ToList();

            if (KHBuddy.SideSelection.Beach == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.KHBeachVectorList.Last()) < 3f && _hecks.Count >= 1)
                {
                    return new NukeState();
                }
            }

            if (KHBuddy.SideSelection.East == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.KHEastVectorList.Last()) < 3f && _hecks.Count >= 1)
                {
                    KHBuddy.IPCChannel.Broadcast(new MoveEastMessage());
                    return new NukeState();
                }
            }

            if (KHBuddy.SideSelection.West == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.KHWestVectorList.Last()) < 3f && _hecks.Count >= 1)
                {
                    KHBuddy.IPCChannel.Broadcast(new MoveWestMessage());
                    return new NukeState();
                }
            }

            if (KHBuddy.SideSelection.EastAndWest == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32())
            {
                if (KHBuddy._doingEast && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.KHEastVectorList.Last()) < 3f && _hecks.Count >= 1)
                {
                    _counterVec = 0;
                    return new NukeState();
                }

                if (KHBuddy._doingWest && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.KHWestVectorList.Last()) < 3f && _hecks.Count >= 1)
                {
                    _counterVec = 0;
                    return new NukeState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            _lastFollowTime = Time.NormalTime;
            Chat.WriteLine("PullState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("PullState::OnStateExit");
        }

        public void Tick()
        {
            if (KHBuddy.SideSelection.Beach == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Profession == Profession.Enforcer
                    && (Time.NormalTime > KHBuddy._timer + 580f || !KHBuddy._init))
                {
                    if (!MovementController.Instance.IsNavigating)
                    {
                        Spell.Find(270786, out Spell mongobuff);

                        if (_counterVec >= 0 && _counterVec < Constants.KHBeachVectorList.Count)
                        {
                            if (_counterVec <= 13)
                            {
                                _counterVec++;
                                MovementController.Instance.SetMovement(MovementAction.Update);
                                MovementController.Instance.SetDestination(Constants.KHBeachVectorList[_counterVec]);
                                _lastFollowTime = Time.NormalTime;
                            }

                            if (_counterVec >= 14 && mongobuff.IsReady && !Spell.HasPendingCast && Time.NormalTime - _lastFollowTime > 4.8
                                 && _counterVec < Constants.KHBeachVectorList.Count)
                            {
                                mongobuff.Cast();
                                CastedMongo = true;
                            }
                            else if (_counterVec >= 14 && CastedMongo == true && !mongobuff.IsReady && Time.NormalTime - _lastFollowTime > 4.8
                                 && _counterVec < Constants.KHBeachVectorList.Count)
                            {
                                _counterVec++;
                                MovementController.Instance.SetMovement(MovementAction.Update);
                                MovementController.Instance.SetDestination(Constants.KHBeachVectorList[_counterVec]);
                                _lastFollowTime = Time.NormalTime;
                                CastedMongo = false;
                            }
                        }
                    }
                }
            }
            if (KHBuddy.SideSelection.East == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Profession == Profession.Enforcer
                    && (Time.NormalTime > KHBuddy._timer + 580f || !KHBuddy._init))
                {
                    if (!MovementController.Instance.IsNavigating)
                    {
                        Spell.Find(270786, out Spell mongobuff);

                        if (_counterVec >= 0 && _counterVec < Constants.KHEastVectorList.Count)
                        {
                            if (_counterVec <= 13)
                            {
                                _counterVec++;
                                MovementController.Instance.SetMovement(MovementAction.Update);
                                MovementController.Instance.SetDestination(Constants.KHEastVectorList[_counterVec]);
                                _lastFollowTime = Time.NormalTime;
                            }

                            if (_counterVec >= 14 && mongobuff.IsReady && !Spell.HasPendingCast && Time.NormalTime - _lastFollowTime > 4.8
                                 && _counterVec < Constants.KHEastVectorList.Count)
                            {
                                mongobuff.Cast();
                                CastedMongo = true;
                            }
                            else if (_counterVec >= 14 && CastedMongo == true && !mongobuff.IsReady && Time.NormalTime - _lastFollowTime > 4.8
                                 && _counterVec < Constants.KHEastVectorList.Count)
                            {
                                _counterVec++;
                                MovementController.Instance.SetMovement(MovementAction.Update);
                                MovementController.Instance.SetDestination(Constants.KHEastVectorList[_counterVec]);
                                _lastFollowTime = Time.NormalTime;
                                CastedMongo = false;
                            }
                        }
                    }
                }
            }

            if (KHBuddy.SideSelection.West == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Profession == Profession.Enforcer
                    && (Time.NormalTime > KHBuddy._timer + 580f || !KHBuddy._init))
                {
                    if (!MovementController.Instance.IsNavigating)
                    {
                        Spell.Find(270786, out Spell mongobuff);

                        if (_counterVec >= 0 && _counterVec < Constants.KHEastVectorList.Count)
                        {
                            if (_counterVec <= 24)
                            {
                                _counterVec++;
                                MovementController.Instance.SetMovement(MovementAction.Update);
                                MovementController.Instance.SetDestination(Constants.KHWestVectorList[_counterVec]);
                                _lastFollowTime = Time.NormalTime;
                            }

                            if (_counterVec >= 25 && mongobuff.IsReady && !Spell.HasPendingCast && Time.NormalTime - _lastFollowTime > 4.8
                                 && _counterVec < Constants.KHWestVectorList.Count)
                            {
                                mongobuff.Cast();
                                CastedMongo = true;
                            }
                            else if (_counterVec >= 25 && CastedMongo == true && !mongobuff.IsReady && Time.NormalTime - _lastFollowTime > 4.8
                                 && _counterVec < Constants.KHWestVectorList.Count)
                            {
                                _counterVec++;
                                MovementController.Instance.SetMovement(MovementAction.Update);
                                MovementController.Instance.SetDestination(Constants.KHWestVectorList[_counterVec]);
                                _lastFollowTime = Time.NormalTime;
                                CastedMongo = false;
                            }
                        }
                    }
                }
            }

            if (KHBuddy.SideSelection.EastAndWest == (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32())
            {
                if (KHBuddy._doingEast && !KHBuddy._doingWest)
                {
                    if (DynelManager.LocalPlayer.Profession == Profession.Enforcer
                        && (Time.NormalTime > KHBuddy._timer + 120f || !KHBuddy._init))
                    {
                        if (!MovementController.Instance.IsNavigating)
                        {
                            Spell.Find(270786, out Spell mongobuff);

                            if (_counterVec >= 0 && _counterVec < Constants.KHEastVectorList.Count)
                            {
                                if (_counterVec <= 13)
                                {
                                    _counterVec++;
                                    MovementController.Instance.SetMovement(MovementAction.Update);
                                    MovementController.Instance.SetDestination(Constants.KHEastVectorList[_counterVec]);
                                    _lastFollowTime = Time.NormalTime;
                                }

                                if (_counterVec >= 14 && mongobuff.IsReady && !Spell.HasPendingCast && Time.NormalTime - _lastFollowTime > 4.8
                                    && _counterVec < Constants.KHEastVectorList.Count)
                                {
                                    mongobuff.Cast();
                                    CastedMongo = true;
                                }
                                else if (_counterVec >= 14 && CastedMongo == true && !mongobuff.IsReady && Time.NormalTime - _lastFollowTime > 4.8
                                    && _counterVec < Constants.KHEastVectorList.Count)
                                {
                                    _counterVec++;
                                    MovementController.Instance.SetMovement(MovementAction.Update);
                                    MovementController.Instance.SetDestination(Constants.KHEastVectorList[_counterVec]);
                                    _lastFollowTime = Time.NormalTime;
                                    CastedMongo = false;
                                }
                            }
                        }
                    }
                }

                if (!KHBuddy._doingEast && KHBuddy._doingWest)
                {
                    if (DynelManager.LocalPlayer.Profession == Profession.Enforcer
                        && Time.NormalTime > KHBuddy._timer + 10f)
                    {
                        if (!MovementController.Instance.IsNavigating)
                        {
                            Spell.Find(270786, out Spell mongobuff);

                            if (_counterVec >= 0 && _counterVec < Constants.KHWestVectorList.Count)
                            {
                                if (_counterVec <= 24)
                                {
                                    _counterVec++;
                                    MovementController.Instance.SetMovement(MovementAction.Update);
                                    MovementController.Instance.SetDestination(Constants.KHWestVectorList[_counterVec]);
                                    _lastFollowTime = Time.NormalTime;
                                }

                                if (_counterVec >= 25 && mongobuff.IsReady && !Spell.HasPendingCast && Time.NormalTime - _lastFollowTime > 4.8
                                     && _counterVec < Constants.KHWestVectorList.Count)
                                {
                                    mongobuff.Cast();
                                    CastedMongo = true;
                                }
                                else if (_counterVec >= 25 && CastedMongo == true && !mongobuff.IsReady && Time.NormalTime - _lastFollowTime > 4.8
                                     && _counterVec < Constants.KHWestVectorList.Count)
                                {
                                    _counterVec++;
                                    MovementController.Instance.SetMovement(MovementAction.Update);
                                    MovementController.Instance.SetDestination(Constants.KHWestVectorList[_counterVec]);
                                    _lastFollowTime = Time.NormalTime;
                                    CastedMongo = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
