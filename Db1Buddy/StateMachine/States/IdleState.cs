﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;

namespace Db1Buddy
{
    public class IdleState : IState
    {

        private static Corpse _mikkelsenCorpse;
        public IState GetNextState()
        {
            if (Playfield.ModelIdentity.Instance == Constants.PWId
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 30f
                && Team.IsInTeam
                && Extensions.CanProceed()
                && Db1Buddy._settings["Toggle"].AsBool())
                return new EnterState();

            if (Playfield.ModelIdentity.Instance == Constants.DB1Id)
            {
                if (!Team.Members.Any(c => c.Character == null)
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._atDoor) < 20f)
                    return new StartState();

                if (!DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.ThriceBlessedbytheAncients)
                    && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._atDoor) > 40f)
                    return new GetBuffState();

                if (DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.ThriceBlessedbytheAncients)
                     && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._atDoor) > 40f)
                    return new FightState();

                if (Db1Buddy.MikkelsenCorpse
                   && Extensions.CanProceed()
                   && Db1Buddy._settings["Farming"].AsBool())
                    return new FarmingState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("IdleState");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit IdleState");
        }

        public void Tick()
        {
            _mikkelsenCorpse = DynelManager.Corpses
             .Where(c => c.Name.Contains("Remains of Ground Chief Mikkelsen"))
                 .FirstOrDefault();

        }
    }
}
