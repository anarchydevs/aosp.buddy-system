using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;

namespace ALBBuddy
{
    public class PatrolState : IState
    {
        private SimpleChar _target;

        public IState GetNextState()
        {


            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.EndPos) <= 5f
                && Team.IsInTeam && !ALBBuddy.NavMeshMovementController.IsNavigating)
                Team.Disband();

            if (Playfield.ModelIdentity.Instance == Constants.Inferno && !Team.IsInTeam)
                return new ReformState();

            if (_target != null)
                return new FightState(_target);

            return null;
        }

        public void OnStateEnter()
        {

            Chat.WriteLine("Pathing");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Stop Pathing");
        }

        private void HandleScan()
        {


            SimpleChar mob = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.IsInLineOfSight
                    && c.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 60f)
                .OrderBy(c => c.HealthPercent)
                .ThenBy(c => c.MaxHealth)
                .ThenBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .FirstOrDefault(c => !Constants._ignores.Contains(c.Name));

            if (mob != null)
            {
                _target = mob;
                Chat.WriteLine($"Found target: {_target.Name}");
            }

            else if (!Team.Members.Any(c => c.Character == null)
                    && !Team.Members.Where(c => c.Character != null
                       && (c.Character.HealthPercent < 66 || c.Character.NanoPercent < 66
                            || c.Character.Position.DistanceFrom(DynelManager.LocalPlayer.Position) > 10f))
                       .Any()
                    && !Spell.HasPendingCast
                    && DynelManager.LocalPlayer.MovementState != MovementState.Sit 
                    && DynelManager.LocalPlayer.Position.DistanceFrom(Constants.EndPos) > 5f)
            {
                if (DynelManager.LocalPlayer.Identity == ALBBuddy.Leader)
                {
                    if (!ALBBuddy._passedStartPos && !ALBBuddy._passedFirstPos && !ALBBuddy._passedSecondPos && !ALBBuddy._passedThirdPos
                        && !ALBBuddy._passedForthPos && !ALBBuddy._passedFifthPos && !ALBBuddy._passedSixthPos && !ALBBuddy._passedSeventhPos
                        && !ALBBuddy._passedEighthPos && !ALBBuddy._passedNinethPos && !ALBBuddy._passedTenthPos && !ALBBuddy._passedLastPos)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.StartPos) < 5f)
                            ALBBuddy._passedStartPos = true;
                        else
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.StartPos);
                    }
                    else if (ALBBuddy._passedStartPos && !ALBBuddy._passedFirstPos && !ALBBuddy._passedSecondPos && !ALBBuddy._passedThirdPos
                        && !ALBBuddy._passedForthPos && !ALBBuddy._passedFifthPos && !ALBBuddy._passedSixthPos && !ALBBuddy._passedSeventhPos
                        && !ALBBuddy._passedEighthPos && !ALBBuddy._passedNinethPos && !ALBBuddy._passedTenthPos && !ALBBuddy._passedLastPos)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.FirstPos) < 5f)
                            ALBBuddy._passedFirstPos = true;
                        else
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.FirstPos);
                    }
                    else if (ALBBuddy._passedStartPos && ALBBuddy._passedFirstPos && !ALBBuddy._passedSecondPos && !ALBBuddy._passedThirdPos
                        && !ALBBuddy._passedForthPos && !ALBBuddy._passedFifthPos && !ALBBuddy._passedSixthPos && !ALBBuddy._passedSeventhPos
                        && !ALBBuddy._passedEighthPos && !ALBBuddy._passedNinethPos && !ALBBuddy._passedTenthPos && !ALBBuddy._passedLastPos)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.SecondPos) < 5f)
                            ALBBuddy._passedSecondPos = true;
                        else
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.SecondPos);
                    }
                    else if (ALBBuddy._passedStartPos && ALBBuddy._passedFirstPos && ALBBuddy._passedSecondPos && !ALBBuddy._passedThirdPos
                        && !ALBBuddy._passedForthPos && !ALBBuddy._passedFifthPos && !ALBBuddy._passedSixthPos && !ALBBuddy._passedSeventhPos
                        && !ALBBuddy._passedEighthPos && !ALBBuddy._passedNinethPos && !ALBBuddy._passedTenthPos && !ALBBuddy._passedLastPos)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.ThirdPos) < 5f)
                            ALBBuddy._passedThirdPos = true;
                        else
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.ThirdPos);
                    }
                    else if (ALBBuddy._passedStartPos && ALBBuddy._passedFirstPos && ALBBuddy._passedSecondPos && ALBBuddy._passedThirdPos
                        && !ALBBuddy._passedForthPos && !ALBBuddy._passedFifthPos && !ALBBuddy._passedSixthPos && !ALBBuddy._passedSeventhPos
                        && !ALBBuddy._passedEighthPos && !ALBBuddy._passedNinethPos && !ALBBuddy._passedTenthPos && !ALBBuddy._passedLastPos)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.ForthPos) < 5f)
                            ALBBuddy._passedForthPos = true;
                        else
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.ForthPos);
                    }
                    else if (ALBBuddy._passedStartPos && ALBBuddy._passedFirstPos && ALBBuddy._passedSecondPos && ALBBuddy._passedThirdPos
                        && ALBBuddy._passedForthPos && !ALBBuddy._passedFifthPos && !ALBBuddy._passedSixthPos && !ALBBuddy._passedSeventhPos
                        && !ALBBuddy._passedEighthPos && !ALBBuddy._passedNinethPos && !ALBBuddy._passedTenthPos && !ALBBuddy._passedLastPos)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.FifthPos) < 5f)
                            ALBBuddy._passedFifthPos = true;
                        else
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.FifthPos);
                    }
                    else if (ALBBuddy._passedStartPos && ALBBuddy._passedFirstPos && ALBBuddy._passedSecondPos && ALBBuddy._passedThirdPos
                        && ALBBuddy._passedForthPos && ALBBuddy._passedFifthPos && !ALBBuddy._passedSixthPos && !ALBBuddy._passedSeventhPos
                        && !ALBBuddy._passedEighthPos && !ALBBuddy._passedNinethPos && !ALBBuddy._passedTenthPos && !ALBBuddy._passedLastPos)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.SixthPos) < 5f)
                            ALBBuddy._passedSixthPos = true;
                        else
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.SixthPos);
                    }
                    else if (ALBBuddy._passedStartPos && ALBBuddy._passedFirstPos && ALBBuddy._passedSecondPos && ALBBuddy._passedThirdPos
                        && ALBBuddy._passedForthPos && ALBBuddy._passedFifthPos && ALBBuddy._passedSixthPos && !ALBBuddy._passedSeventhPos
                        && !ALBBuddy._passedEighthPos && !ALBBuddy._passedNinethPos && !ALBBuddy._passedTenthPos && !ALBBuddy._passedLastPos)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.SeventhPos) < 5f)
                            ALBBuddy._passedSeventhPos = true;
                        else
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.SeventhPos);
                    }
                    else if (ALBBuddy._passedStartPos && ALBBuddy._passedFirstPos && ALBBuddy._passedSecondPos && ALBBuddy._passedThirdPos
                        && ALBBuddy._passedForthPos && ALBBuddy._passedFifthPos && ALBBuddy._passedSixthPos && ALBBuddy._passedSeventhPos
                        && !ALBBuddy._passedEighthPos && !ALBBuddy._passedNinethPos && !ALBBuddy._passedTenthPos && !ALBBuddy._passedLastPos)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.EighthPos) < 5f)
                            ALBBuddy._passedEighthPos = true;
                        else
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.EighthPos);
                    }
                    else if (ALBBuddy._passedStartPos && ALBBuddy._passedFirstPos && ALBBuddy._passedSecondPos && ALBBuddy._passedThirdPos
                        && ALBBuddy._passedForthPos && ALBBuddy._passedFifthPos && ALBBuddy._passedSixthPos && ALBBuddy._passedSeventhPos
                        && ALBBuddy._passedEighthPos && !ALBBuddy._passedNinethPos && !ALBBuddy._passedTenthPos && !ALBBuddy._passedLastPos)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.NinethPos) < 5f)
                            ALBBuddy._passedNinethPos = true;
                        else
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.NinethPos);
                    }
                    else if (ALBBuddy._passedStartPos && ALBBuddy._passedFirstPos && ALBBuddy._passedSecondPos && ALBBuddy._passedThirdPos
                        && ALBBuddy._passedForthPos && ALBBuddy._passedFifthPos && ALBBuddy._passedSixthPos && ALBBuddy._passedSeventhPos
                        && ALBBuddy._passedEighthPos && ALBBuddy._passedNinethPos && !ALBBuddy._passedTenthPos && !ALBBuddy._passedLastPos)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.TenthPos) < 5f)
                            ALBBuddy._passedTenthPos = true;
                        else
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.TenthPos);
                    }
                    else if (ALBBuddy._passedStartPos && ALBBuddy._passedFirstPos && ALBBuddy._passedSecondPos && ALBBuddy._passedThirdPos
                        && ALBBuddy._passedForthPos && ALBBuddy._passedFifthPos && ALBBuddy._passedSixthPos && ALBBuddy._passedSeventhPos
                        && ALBBuddy._passedEighthPos && ALBBuddy._passedNinethPos && ALBBuddy._passedTenthPos && !ALBBuddy._passedLastPos)
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.LastPos) < 5f)
                            ALBBuddy._passedLastPos = true;
                        else
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.LastPos);
                    }
                    else
                        ALBBuddy.NavMeshMovementController.SetNavMeshDestination(Constants.EndPos);
                }
            }
            //if (!DynelManager.LocalPlayer.IsMoving
            //    && DynelManager.LocalPlayer.FightingTarget == null
            //        && !DynelManager.LocalPlayer.IsAttacking
            //        && !DynelManager.LocalPlayer.IsAttackPending)
            //{
            //    ALBBuddy.NavMeshMovementController.SetMovement(MovementAction.JumpStart);
            //}
        }

        public void Tick()
        {

            if (!Team.IsInTeam || Game.IsZoning) { return; }

            
                foreach (TeamMember member in Team.Members)
                {
                    if (!ReformState._teamCache.Contains(member.Identity))
                        ReformState._teamCache.Add(member.Identity);
                }

                if (Playfield.ModelIdentity.Instance == Constants.Albtraum)
                    ALBBuddy._ourPos = DynelManager.LocalPlayer.Position;

                if (DynelManager.LocalPlayer.Identity != ALBBuddy.Leader)
                {
                    ALBBuddy._leader = Team.Members
                        .Where(c => c.Character?.Health > 0
                            && c.Character?.IsValid == true
                            && c.IsLeader)
                        .FirstOrDefault()?.Character;

                    if (ALBBuddy._leader != null)
                    {

                        ALBBuddy._leaderPos = (Vector3)ALBBuddy._leader?.Position;

                        if (ALBBuddy._leader?.FightingTarget != null)
                        {
                            SimpleChar targetMob = DynelManager.NPCs
                                .Where(c => c.Health > 0
                                    && c.Identity == (Identity)ALBBuddy._leader?.FightingTarget?.Identity)
                                .FirstOrDefault(c => !Constants._ignores.Contains(c.Name));

                            if (DynelManager.LocalPlayer.Position.DistanceFrom(ALBBuddy._leaderPos) > 7f
                                && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
                                ALBBuddy.NavMeshMovementController.SetNavMeshDestination(ALBBuddy._leaderPos);

                            if (targetMob != null)
                            {
                                _target = targetMob;
 
                            }
                        }
                        else
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(ALBBuddy._leaderPos) > 7f
                            && !Spell.HasPendingCast
                            && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
                            ALBBuddy.NavMeshMovementController.SetNavMeshDestination(ALBBuddy._leaderPos);
                    }
                    else
                        HandleScan();
                }
                else
                    HandleScan();
            
        }
    }
}