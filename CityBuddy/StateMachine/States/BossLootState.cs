using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CityBuddy
{
    public class BossLootState : IState
    {
        private Dynel _downButton;

        private static Corpse _corpse;

        private Stopwatch _lootTimer;

        public bool _initCorpse = false;
        public bool _atCorpse = false;

        public IState GetNextState()
        {
            if (!CityBuddy._settings["Enable"].AsBool())
                return new IdleState();

            if (_initCorpse && _atCorpse)
            {
                if (Playfield.IsDungeon && DynelManager.LocalPlayer.Room.Name == "AI_bossroom")
                {
                    return new ButtonExitState();
                }

                if (Playfield.ModelIdentity.Instance == CityBuddy.MontroyalCity
                        || Playfield.ModelIdentity.Instance == CityBuddy.SerenityIslands
                        || Playfield.ModelIdentity.Instance == CityBuddy.PlayadelDesierto)
                {
                    return new WaitForShipState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            _atCorpse = false;
            _initCorpse = false;

            Chat.WriteLine("Boss loot state");
        }

        public void OnStateExit()
        {
            CityBuddy.NavMeshMovementController.Halt();

            _atCorpse = false;
            _initCorpse = false;

            Chat.WriteLine("Exit Boss loot state");
        }

        public void Tick()
        {
            try
            {
                if (Game.IsZoning || !Team.IsInTeam) { return; }

                _corpse = DynelManager.Corpses
                    .Where(c => c.Name.Contains("Fleet Admiral") || c.Name.Contains("General")
                    || c.Name.Contains("Recruitment Director"))
               .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
               .FirstOrDefault();

                if (_corpse != null)
                {
                    if (!_atCorpse)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(_corpse.Position) < 2)
                        {
                            _atCorpse = true;
                        }
                        else
                        {
                            MovementController.Instance.SetDestination(_corpse.Position);
                        }
                    }
                    if (!_initCorpse && _atCorpse)
                    {
                        // Initialize the Stopwatch the first time you get to the corpse
                        if (_lootTimer == null)
                        {
                            _lootTimer = Stopwatch.StartNew();
                            //Chat.WriteLine("Pause for looting, 30 sec");
                        }

                        // Check the elapsed time
                        if (_lootTimer.ElapsedMilliseconds >= 30000)
                        {
                            // Reset and stop the Stopwatch
                            _lootTimer.Reset();

                            //Chat.WriteLine("Done looting");
                            _initCorpse = true;

                            // Dispose of the Stopwatch if you don't need it anymore
                            _lootTimer = null;
                        }
                    }

                    
                }

                else //if (_corpse == null)
                {

                    _initCorpse = true;
                    _atCorpse = true;
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
