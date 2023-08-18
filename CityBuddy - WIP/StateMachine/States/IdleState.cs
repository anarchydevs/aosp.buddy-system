using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using CityBuddy.IPCMessages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CityBuddy
{
    public class IdleState : IState
    {
        

        //public static List<string> EntranceNames = new List<string>();

        public IState GetNextState()
        {

            if (CityBuddy._settings["Toggle"].AsBool() && CityBuddy.Toggle)
            {
                if (Playfield.ModelIdentity.Instance == CityBuddy.MontroyalCity
                    || Playfield.ModelIdentity.Instance == CityBuddy.SerenityIslands
                    || Playfield.ModelIdentity.Instance == CityBuddy.PlayadelDesierto)
                {
                    //CityBuddy.ParkPos = DynelManager.LocalPlayer.Position;

                    if (DynelManager.LocalPlayer.Identity == CityBuddy.Leader)
                        return new CityControllerState();
                    else if (DynelManager.LocalPlayer.Identity != CityBuddy.Leader)
                        return new CityAttackState();


                }
            }

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return new SitState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Idle state");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit Idle state");
        }

        public void Tick()
        {
            //if (Entrance != null)
            //{
            //    if (!EntranceNames.Contains(Entrance.Name))
            //    {
            //        EntranceNames.Add(Entrance.Name);
            //        Chat.WriteLine($"Added {Entrance.Name} to EntranceNames.");
            //    }
            //}
        }
    }
}
