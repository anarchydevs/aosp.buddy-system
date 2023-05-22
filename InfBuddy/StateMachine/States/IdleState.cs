using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;

namespace InfBuddy
{
    public class IdleState : IState
    {
        public static bool _init = false;

        public IState GetNextState()
        {
            if (!InfBuddy.Toggle)
                return null;

            if (Playfield.ModelIdentity.Instance == Constants.InfernoId && !Team.IsInTeam)
            {
                return new ReformState();
            }

            if (Team.IsInTeam && Playfield.ModelIdentity.Instance == Constants.InfernoId && !Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
            {
                //foreach (Mission mission in Mission.List)
                //    if (mission.DisplayName.Contains("The Purification"))
                //        mission.Delete();

                return new MoveToQuestGiverState();
            }

            if (Team.IsInTeam && Playfield.ModelIdentity.Instance == Constants.InfernoId && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
            {
                return new MoveToEntranceState();
            }

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId && Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
            {
                if (InfBuddy.ModeSelection.Normal == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                {
                    Constants.DefendPos = new Vector3(165.6f, 2.2f, 186.4f);
                    return new DefendSpiritState();
                }

                if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                {
                    Constants.RoamPos = new Vector3(184.5f, 1.0f, 242.9f);
                    return new RoamState();
                }

                if (InfBuddy.ModeSelection.Leech == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                    return new LeechState();
            }

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId && !Mission.List.Exists(x => x.DisplayName.Contains("The Purification Ri")))
                return new ExitMissionState();

            if(Playfield.ModelIdentity.Instance == Constants.NewInfMissionId && Extensions.IsClear())
                return new ExitMissionState();

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
        }
    }
}
