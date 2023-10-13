using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using KHBuddy.IPCMessages;
using System.Collections.Generic;
using System.Linq;

namespace KHBuddy
{
    public class PullState : IState
    {
        public static int _counterVec = 0;

        public static bool CastedMongo = false;

        private double _lastFollowTime = Time.NormalTime;

        private double _eastTimer;
        private double _westTimer;
        private double _beachTimer;

        private Spell mongoSlam;
        private Spell mongoDemolish;
        private Spell mongo;

        public static IPCChannel IPCChannel { get; private set; }

        public IState GetNextState()
        {
            List<SimpleChar> _hecks = DynelManager.NPCs
                .Where(x => (x.Name.Contains("Heckler") || x.Name.Contains("Voracious"))
                    && x.DistanceFrom(DynelManager.LocalPlayer) <= 10f
                    && x.IsAlive && x.IsInLineOfSight)
                .ToList();

            var currentSelection = (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32();

            if (_hecks.Count >= 1)
            {

                switch (currentSelection)
                {
                    case KHBuddy.SideSelection.Beach:
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.KHBeachVectorList.Last()) < 3f)
                            return new NukeState();
                        break;

                    case KHBuddy.SideSelection.East:
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.KHEastVectorList.Last()) < 3f)
                        {
                            KHBuddy.IPCChannel.Broadcast(new MoveEastMessage());
                            return new NukeState();
                        }
                        break;

                    case KHBuddy.SideSelection.West:
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.KHWestVectorList.Last()) < 3f)
                        {
                            KHBuddy.IPCChannel.Broadcast(new MoveWestMessage());
                            return new NukeState();
                        }
                        break;

                    case KHBuddy.SideSelection.EastAndWest:
                        if (KHBuddy._doingEast && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.KHEastVectorList.Last()) < 3f)
                        {
                            //_counterVec = 0;
                            return new NukeState();
                        }

                        if (KHBuddy._doingWest && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.KHWestVectorList.Last()) < 3f)
                        {
                           // _counterVec = 0;
                            return new NukeState();
                        }
                        break;
                }
            }

            return null;
        }


        public void OnStateEnter()
        {
            _lastFollowTime = Time.NormalTime;
            Chat.WriteLine("Pull State");
        }

        public void OnStateExit()
        {
            // Chat.WriteLine("PullState::OnStateExit");
        }

        public void Tick()
        {
            if (DynelManager.LocalPlayer.Profession == Profession.Enforcer)
            {
                Spell.Find(270786, out mongoDemolish);
                Spell.Find(100198, out mongoSlam);

                SetMongoBasedOnHealth();

                var currentSelection = (KHBuddy.SideSelection)KHBuddy._settings["SideSelection"].AsInt32();

                if (currentSelection == KHBuddy.SideSelection.EastAndWest)
                {
                    if (KHBuddy._doingEast)
                    {
                        if (HandleMovementAndCasting(KHBuddy.SideSelection.East)) // If East is done
                        {
                            KHBuddy._doingEast = false;
                            KHBuddy._doingWest = true;

                            Chat.WriteLine("Pull state, setting _doingWest true");
                        }
                    }
                    else if (KHBuddy._doingWest)
                    {
                        if (HandleMovementAndCasting(KHBuddy.SideSelection.West)) // If West is done
                        {
                            KHBuddy._doingWest = false;
                            KHBuddy._doingEast = true; // Loop back to East
                            Chat.WriteLine("Pull state, setting _doingEast true");
                        }
                    }
                }
                else
                {
                    HandleMovementAndCasting(currentSelection);
                }
            }
        }

        private void SetMongoBasedOnHealth()
        {
            SimpleChar localPlayer = new SimpleChar(DynelManager.LocalPlayer.Pointer);
            float healthPercentage = localPlayer.HealthPercent;


            if (healthPercentage > 80 && !localPlayer.Buffs.Contains(270786))
            {
                mongo = mongoSlam;
            }
            else
            {
                mongo = mongoDemolish;
            }
        }

        private bool ShouldStartSequence(KHBuddy.SideSelection selection)
        {
            double targetTimer = 0.0;

            switch (selection)
            {
                case KHBuddy.SideSelection.Beach:
                    targetTimer = _beachTimer;
                    break;
                case KHBuddy.SideSelection.East:
                    targetTimer = _eastTimer;
                    break;
                case KHBuddy.SideSelection.West:
                    targetTimer = _westTimer;
                    break;
                default:
                    return false;
            }

            return !KHBuddy._init || Time.NormalTime > targetTimer;
        }
        private bool HandleMovementAndCasting(KHBuddy.SideSelection selection)
        {
            if (!ShouldStartSequence(selection))
                return false;

            if (MovementController.Instance.IsNavigating)
                return false;

            List<Vector3> vectorList;

            switch (selection)
            {
                case KHBuddy.SideSelection.Beach:
                    vectorList = Constants.KHBeachVectorList;
                    break;
                case KHBuddy.SideSelection.East:
                    vectorList = Constants.KHEastVectorList;
                    break;
                case KHBuddy.SideSelection.West:
                    vectorList = Constants.KHWestVectorList;
                    break;
                default:
                    return false;
            }

            int limit = GetLimitForSelection(selection);

            if (_counterVec <= limit)
            {
                MoveToNextDestination(vectorList);
            }
            else
            {
                HandleCasting(vectorList, limit + 1);
            }

            if (_counterVec >= vectorList.Count)
            {
                _counterVec = 0; // Reset counter for next sequence
                //Chat.WriteLine($"{selection} sequence complete");
                // Reset timer here when the sequence is complete.
                switch (selection)
                {
                    case KHBuddy.SideSelection.Beach:
                        _beachTimer = Time.NormalTime + 580.0;
                        //Chat.WriteLine($"Setting {_beachTimer} for Beach");
                        break;
                    case KHBuddy.SideSelection.East:
                        _eastTimer = Time.NormalTime + 580.0;
                        //Chat.WriteLine($"Setting {_eastTimer} for East");
                        break;
                    case KHBuddy.SideSelection.West:
                        _westTimer = Time.NormalTime + 580.0;
                        //Chat.WriteLine($"Setting {_westTimer} for West");
                        break;
                }
                Chat.WriteLine($"Timer reset for {selection}");
                return true; // Movement and casting for this side is complete
            }

            return false;
        }

        private int GetLimitForSelection(KHBuddy.SideSelection selection)
        {
            switch (selection)
            {
                case KHBuddy.SideSelection.Beach:
                case KHBuddy.SideSelection.East:
                    return 13;
                case KHBuddy.SideSelection.West:
                    return 24;
                default:
                    return 0;
            }
        }

        private void MoveToNextDestination(List<Vector3> vectorList)
        {
            _counterVec++;
            MovementController.Instance.SetMovement(MovementAction.Update);
            MovementController.Instance.SetDestination(vectorList[_counterVec]);
            _lastFollowTime = Time.NormalTime;
        }

        private void HandleCasting(List<Vector3> vectorList, int startIdx)
        {
            if (_counterVec >= startIdx && mongoSlam.IsReady && mongoDemolish.IsReady && !Spell.HasPendingCast && Time.NormalTime - _lastFollowTime > 4.8)
            {
                mongo.Cast();
                CastedMongo = true;
            }
            else if (_counterVec >= startIdx && CastedMongo && !mongoSlam.IsReady && !mongoDemolish.IsReady && Time.NormalTime - _lastFollowTime > 4.8)
            {
                MoveToNextDestination(vectorList);
                MovementController.Instance.SetMovement(MovementAction.JumpStart);
                CastedMongo = false;
            }
        }
    }
}
