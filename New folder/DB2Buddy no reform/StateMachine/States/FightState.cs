using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Configuration;
using System.Linq;


namespace DB2Buddy
{
    public class FightState : IState
    {
        public static double _time;

        private SimpleChar _target;

        public static bool _init = false;
        public static bool _initLol = false;
        public static bool _initStart = false;
        public static bool _initTower = false;
        public static bool _initCorpse = false;
        public static bool IsLeader = false;
        public static bool _repeat = false;

        private double _fightStartTime;
        public const double FightTimeout = 45f;

        public FightState(SimpleChar target)
        {
            _target = target;
        }

        public IState GetNextState()
        {


            if (Extensions.IsNull(_target)
                || Time.NormalTime > _fightStartTime + FightTimeout) { }


            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine($"FightState::OnStateEnter");

            _fightStartTime = Time.NormalTime;
            DB2Buddy.NavMeshMovementController.Halt();
        }

        public void OnStateExit()
        {
            Chat.WriteLine("FightState::OnStateExit");

        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }


            SimpleChar aune = DynelManager.NPCs
                .Where(c => c.Health > 0 && c.Name.Contains("Ground Chief Aune")).FirstOrDefault();

            Dynel tower = DynelManager.AllDynels
                .Where(c => c.Identity.Type != IdentityType.Corpse
                    && c.Name.Contains("Strange Xan Artifact"))
                .FirstOrDefault();

            SimpleChar towerChar = DynelManager.NPCs
                .Where(c => c.Health == 0
                    && c.Name.Contains("Strange Xan Artifact"))
                .FirstOrDefault();

            Dynel mist = DynelManager.AllDynels
                .Where(c => c.Name.Contains("Notum Irregularity"))
                .FirstOrDefault();

            Corpse _aune = DynelManager.Corpses.FirstOrDefault(c => c.Name.Contains("Aune"));




            //Outside db2

                if (Playfield.ModelIdentity.Instance == 570 && Team.IsInTeam && Time.NormalTime > _time + 2f)
                {
                     _time = Time.NormalTime;

                         DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._entrancePos);
                        //MovementController.Instance.SetPath(Constants._entrancePos);

                    if (_repeat)
                    {
                    _initCorpse = false;
                    _initStart = false;
                    _initLol = false;
                    _init = false;

                    //if (IsLeader)
                    //{
                    //    foreach (Identity identity in _teamCache)
                    //    {
                    //        Team.Invite(identity);
                    //    }
                    //}

                    _repeat = false;
                    }

                }


            //Inside db2
            if (Playfield.ModelIdentity.Instance == 6055 && !_init && !_initLol)
            {
                 _init = true;
            }

            if (Playfield.ModelIdentity.Instance == 6055
               && _init && Time.NormalTime > _time + 2f && !_initLol)

            if (Playfield.ModelIdentity.Instance == 6055 && Time.NormalTime > _time + 2f)
            {
                _time = Time.NormalTime;

                if (Team.Members.FirstOrDefault(c => c.Character == null) == null)
                {
                    _init = false;
                    _initLol = true;
                    MovementController.Instance.SetPath(Constants._pathToAune);
                }
            }

            if (mist != null && tower == null && _aune == null)
            {
                DynelManager.LocalPlayer.Position = (Vector3)mist?.Position;
                MovementController.Instance.SetMovement(MovementAction.Update);
            }

            if (_aune != null)
            {
                DynelManager.LocalPlayer.Position = (Vector3)_aune?.Position;
                MovementController.Instance.SetMovement(MovementAction.Update);

                //if (!_initCorpse && Team.IsInTeam)
                //{
                //    _initCorpse = true;

                //    //Task.Factory.StartNew(
                //    //    async () =>
                //    //    {
                //    //        foreach (Identity identity in Team.Members.Select(c => c.Identity))
                //    //        {
                //    //            if (!_teamCache.Contains(identity))
                //    //                _teamCache.Add(identity);
                //    //        }

                //    //        await Task.Delay(10000);
                //    //        Team.Leave();
                //    //        _repeat = true;
                //    //    });
                //}
            }

            if (aune != null && DynelManager.LocalPlayer.Position.DistanceFrom(aune.Position) >= 3f
                && mist == null
                && (tower == null
                    || towerChar != null)
                && _initStart)
            {
                DynelManager.LocalPlayer.Position = (Vector3)aune?.Position;
                MovementController.Instance.SetMovement(MovementAction.Update);
            }


            //Attack and initial start
            if (aune != null && DynelManager.LocalPlayer.Position.DistanceFrom(aune.Position) < 30f
                && !DynelManager.Players.Any(c => c.Buffs.Contains(274101))
                && aune.Buffs.Contains(273220) == false)
            {
                if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                    DynelManager.LocalPlayer.Attack(aune);

                if (DynelManager.LocalPlayer.Position.DistanceFrom(aune.Position) < 3f) ;
                    _initStart = true;
            }


            //Has buff stop and move to tower
            if (aune != null && aune.Buffs.Contains(273220) == true
                || DynelManager.Players.Any(c => c.Buffs.Contains(274101)))
            {
                if (DynelManager.LocalPlayer.FightingTarget != null
                    && DynelManager.LocalPlayer.FightingTarget.Name == aune.Name)
                {
                    DynelManager.LocalPlayer.StopAttack();
                }

                if (tower == null)
                {
                    DynelManager.LocalPlayer.Position = new Vector3(285.7f, 133.3f, 232.9f);
                    MovementController.Instance.SetMovement(MovementAction.Update);
                }

                if (tower != null)
                {
                    DynelManager.LocalPlayer.Position = tower.Position;
                    MovementController.Instance.SetMovement(MovementAction.Update);

                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(tower);
                    }
                }
            }
        }
    }
}
       