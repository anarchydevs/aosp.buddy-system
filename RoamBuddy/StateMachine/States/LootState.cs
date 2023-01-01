using AOSharp.Core;
using AOSharp.Core.Movement;
using System.Collections.Generic;
using System.Linq;

namespace RoamBuddy
{
    public class LootState : IState
    {
        private double _waitToLoot = Time.NormalTime;
        private double _timeOut = Time.NormalTime;

        public static List<Corpse> corpsesToLoot = new List<Corpse>();

        public IState GetNextState()
        {
            if (Time.NormalTime - _timeOut > 7 && MovementController.Instance.IsNavigating
                && RoamBuddy._settings["Toggle"].AsBool())
                return new RoamState();

            if (RoamBuddy._settings["Toggle"].AsBool()
                && corpsesToLoot?.Count == 0 && Time.NormalTime - _waitToLoot > 2)
                return new RoamState();

            return null;
        }

        public void OnStateEnter()
        {
            _waitToLoot = Time.NormalTime;
            _timeOut = Time.NormalTime;

            //Chat.WriteLine("LootState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("LootState::OnStateExit");
        }

        public void Tick()
        {
            corpsesToLoot = DynelManager.Corpses
                    .Where(c => c.DistanceFrom(DynelManager.LocalPlayer) <= RoamBuddy.ScanRange)
                    .ToList();

            if (corpsesToLoot?.Count == 0) { return; }

            foreach (Corpse corpse in corpsesToLoot)
            {
                if (!MovementController.Instance.IsNavigating && DynelManager.LocalPlayer.Position.DistanceFrom(corpse.Position) >= 5)
                    MovementController.Instance.SetDestination(corpse.Position);

                if (MovementController.Instance.IsNavigating && DynelManager.LocalPlayer.Position.DistanceFrom(corpse.Position) < 5)
                {
                    MovementController.Instance.Halt();

                    _waitToLoot = Time.NormalTime;
                }
            }
        }
    }
}
