using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace InfBuddy
{
    public class DiedState : IState
    {
        private double _useCheck = Time.NormalTime;

        private bool _pathedOnce = false;
        private bool _usedKit = false;

        List<Vector3> _pathPandePlat = new List<Vector3>
        {
            new Vector3(172.9f, 25.6f, 62.3f),
            new Vector3(120.5f, 47.5f, 28.1f),
            new Vector3(111.6f, 46.4f, 20.0f)
        };

        List<Vector3> _pathToErgo = new List<Vector3>
        {
            new Vector3(2847.5f, 31.8f, 3461.5f),
            new Vector3(2824.9f, 33.0f, 3440.4f),
            new Vector3(2830.6f, 31.8f, 3423.7f),
            new Vector3(2797.6f, 25.4f, 3378.1f),
            new Vector3(2776.8f, 24.6f, 3329.9f),
            new Vector3(2739.3f, 24.6f, 3317.0f),
            new Vector3(2719.3f, 24.6f, 3326.9f)
        };

        List<Vector3> _appendPath = new List<Vector3>
        {
            new Vector3(120.9f, 47.2f, 30.2f),
            new Vector3(111.6f, 46.4f, 20.0f) // 98.2f, 46.4f, 10.8f
        };

        List<Vector3> _clanGPath = new List<Vector3>
        {
            new Vector3(354.6f, 119.6f, 397.1f),
            new Vector3(365.9f, 118.6f, 398.8f),
            new Vector3(386.0f, 115.0f, 414.5f)
        };

        public IState GetNextState()
        {
            //Edge case correction
            if (Playfield.ModelIdentity.Instance == Constants.InfernoId && MovementController.Instance.IsNavigating
                && DynelManager.LocalPlayer.HealthPercent >= 66)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.EntrancePos) <= 10f)
                {
                    if (Team.Members.Where(c => c.Character != null).ToList().Count == Team.Members.Count)
                    {
                        InfBuddy.NavMeshMovementController.Halt();
                        _pathedOnce = false;
                        _usedKit = false;

                        foreach (Mission mission in Mission.List)
                            if (mission.DisplayName.Contains("The Purification Ritual"))
                                mission.Delete();

                        return new ReformState();
                    }
                }
            }

            if (Playfield.ModelIdentity.Instance == Constants.NewInfMissionId)
            {
                if (InfBuddy.ModeSelection.Leech == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                    return new LeechState();

                if (InfBuddy.ModeSelection.Roam == (InfBuddy.ModeSelection)InfBuddy._settings["ModeSelection"].AsInt32())
                    return new RoamState();

                return new DefendSpiritState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("DiedState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("DiedState::OnStateExit");
        }

        public void Tick()
        {
            List<Dynel> Statue = DynelManager.AllDynels
                .Where(x => x.Name == "Unredeemed Garden Exit" || x.Name == "Redeemed Garden Exit")
                .ToList();

            if (Statue.Count() >= 1 && Time.NormalTime - _useCheck > 3
                && DynelManager.LocalPlayer.DistanceFrom(Statue.FirstOrDefault()) <= 7f
                && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment)
                && DynelManager.LocalPlayer.MovementState != MovementState.Sit
                && (Playfield.ModelIdentity.Instance == Constants.OmniPandeGId
                || Playfield.ModelIdentity.Instance == Constants.ClanPandeGId))
            {
                Statue.FirstOrDefault().Use();
                _useCheck = Time.NormalTime;
            }

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit && DynelManager.LocalPlayer.HealthPercent >= 66)
                MovementController.Instance.SetMovement(MovementAction.LeaveSit);

            if (Playfield.ModelIdentity.Instance == Constants.ClanPandeGId && !MovementController.Instance.IsNavigating
                && DynelManager.LocalPlayer.HealthPercent >= 66 && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
            {
                if (Statue.Count() >= 1 && DynelManager.LocalPlayer.DistanceFrom(Statue.FirstOrDefault()) > 5f)
                {
                    MovementController.Instance.SetPath(_clanGPath);
                }
            }

            if (Playfield.ModelIdentity.Instance == Constants.OmniPandeGId && !MovementController.Instance.IsNavigating
                && DynelManager.LocalPlayer.HealthPercent >= 66 && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
            {
                if (Statue.Count() >= 1 && DynelManager.LocalPlayer.DistanceFrom(Statue.FirstOrDefault()) > 5f)
                {
                    MovementController.Instance.SetDestination(new Vector3(462.0f, 40.0f, 446.4f));
                }
            }

            if (Playfield.ModelIdentity.Instance == Constants.PandePlatId &&
                DynelManager.LocalPlayer.HealthPercent < 66 && MovementController.Instance.IsNavigating
                && _usedKit == false)
            {
                MovementController.Instance.Halt();
                _usedKit = true;
                return;
            }

            if (Playfield.ModelIdentity.Instance == Constants.PandePlatId && !MovementController.Instance.IsNavigating
                 && _pathedOnce == true && DynelManager.LocalPlayer.HealthPercent >= 66 && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
            {
                MovementController.Instance.SetPath(_appendPath);
                _pathedOnce = false;
                _usedKit = false;
            }

            if (Playfield.ModelIdentity.Instance == Constants.PandePlatId && !MovementController.Instance.IsNavigating
                && _pathedOnce == false && DynelManager.LocalPlayer.HealthPercent >= 66 && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
            {
                MovementController.Instance.SetPath(_pathPandePlat);
                _pathedOnce = true;
            }

            if (Playfield.ModelIdentity.Instance == Constants.InfernoId && !MovementController.Instance.IsNavigating
                && DynelManager.LocalPlayer.HealthPercent >= 66)
            {
                InfBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.EntrancePos);
                InfBuddy.NavMeshMovementController.AppendDestination(Constants.EntranceFinalPos);
                _pathedOnce = false;
                _usedKit = false;
            }
        }
    }
}
