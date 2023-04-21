using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class FightBossState : IState
    {
        private static SimpleChar _aune;
        private static Corpse _auneCorpse;

        private static bool _init = false;
        private static double _time;
        private static double _mistCycle;

        public IState GetNextState()
        {
            _aune = DynelManager.NPCs
               .Where(c => c.Health > 0
                   && c.Name.Contains("Ground Chief Aune"))
               .FirstOrDefault();

            _auneCorpse = DynelManager.Corpses
               .Where(c => c.Name.Contains("Ground Chief Aune"))
               .FirstOrDefault();

            List<Dynel> _mists = DynelManager.AllDynels
                .Where(c => c.Name.Contains("Notum Irregularity"))
                .OrderBy(c => c.Position.DistanceFrom(new Vector3(285.1f, 133.4f, 229.1f)))
                .ToList();

            if (_aune != null)
            {
                if (_aune.Buffs.Contains(273220) || DynelManager.LocalPlayer.Buffs.Contains(274101))
                    return new CircleState();
            }

            if (_auneCorpse != null && _mists.Count == 0)
            {
                if (!_init)
                {
                    _init = true;
                    _time = Time.NormalTime;
                }

                DynelManager.LocalPlayer.Position = _auneCorpse.Position;
                MovementController.Instance.SetMovement(MovementAction.Update);

                if (_init && Time.NormalTime > _time + 29f)
                {
                    return new ReformState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine($"FightBossState::OnStateEnter");

            _mistCycle = Time.NormalTime;
        }

        public void OnStateExit()
        {
            Chat.WriteLine("FightBossState::OnStateExit");
            DynelManager.LocalPlayer.StopAttack();
            _init = false;
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            _aune = DynelManager.NPCs
               .Where(c => c.Health > 0
                   && c.Name.Contains("Ground Chief Aune"))
               .FirstOrDefault();

            List<Dynel> _mists = DynelManager.AllDynels
                .Where(c => c.Name.Contains("Notum Irregularity"))
                .OrderBy(c => c.Position.DistanceFrom(new Vector3(285.1f, 133.4f, 229.1f)))
                .ToList();

            if (_aune != null)
            {
                if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending
                    && DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) < 30f)
                    DynelManager.LocalPlayer.Attack(_aune);

                if (_mists.Count == 0 && DynelManager.LocalPlayer.Position.DistanceFrom(_aune.Position) > 1f)
                {
                    DynelManager.LocalPlayer.Position = _aune.Position;
                    MovementController.Instance.SetMovement(MovementAction.Update);
                }
            }

            if (_mists.Count > 0
                && Time.NormalTime > _mistCycle + 3f)
            {
                _mistCycle = Time.NormalTime;

                foreach (Dynel mist in _mists.Where(c => c.DistanceFrom(DynelManager.LocalPlayer) > 1f))
                {
                    DynelManager.LocalPlayer.Position = mist.Position;
                    MovementController.Instance.SetMovement(MovementAction.Update);
                }
            }
        }
    }
}