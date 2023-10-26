using AOSharp.Core;
using AOSharp.Common.GameData;
using System.Diagnostics;
using System.Linq;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using System.Collections.Generic;
using System.Security.Policy;

namespace Shared
{
    public class TauntingTools
    {


        public static int _aggToolCounter = 0;
        public static int _attackTimeout = 0;

        public static void HandleTaunting(SimpleChar target)
        {
            if (_aggToolCounter >= 2)
            {
                if (_attackTimeout >= 1)
                {
                    _attackTimeout = 0;
                    _aggToolCounter = 0;
                    return;
                }

                _attackTimeout++;
                _aggToolCounter = 0;
            }

            Item item = null;

            if (Inventory.Find(83920, out item) ||  // Aggression Enhancer
                Inventory.Find(83919, out item) ||  // Aggression Multiplier
                Inventory.Find(151692, out item) ||  // Modified Aggression Enhancer (low)
                Inventory.Find(151693, out item) ||  // Modified Aggression Enhancer (High)
                Inventory.Find(152029, out item) || // Aggression Enhancer (Jealousy Augmented)
                Inventory.Find(152028, out item) || // Aggression Multiplier (Jealousy Augmented)
                Inventory.Find(244655, out item) || // Scorpio's Aim of Anger
                Inventory.Find(253186, out item) || // Codex of the Insulting Emerto (Low)
                Inventory.Find(253187, out item))   // Codex of the Insulting Emerto (High)
            {
                if (!Item.HasPendingUse && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology))
                {
                    item.Use(target, true);
                    _aggToolCounter++;
                }
            }
        }
    }

}