using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;
using static LeBuddy.LeBuddy;

namespace LeBuddy
{
    public class GrabMissionState : IState
    {
        private static bool _init = false;
        private double _timeToOpenDialog = 0;

        public IState GetNextState()
        {
            if (Game.IsZoning) { return null; }

            if (!LeBuddy._settings["Enable"].AsBool())
                return new IdleState();

            if (_settings["Enable"].AsBool())
            {
                if (Mission.List.Exists(x => x.DisplayName.Contains("Infiltrate the alien ships!"))
                    && Playfield.ModelIdentity.Instance == Constants.UnicornOutpost
                    && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._reformArea) < 10)
                {
                        return new IdleState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            _init = false;

            NpcDialog.AnswerListChanged += NpcDialog_AnswerListChanged;

            Chat.WriteLine("GrabMissionState");
            //NavGenState.DeleteNavMesh();
        }

        public void OnStateExit()
        {
            NpcDialog.AnswerListChanged -= NpcDialog_AnswerListChanged;

            LeBuddy.NavMeshMovementController.Halt();

            Chat.WriteLine("Exit GrabMissionState");
        }

        public void Tick()
        {
            if (Game.IsZoning || !Team.IsInTeam) { return; }

            Dynel _recruiter = DynelManager.NPCs
                .Where(c => c.Name == Constants.UnicornRecruiter)
                .FirstOrDefault();

            if (!Extensions.IsAtUnicornRecruiter())
            {
                NavMeshMovementController.SetNavMeshDestination(Constants._unicornRecruiter);
            }

            if (_recruiter != null && Extensions.IsAtUnicornRecruiter() && !_init)
            {
                _init = true;
                _timeToOpenDialog = Time.NormalTime + 1; // Schedule dialog open in 1 second
            }

            if (_timeToOpenDialog > 0 && Time.NormalTime >= _timeToOpenDialog)
            {
                NpcDialog.Open(_recruiter);
                _timeToOpenDialog = 0;
                _init = false;
            }

            if (Mission.List.Exists(x => x.DisplayName.Contains("Infiltrate the alien ships!"))
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._reformArea) > 5)
            {
                NavMeshMovementController.SetNavMeshDestination(Constants._reformArea);
            }
        }

        private void NpcDialog_AnswerListChanged(object s, Dictionary<int, string> options)
        {
            SimpleChar dialogNpc = DynelManager.GetDynel((Identity)s).Cast<SimpleChar>();

            if (dialogNpc.Name == Constants.UnicornRecruiter)
            {
                foreach (KeyValuePair<int, string> option in options)
                {
                    if (option.Value == "I want to visit the alien mothership!"
                       || (DifficultySelection.Easy == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && option.Value == "Take it easy on us.  We want to come back in one piece.")
                       || (DifficultySelection.Medium == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && option.Value == "We can handle our business.")
                       || (DifficultySelection.Hard == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && option.Value == "I want the mission Unicorns are too chicken to take."))
                        NpcDialog.SelectAnswer(dialogNpc.Identity, option.Key);
                }
            }
        }
    }
}
