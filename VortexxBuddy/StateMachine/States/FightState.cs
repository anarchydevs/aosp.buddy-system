using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace VortexxBuddy
{
    public class FightState : IState
    {

        private static SimpleChar _vortexx;
        private static Corpse _vortexxCorpse;

        private static SimpleChar _desecratedSpirits;
        private static SimpleChar _releasedSpirit;

        public IState GetNextState()
        {
            //if (VortexxBuddy._settings["Immunity"].AsBool() && _releasedSpirit != null)
            //    return new ImmunityState();

                if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && !Team.IsInTeam
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20f)
                return new ReformState();

            if (Playfield.ModelIdentity.Instance == Constants.VortexxId
                && VortexxBuddy.VortexxCorpse
                && Extensions.CanProceed()
                && VortexxBuddy._settings["Farming"].AsBool())
                return new FarmingState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("FightState");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit FightState");
        }

        public void Tick()
        {
            if (!Team.IsInTeam || Game.IsZoning) { return; }

            foreach (TeamMember member in Team.Members)
            {
                if (!ReformState._teamCache.Contains(member.Identity))
                    ReformState._teamCache.Add(member.Identity);
            }

            if (Playfield.ModelIdentity.Instance == Constants.VortexxId)
            {

                _vortexx = DynelManager.NPCs
                 .Where(c => c.Health > 0
                  && c.Name.Contains("Ground Chief Vortexx")
                  && !c.Name.Contains("Remains of"))
                  .FirstOrDefault();

                _desecratedSpirits = DynelManager.NPCs
                   .Where(c => c.Health > 0
                           && c.Name.Contains("Desecrated Spirits"))
                       .FirstOrDefault();

                _releasedSpirit = DynelManager.NPCs
                    .Where(c => c.Health > 0
                           && c.Name.Contains("Released Spirit"))
                       .FirstOrDefault();

                _vortexxCorpse = DynelManager.Corpses
                  .Where(c => c.Name.Contains("Remains of Ground Chief Vortexx"))
                      .FirstOrDefault();

                List<Dynel> _notum = DynelManager.AllDynels
                  .Where(c => c.Name.Contains("Notum Erruption"))
                  .OrderBy(c => c.Position.DistanceFrom(Constants._centerPodium))
                  .ToList();

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPodium) > 5f
                    && !MovementController.Instance.IsNavigating)
                    VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._centerPodium);

                if (_vortexxCorpse != null)
                    VortexxBuddy.VortexxCorpse = true;

                //Attack and initial start
                if (_vortexx != null && DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                    DynelManager.LocalPlayer.Attack(_vortexx);

                if (_desecratedSpirits != null && _vortexx == null)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_desecratedSpirits);
                    }
                }

                if (VortexxBuddy._settings["Immunity"].AsBool() && _releasedSpirit != null)
                    if (DynelManager.LocalPlayer.FightingTarget != null
                       && DynelManager.LocalPlayer.FightingTarget.Name == _vortexx.Name)
                    {
                        DynelManager.LocalPlayer.StopAttack();
                    }

                //Pathing to Notum

                if (!MovementController.Instance.IsNavigating)
                { 
                    if (_notum.Count > 0  && _vortexx.Buffs.Contains(VortexxBuddy.Nanos.CrystalBossShapeChanger)
                    && !DynelManager.LocalPlayer.Buffs.Contains(VortexxBuddy.Nanos.NanoInfusion))
                    {
                        foreach (Dynel notum in _notum.Where(c => c.DistanceFrom(DynelManager.LocalPlayer) > 1f))
                                VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(notum.Position); 
                    }
                }
            }
        }
    }
}