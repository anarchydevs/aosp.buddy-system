﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MitaarBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20.0f
                && MitaarBuddy.Toggle == true 
                && Team.IsInTeam
                && Extensions.CanProceed()
                && MitaarBuddy._settings["Toggle"].AsBool())
            {
                return new EnterState();
            }

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) > 10.0f
                && MitaarBuddy.Toggle == true
                && Team.IsInTeam
                && Extensions.CanProceed()
                && MitaarBuddy._settings["Toggle"].AsBool())
            {
                return new DiedState();
            }

            if (Playfield.ModelIdentity.Instance == Constants.MitaarId)
            {
                if (MitaarBuddy.DifficultySelection.Easy == (MitaarBuddy.DifficultySelection)MitaarBuddy._settings["DifficultySelection"].AsInt32())
                    if (MitaarBuddy._died || (!MitaarBuddy._died && !Team.Members.Any(c => c.Character == null)))
                        return new EasyState();

                if (MitaarBuddy.DifficultySelection.Medium == (MitaarBuddy.DifficultySelection)MitaarBuddy._settings["DifficultySelection"].AsInt32())
                    if (MitaarBuddy._died || (!MitaarBuddy._died && !Team.Members.Any(c => c.Character == null)))
                        return new MediumState();

                if (MitaarBuddy.DifficultySelection.Hardcore == (MitaarBuddy.DifficultySelection)MitaarBuddy._settings["DifficultySelection"].AsInt32())
                    if (MitaarBuddy._died || (!MitaarBuddy._died && !Team.Members.Any(c => c.Character == null)))
                        return new HardcoreState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("IdleState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("IdleState::OnStateExit");
        }

        public void Tick()
        {
        }
    }
}