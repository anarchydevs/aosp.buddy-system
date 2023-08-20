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

                shipentrance = DynelManager.AllDynels.Where(c => c.Name == "Door").FirstOrDefault();

                if (shipentrance != null)
                {
                    float distance = DynelManager.LocalPlayer.Position.DistanceFrom(shipentrance.Position);

                    if (distance > 1 && !hasReachedEntrance)
                    {
                        // Move to ship entrance
                        MovementController.Instance.SetDestination(shipentrance.Position);
                        if (distance <= 1) // Assuming the Update method is called frequently
                        {
                            hasReachedEntrance = true;
                        }
                    }
                    else if (hasReachedEntrance)
                    {
                        // Randomly move around ship entrance
                        Random rand = new Random();
                        float xOffset = (float)(rand.NextDouble() - 0.5); // Between -0.5 and 0.5
                        float yOffset = (float)(rand.NextDouble() - 0.5); // Between -0.5 and 0.5

                        Vector3 randomizedPosition = new Vector3(
                            shipentrance.Position.X + xOffset,
                            shipentrance.Position.Y + yOffset,
                            shipentrance.Position.Z  // Keep the Z coordinate the same
                        );

                        MovementController.Instance.SetDestination(randomizedPosition);
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
    }
}