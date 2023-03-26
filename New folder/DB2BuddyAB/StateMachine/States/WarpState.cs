using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class WarpState : IState
    {
        private SimpleChar _target;

        public static bool _init = false;

        public static void DestroyInstance()
        {
            Player = new Vector3();
            AunePosition = new Vector3();
        }

        public static Vector3 Player { get; set; }
        public static Vector3 AunePosition { get; set; }

        public IState GetNextState()
        {
            if (_target != null || !DB2Buddy._settings["Toggle"].AsBool())
                return new FightState(_target);

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("ScanState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("ScanState::OnStateExit");
        }

        //public void Tick()
        //{
        //    if (Extensions.GetLeader(DB2Buddy.Leader) != null)
        //    {
               
        //        if (DB2Buddy._mob.Count >= 1)
        //        {
        //            if (DB2Buddy._mob.FirstOrDefault().Health == 0) { return; }

        //            _target = DB2Buddy._mob.FirstOrDefault();

        //            Chat.WriteLine($"Found target: {_target.Name}.");

        //            DynelManager.LocalPlayer.Position = _aunePosition;

        //        }
        //    }


            public void Tick()
            {

                SimpleChar aune = DynelManager.NPCs
                   .Where(c => c.Health > 0
                       && c.Name.Contains("Ground Chief Aune"))
                   .FirstOrDefault();

                SimpleChar tower = DynelManager.NPCs
                    .Where(c => c.Health > 0
                        && DynelManager.LocalPlayer.Position.DistanceFrom(c.Position) < 3f
                        && c.Name.Contains("Strange Xan Artifact"))
                    .FirstOrDefault();


                if (DynelManager.LocalPlayer.Position.DistanceFrom((Vector3)aune?.Position) < 3f
                    && DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                {
                    DynelManager.LocalPlayer.Attack(aune);
                }

                //Reflect shield
                if (aune?.Buffs.Contains(273220) == true)
                {
                    if (!_init)
                    {
                        _init = true;

                        if (DynelManager.LocalPlayer.FightingTarget != null)
                            DynelManager.LocalPlayer.StopAttack();

                        Task.Factory.StartNew(
                            async () =>
                            {
                                foreach (Vector3 _pos in _towerPositions)
                                {
                                    await Task.Delay(500);
                                    DynelManager.LocalPlayer.Position = _pos;
                                    MovementController.Instance.SetMovement(MovementAction.Update);
                                    await Task.Delay(500);
                                    if (tower != null && DynelManager.LocalPlayer.FightingTarget != null
                                        && DynelManager.LocalPlayer.IsAttackPending)
                                    {
                                        DynelManager.LocalPlayer.Attack(tower);
                                        _towerAttacked = true;
                                        return;
                                    }
                                }
                            });
                    }

                    if (_towerAttacked)
                    {
                        if (tower == null && DynelManager.LocalPlayer.FightingTarget == null
                                        && !DynelManager.LocalPlayer.IsAttackPending)
                        {
                            DynelManager.LocalPlayer.Position = (Vector3)aune?.Position;
                            MovementController.Instance.SetMovement(MovementAction.Update);
                            _towerAttacked = false;
                            _reflectShield = false;
                            _init = false;
                            return;
                        }
                    }
            }
        }

        public static List<Vector3> _towerPositions = new List<Vector3>()
        {
            new Vector3(294.2f, 135.3f, 199.0f),
            new Vector3(250.9f, 135.3f, 225.1f),
            new Vector3(277.0f, 135.3f, 267.1f),
            new Vector3(320.2f, 135.3f, 242.1f)
        };

    }
}
