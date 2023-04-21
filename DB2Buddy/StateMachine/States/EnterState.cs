using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class EnterState : IState
    {
        private static bool _init = false;

        private static double _time;
        private static double _startTime;

        private static SimpleChar _aune;

        public IState GetNextState()
        {
            _aune = DynelManager.NPCs
               .Where(c => c.Health > 0
                   && c.Name.Contains("Ground Chief Aune"))
               .FirstOrDefault();

            if (_aune != null && DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) < 5f)
            {
                return new FightBossState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("EnterState::OnStateEnter");
            _time = Time.NormalTime;
            _startTime = Time.NormalTime;
            _init = true;
        }

        public void OnStateExit()
        {
            Chat.WriteLine("EnterState::OnStateExit");

            _init = false;
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Playfield.ModelIdentity.Instance == 570
                && Time.NormalTime > _time + 2f)
            {
                _time = Time.NormalTime;

                DB2Buddy.NavMeshMovementController.SetDestination(new Vector3(2121.8f, 0.4f, 2769.1f).Randomize(2f));
            }


            if (Playfield.ModelIdentity.Instance == 6055
                && Time.NormalTime > _startTime + 12f
                && _init)
            {
                if (!Team.Members.Any(c => c.Character == null))
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(285.1f, 133.4f, 229.1f)) > 1f)
                    {
                        DB2Buddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(285.1f, 133.4f, 229.1f));
                    }
                }
            }
        }
    }
}