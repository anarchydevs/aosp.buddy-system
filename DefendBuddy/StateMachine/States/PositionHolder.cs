using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;

namespace DefendBuddy
{
    public class PositionHolder
    {
        private readonly Vector3 _holdPos;
        private readonly float _HoldDist;
        private readonly int _entropy;

        public PositionHolder(Vector3 holdPos, float holdDist ,int entropy)
        {
            _holdPos = holdPos;
            _HoldDist = holdDist;
            _entropy = entropy;
        }

        public void HoldPosition()
        {
            if (!MovementController.Instance.IsNavigating && !IsNearDefenseSpot())
            {
                Vector3 randomHoldPos = _holdPos;
                randomHoldPos.AddRandomness(_entropy);

                MovementController.Instance.SetPath(new Path(randomHoldPos));
            }
        }

        private bool IsNearDefenseSpot()
        {
            return DynelManager.LocalPlayer.Position.DistanceFrom(Constants._posToDefend) < _HoldDist;
        }
    }
}
