using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ALBBuddy
{
    public class EnterSectorState : IState
    {
        private const int MinWait = 8;
        private const int MaxWait = 10;
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.S13Id)
            {
                if (ALBBuddy.ModeSelection.Leech == (ALBBuddy.ModeSelection)ALBBuddy._settings["ModeSelection"].AsInt32())
                    if (ALBBuddy._died || (!ALBBuddy._died && !Team.Members.Any(c => c.Character == null)))
                        return new LeechState();

                if (ALBBuddy.ModeSelection.Roam == (ALBBuddy.ModeSelection)ALBBuddy._settings["ModeSelection"].AsInt32())
                    if (ALBBuddy._died || (!ALBBuddy._died && !Team.Members.Any(c => c.Character == null)))
                        return new RoamState();

                if (ALBBuddy._died || (!ALBBuddy._died && !Team.Members.Any(c => c.Character == null)))
                    return new PatrolState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("EnterSectorState::OnStateEnter");

            if (DynelManager.LocalPlayer.Identity == ALBBuddy.Leader)
            {
                Task.Delay(2 * 1000).ContinueWith(x =>
                {
                    ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13EntrancePos);
                }, _cancellationToken.Token);
            }
            else if (ALBBuddy.ModeSelection.Leech == (ALBBuddy.ModeSelection)ALBBuddy._settings["ModeSelection"].AsInt32())
            {
                if (!ALBBuddy._settings["Merge"].AsBool())
                {
                    Task.Delay(5 * 1000).ContinueWith(x =>
                    {
                        ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13EntrancePos);
                    }, _cancellationToken.Token);
                }
                else
                {
                    Task.Delay(7 * 1000).ContinueWith(x =>
                    {
                        ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13EntrancePos);
                    }, _cancellationToken.Token);
                }
            }
            else
            {
                int randomWait = Extensions.Next(MinWait, MaxWait);
                Chat.WriteLine($"Idling for {randomWait} seconds..");

                Task.Delay(randomWait * 1000).ContinueWith(x =>
                {
                    ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.S13EntrancePos);
                }, _cancellationToken.Token);
            }
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("EnterSectorState::OnStateExit");

            _cancellationToken.Cancel();
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }
        }
    }
}