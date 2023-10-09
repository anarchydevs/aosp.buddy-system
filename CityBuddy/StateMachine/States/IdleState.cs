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
        private Dynel shipentrance;

        public IState GetNextState()
        {

            shipentrance = DynelManager.AllDynels.Where(c => c.Name == "Door").FirstOrDefault();

            if (CityBuddy._settings["Enable"].AsBool())
            {
                var validPlayfields = new[]
                {
                    CityBuddy.MontroyalCity,
                    CityBuddy.SerenityIslands,
                    CityBuddy.PlayadelDesierto
                };

                if (validPlayfields.Contains(Playfield.ModelIdentity.Instance))
                {
                    if (shipentrance != null && CityBuddy._settings["Ship"].AsBool())
                    {
                        return new WaitForShipState();
                    }
                    else
                    {
                        return new CityAttackState();
                    }
                }

                if (Playfield.IsDungeon)
                {
                    if (DynelManager.LocalPlayer.Room.Name == "AI_bossroom")
                    {
                        return new BossRoomState();
                    }
                    else
                    {
                        return new PathState();
                    }
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("Idle state");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("Exit Idle state");
        }

        public void Tick()
        {
            
        }
    }
}
