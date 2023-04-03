using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ALBBuddy
{
    public class LeechState : IState
    {
        private static double _stuck;
        private static double _timeOut;

        private static bool _init = false;

        public IState GetNextState()
        {
            //if (Extensions.HasDied())
            //    return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.Albtraum
                && !Team.IsInTeam
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.ZoneOutPos) <= 10f)
                return new ReformState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("LeechState::OnStateEnter");

            DynelManager.LocalPlayer.Position = new Vector3(373.8, 39.2f, 97.9f);
        }

        public void OnStateExit()
        {
            Chat.WriteLine("LeechState::OnStateExit");
        }

        public void Tick()
        {
            if (!Team.IsInTeam || Game.IsZoning) { return; }

            foreach (TeamMember member in Team.Members)
            {
                if (!ReformState._teamCache.Contains(member.Identity))
                    ReformState._teamCache.Add(member.Identity);
            }

            if (Playfield.ModelIdentity.Instance == Constants.Inferno)
                ALBBuddy._ourPos = DynelManager.LocalPlayer.Position;

            if (!ALBBuddy._initMerge && ALBBuddy._settings["Merge"].AsBool())
            {
                if (!ALBBuddy._initMerge)
                    ALBBuddy._initMerge = true;

                ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.EndPos);
            }


            ALBBuddy._leader = Team.Members
                .Where(c => c.Character?.Health > 0
                    && c.Character?.IsValid == true
                    && c.IsLeader)
                .FirstOrDefault()?.Character;

            if (ALBBuddy._leader != null)
            {
               

                ALBBuddy._leaderPos = (Vector3)ALBBuddy._leader?.Position;

                //Reason: Edge correction
                //if (ALBBuddy.NavMeshMovementController.IsNavigating)
                //{
                //    if (DynelManager.LocalPlayer.Position.Distance2DFrom(new Vector3(169.5f, 36.0f, 164.3f)) > 15f)
                //        _init = false;

                //    if (Time.NormalTime > _timeOut + 10f && _init)
                //    {
                //        ALBBuddy.NavMeshMovementController.Halt();
                //        DynelManager.LocalPlayer.Position = new Vector3(ALBBuddy._leaderPos.X, 67.7f, ALBBuddy._leaderPos.Z);
                //    }

                //    if (DynelManager.LocalPlayer.Position.Distance2DFrom(new Vector3(169.5f, 36.0f, 164.3f)) <= 15f)
                //    {
                //        if (!_init)
                //        {
                //            _init = true;
                //            _timeOut = Time.NormalTime;
                //        }
                //    }
                //}

                if (DynelManager.LocalPlayer.Position.Distance2DFrom(ALBBuddy._leaderPos) > 2f
                    && DynelManager.LocalPlayer.Position.Distance2DFrom(Constants.EndPos) > 30f)
                    ALBBuddy.NavMeshMovementController.SetNavMeshDestination(new Vector3(ALBBuddy._leaderPos.X, 67.7f, ALBBuddy._leaderPos.Z));

            }
        }
    }
}