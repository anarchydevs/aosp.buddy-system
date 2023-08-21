using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CityBuddy
{

    public class EnterState : IState
    {
        private bool hasReachedEntrance = false;
        public static bool NavGenSuccessful;

        private Dynel shipentrance;

        private State currentState = State.PathingToEntrance;
        private double forwardStartTime;
        private Vector3 initialPosition;

        public IState GetNextState()
        {

            if (!CityBuddy._settings["Toggle"].AsBool())
                return new IdleState();

            if (Playfield.IsDungeon && DynelManager.LocalPlayer.Room.Name == "AI_entrance")
            {
                if (CityAttackState.selectedMember != null)
                {
                    if (DynelManager.LocalPlayer.Identity == CityAttackState.selectedMember.Identity)
                    {
                        if (!NavGenSuccessful)
                            return new NavGenState();
                        if (NavGenSuccessful && !Team.Members.Any(c => c.Character == null))
                            return new PathState();
                    }

                    if (DynelManager.LocalPlayer.Identity != CityAttackState.selectedMember.Identity
                    && Team.Members.Any(c => c.Character != null))
                        return new PathState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("EnterState");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit EnterState");

        }

        public void Tick()
        {
            try
            {
                if (Game.IsZoning) { return; }

                Dynel shipEntrance = DynelManager.AllDynels.Where(c => c.Name == "Door").FirstOrDefault();

                if (shipEntrance != null)
                {
                    float distanceToDoor = DynelManager.LocalPlayer.Position.DistanceFrom(shipEntrance.Position);

                    switch (currentState)
                    {
                        case State.PathingToEntrance:
                            if (distanceToDoor > 1)
                            {
                                MovementController.Instance.SetDestination(shipEntrance.Position); // Path to door
                            }
                            else
                            {
                                MovementController.Instance.SetMovement(MovementAction.ForwardStop); // Stop moving
                                currentState = State.ArrivedAtEntrance;
                            }
                            break;

                        case State.ArrivedAtEntrance:
                            // Start moving forward for 2 seconds
                            MovementController.Instance.SetMovement(MovementAction.ForwardStart);
                            forwardStartTime = Time.NormalTime;
                            currentState = State.MovingForward;
                            break;

                        case State.MovingForward:
                            if (Time.NormalTime >= forwardStartTime + 2)
                            {
                                MovementController.Instance.SetMovement(MovementAction.ForwardStop); // Stop moving forward
                                currentState = State.PathingBackToEntrance;
                            }
                            break;

                        case State.PathingBackToEntrance:
                            // Path back to the entrance door
                            MovementController.Instance.SetDestination(shipEntrance.Position); // Path to door
                            currentState = State.PathingToEntrance; // Reset to initial state
                            break;
                    }
                }
            }

            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + CityBuddy.GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != CityBuddy.previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    CityBuddy.previousErrorMessage = errorMessage;
                }
            }
        }
        public enum State
        {
            PathingToEntrance,
            ArrivedAtEntrance,
            MovingForward,
            PathingBackToEntrance
        }
    }
}