using AOSharp.Core;
using AOSharp.Core.Movement;
using System;
using System.Linq;

namespace CityBuddy
{
    public class AttackState : IState
    {
        public IState GetNextState()
        {
            if (CityBuddy.gameTime > CityBuddy.cloakTime && CityBuddy.gameTime > CityBuddy.endWave1)
            {
                CityBuddy.UsedCru = false;
                return new ToggleState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("AttackState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("AttackState::OnStateExit");
        }

        public void Tick()
        {
            CityBuddy.gameTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Time.NormalTime);


            Corpse anycorpse = DynelManager.Corpses
                .Where(c => !c.Name.Contains("General"))
                .FirstOrDefault();

            if (anycorpse != null)
            {
                CityBuddy.endWave1 = CityBuddy.gameTime.AddSeconds(420);
            }

            if (DynelManager.LocalPlayer.Position.DistanceFrom(CityBuddy.DefendPos) > 5f && MovementController.Instance.IsNavigating == false)
            {
                MovementController.Instance.SetDestination(CityBuddy.DefendPos);
            }
        }
    }
}
