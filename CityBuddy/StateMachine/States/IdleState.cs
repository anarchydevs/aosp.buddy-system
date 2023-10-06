﻿using AOSharp.Common.GameData;
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

        public IState GetNextState()
        {

            if (CityBuddy._settings["Enable"].AsBool())
            {
                if (Playfield.ModelIdentity.Instance == CityBuddy.MontroyalCity
                    || Playfield.ModelIdentity.Instance == CityBuddy.SerenityIslands
                    || Playfield.ModelIdentity.Instance == CityBuddy.PlayadelDesierto)
                {
                        return new CityAttackState();
                }

                if (Playfield.IsDungeon)
                {
                    if (DynelManager.LocalPlayer.Room.Name == "AI_bossroom")
                        return new BossRoomState();
                    else
                    return new PathState();
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
