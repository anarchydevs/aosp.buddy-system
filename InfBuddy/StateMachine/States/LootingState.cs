using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Linq;

namespace InfBuddy
{
    public class LootingState : IState
    {
        private SimpleChar _target;

        private Corpse _corpse;

        private static Vector3 _corpsePos = Vector3.Zero;

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId)
            {
                if (_corpse == null || !Extensions.IsNull(_target))
                {
                    return new IdleState();
                }
            }

            if (Playfield.ModelIdentity.Instance != Constants.NewInfMissionId)
            {
                return new IdleState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("Moving to corpse");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("Done looting");
        }

        public void Tick()
        {
            try
            {
                _corpse = DynelManager.Corpses
                    .Where(c => c.Name.Contains("Remains of "))
                    .FirstOrDefault();

                if (Game.IsZoning || _corpse == null) { return; }

                if (_corpse != null)
                {
                    _corpsePos = (Vector3)_corpse?.Position;

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpsePos) > 5f)
                    {
                        InfBuddy.NavMeshMovementController.SetNavMeshDestination(_corpsePos);
                    }   
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + InfBuddy.GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != InfBuddy.previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    InfBuddy.previousErrorMessage = errorMessage;
                }
            }
        }
    }
}