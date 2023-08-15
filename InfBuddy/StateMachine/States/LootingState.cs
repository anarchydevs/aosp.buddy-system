using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InfBuddy
{
    public class LootingState : IState
    {
        private static bool _initCorpse = false;
        public static bool _missionsLoaded = false;

        private SimpleChar _target;

        private Corpse _corpse;

        private static Vector3 _corpsePos = Vector3.Zero;

        private double looting;

        private string previousErrorMessage = string.Empty;

        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId)
            {
                if (_corpse == null) //|| _initCorpse)
                    return new IdleState();

                if (!Extensions.IsNull(_target))
                    return new IdleState();

                //if (Extensions.CanExit(_missionsLoaded) || Extensions.IsClear())
                //    return new ExitMissionState();
            }

            if (Playfield.ModelIdentity.Instance != Constants.NewInfMissionId)
                return new IdleState();

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("Moving to corpse");
            //looting = Time.NormalTime;
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("Done looting");
            //_initCorpse = false;
            //_missionsLoaded = false;
        }

        public void Tick()
        {
            try
            {
                _corpse = DynelManager.Corpses
                    .Where(c => c.Name.Contains("Remains of "))
                    .FirstOrDefault();

                if (Game.IsZoning || _corpse == null) { return; }

                if (_corpse != null)//Path to corpse
                {
                    _corpsePos = (Vector3)_corpse?.Position;

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpsePos) > 5f)
                        InfBuddy.NavMeshMovementController.SetNavMeshDestination((Vector3)_corpse?.Position);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    previousErrorMessage = errorMessage;
                }
            }
        }
        private int GetLineNumber(Exception ex)
        {
            var lineNumber = 0;

            var lineMatch = Regex.Match(ex.StackTrace ?? "", @":line (\d+)$", RegexOptions.Multiline);

            if (lineMatch.Success)
                lineNumber = int.Parse(lineMatch.Groups[1].Value);

            return lineNumber;
        }
    }
}