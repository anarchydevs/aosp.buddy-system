using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;

namespace InfBuddy
{
    public class IdleState : IState
    {
        private SimpleChar _target;
        private static Corpse _corpse;

        public static bool _init = false;

        public IState GetNextState()
        {
            bool missionExists = Mission.List.Exists(m => m.DisplayName.Contains("The Purification Ritual"));

            _corpse = DynelManager.Corpses
                .Where(c => c.Name.Contains("Remains of "))
                .FirstOrDefault();

            if (!InfBuddy.Toggle)
                return null;

            if (Playfield.ModelIdentity.Instance == Constants.InfernoId)
            {
                if (!Team.IsInTeam)
                {
                    return new ReformState();
                }

                if (!missionExists)
                {
                    return new MoveToQuestGiverState();
                }
                else
                {
                    return new MoveToEntranceState();
                }
            }

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId)
            {
                if (missionExists)
                {
                    if (InfBuddy._settings["Leech"].AsBool())
                    {
                        return new LeechState();
                    }
                    else
                    {
                        if (DynelManager.NPCs.Any(c => c.Name == Constants.QuestStarterName))
                            return new MoveToQuestStarterState();

                        if (InfBuddy.ModeSelection.Normal == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                        {
                            return new DefendSpiritState();
                        }
                        else
                        {
                            return new RoamState();
                        }
                    }
                }

                if (Extensions.IsClear() || !missionExists)
                {
                    if (InfBuddy._settings["Looting"].AsBool() && _corpse != null)
                    {
                        return new LootingState();
                    }
                    else
                    {
                        return new ExitMissionState();
                    }
                }    
            }

            if (Playfield.ModelIdentity.Instance == Constants.ClanPandeGId || Playfield.ModelIdentity.Instance == Constants.OmniPandeGId)
                return new DiedState();


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
            if (Game.IsZoning || !Team.IsInTeam) { return; }

            if (Team.IsInTeam)
            {
                foreach (TeamMember member in Team.Members)
                {
                    if (!ReformState._teamCache.Contains(member.Identity))
                        ReformState._teamCache.Add(member.Identity);
                }
            }
        }
    }
}
