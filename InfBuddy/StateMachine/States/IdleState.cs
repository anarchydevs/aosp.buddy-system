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

                if (!Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                {
                    return new MoveToQuestGiverState();
                }

                if (Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                {
                    return new MoveToEntranceState();
                }
            }

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId)
            {
                if (Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                {
                    if (InfBuddy.ModeSelection.Normal == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                    {
                        Constants.DefendPos = new Vector3(165.6f, 2.2f, 186.4f);
                        return new DefendSpiritState();
                    }

                    if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                    {
                        //Constants.RoamPos = new Vector3(184.5f, 1.0f, 242.9f);
                        return new RoamState();
                    }

                    if (InfBuddy.ModeSelection.Leech == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                        return new LeechState();
                }

                if (!Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")) || Extensions.IsClear())
                    return new ExitMissionState();

                if (InfBuddy._settings["Looting"].AsBool()
                    && _corpse != null
                    && Extensions.IsNull(_target))
                    return new LootingState();
            }

            if (Playfield.ModelIdentity.Instance == Constants.ClanPandeGId || Playfield.ModelIdentity.Instance == Constants.OmniPandeGId)
                return new DiedState();

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                return new SitState();

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
