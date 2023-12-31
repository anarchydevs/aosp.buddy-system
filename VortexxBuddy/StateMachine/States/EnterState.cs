﻿using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using AOSharp.Common.GameData;
using System.Linq;
using System.Collections.Generic;
using System.Timers;
using System.Runtime.InteropServices;

namespace VortexxBuddy
{
    public class EnterState : IState
    {

        private static double _time;

        private const int MinWait = 3;
        private const int MaxWait = 5;
        

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.VortexxId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPodium) <5)
                return new FightState();

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId)
            {
                if (!Extensions.CanProceed())
                    return new IdleState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            if (Extensions.CanProceed())
            {
                //Chat.WriteLine("Entering");
                VortexxBuddy.VortexxCorpse = false;
            }
        }

        public void OnStateExit()
        {
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (!Team.IsInTeam && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._startPos) > 3)
                VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPos);

            if (Team.IsInTeam && Time.NormalTime > _time + 2f)
            {
                _time = Time.NormalTime;

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20)
                {
                    VortexxBuddy.NavMeshMovementController.SetDestination(Constants._entrance);
                    VortexxBuddy.NavMeshMovementController.AppendDestination(Constants._reneterPos);
                }
            }

            if (Playfield.ModelIdentity.Instance == Constants.VortexxId)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPodium) > 2f)
                    VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._centerPodium);
            }
        }
    }
}