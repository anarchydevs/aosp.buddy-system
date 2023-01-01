using AOSharp.Common.GameData;
using AOSharp.Core;
using System.Data;

namespace InfBuddy
{
    public class PositionHolder
    {
        private readonly Vector3 _holdPos;
        private readonly float _HoldDist;
        private readonly int _entropy;

        private double _movementTimer;

        public PositionHolder(Vector3 holdPos, float holdDist , int entropy)
        {
            _holdPos = holdPos;
            _HoldDist = holdDist;
            _entropy = entropy;
        }

        public void HoldPosition()
        {
            if (!InfBuddy.NavMeshMovementController.IsNavigating && !IsNearDefenseSpot())
            {
                Vector3 randomHoldPos = _holdPos;
                randomHoldPos.AddRandomness(_entropy);

                _movementTimer = Time.NormalTime;
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(randomHoldPos);
            }

            if (InfBuddy.NavMeshMovementController.IsNavigating
                && Time.NormalTime > _movementTimer + 20f)
            {
                _movementTimer = Time.NormalTime;

                InfBuddy.NavMeshMovementController.Halt();
                DynelManager.LocalPlayer.Position = new Vector3(DynelManager.LocalPlayer.Position.X, DynelManager.LocalPlayer.Position.Y, DynelManager.LocalPlayer.Position.Z + 4f);
            }
        }

        private bool IsNearDefenseSpot()
        {
            return DynelManager.LocalPlayer.Position.DistanceFrom(Constants.DefendPos) < _HoldDist;
        }
    }
}
