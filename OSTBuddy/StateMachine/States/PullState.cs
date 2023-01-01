using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSTBuddy
{
    public class PullState : IState
    {
        public static bool SwitchingState = false;

        public static bool MoveToStartNorth = false;
        public static bool MoveToStartSouth = false;

        public static bool StartedSwitch = false;

        public static bool StartedSouthSwitch = false;
        public static bool StartedNorthSwitch = false;

        public static bool GettingAllSamePosition = false;



        public static bool CastedMongo = false;

        public static bool FirstWaypoint = false;

        public static int _counter;
        private static double _waitAtPos;

        private static int _mongoId = 0;

        public static Vector3 _nextWaypoint;

        public static IPCChannel IPCChannel { get; private set; }

        public IState GetNextState()
        {
            if (!OSTBuddy.Toggle)
            {
                MovementController.Instance.Halt();
                return new IdleState();
            }

            List<SimpleChar> mobs = DynelManager.NPCs
                .Where(x => x.DistanceFrom(DynelManager.LocalPlayer) <= 30f)
                .Where(x => x.IsAlive)
                .Where(x => x.IsInLineOfSight)
                .ToList();

            Spell.Find(_mongoId, out Spell mongobuff);

            if (SwitchingState == true && mobs.Count >= 1
                && !DynelManager.LocalPlayer.IsMoving)
            {
                mongobuff.Cast();
                SwitchingState = false;
                return new WaitState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            if (!FirstWaypoint)
            {
                List<Vector3> _waypointList = new List<Vector3>(OSTBuddy._waypoints);

                _counter = 1;
                MovementController.Instance.SetDestination(_waypointList.First());

                FirstWaypoint = true;
            }

            //Chat.WriteLine("PullState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("PullState::OnStateExit");
        }

        public void Tick()
        {
            if (OSTBuddy._waypoints.Count >= 1)
            {
                foreach (Vector3 pos in OSTBuddy._waypoints)
                {
                    Debug.DrawSphere(pos, 0.2f, DebuggingColor.White);
                }
            }

            Spell spell = Spell.List.FirstOrDefault(c => c.IsReady);

            List<Vector3> _waypointList = new List<Vector3>(OSTBuddy._waypoints);

            _nextWaypoint = _waypointList
                    .Where(c => _counter <= _waypointList.Count)
                    .Where(c => c == _waypointList.ElementAt(_counter))
                    .FirstOrDefault();

            if (OSTBuddy.MongoSelection.Demolish == (OSTBuddy.MongoSelection)OSTBuddy._settings["MongoSelection"].AsInt32())
                _mongoId = 270786;

            if (OSTBuddy.MongoSelection.Slam == (OSTBuddy.MongoSelection)OSTBuddy._settings["MongoSelection"].AsInt32())
                _mongoId = 100198;

            Spell.Find(_mongoId, out Spell mongobuff);

            if (!MovementController.Instance.IsNavigating && spell != null && !Spell.HasPendingCast)
            {
                if (_counter < _waypointList.Count - 1)
                {
                    if (mongobuff.IsReady && !Spell.HasPendingCast
                        && !DynelManager.LocalPlayer.IsMoving && CastedMongo == false)
                    {
                        mongobuff.Cast();
                        CastedMongo = true;
                        _waitAtPos = Time.NormalTime;
                    }

                    if (CastedMongo == true && Time.NormalTime - _waitAtPos > 5.8)
                    {
                        CastedMongo = false;
                        MovementController.Instance.SetDestination(_nextWaypoint);
                    }

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_waypointList.ElementAt(_counter)) < 1f && CastedMongo == true)
                    {
                        _waitAtPos = Time.NormalTime;
                        _counter++;
                        return;
                    }
                }
                else
                {
                    if (mongobuff.IsReady && !Spell.HasPendingCast
                        && !DynelManager.LocalPlayer.IsMoving && Time.NormalTime - _waitAtPos > 5.8)
                    {
                        _counter = 0;
                        MovementController.Instance.SetDestination(_nextWaypoint);
                        SwitchingState = true;
                    }
                }
            }
        }
    }
}
