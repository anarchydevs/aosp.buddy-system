using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
            _desecratedSpirits = DynelManager.NPCs
                  .Where(c => c.Health > 0
                          && c.Name.Contains("Desecrated Spirit"))
                      .FirstOrDefault();

            _releasedSpirit = DynelManager.NPCs
                   .Where(c => c.Health > 0
                          && c.Name.Contains("Released Spirit"))
                      .FirstOrDefault();

            if (VortexxBuddy._settings["Immunity"].AsBool() && _releasedSpirit != null)
                return new ImmunityState();

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && !Team.IsInTeam
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 20f)
                return new ReformState();

            if (Playfield.ModelIdentity.Instance == Constants.VortexxId
                && _desecratedSpirits == null
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
                  && c.Name.Contains("Ground Chief Vortexx"))
                  .FirstOrDefault();

                _desecratedSpirits = DynelManager.NPCs
                   .Where(c => c.Health > 0
                           && c.Name.Contains("Desecrated Spirit"))
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

                if (_desecratedSpirits != null && _vortexxCorpse != null)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_desecratedSpirits);
                    }
                }

                //Attack and initial start
                if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                    DynelManager.LocalPlayer.Attack(_vortexx);

                if (VortexxBuddy._settings["Immunity"].AsBool() && _releasedSpirit != null)
                    if (DynelManager.LocalPlayer.FightingTarget != null
                       && DynelManager.LocalPlayer.FightingTarget.Name == _vortexx.Name)
                    {
                        DynelManager.LocalPlayer.StopAttack();
                    }

                //Pathing to Notum
                if (_vortexx.Buffs.Contains(VortexxBuddy.Nanos.CrystalBossShapeChanger)
                && !DynelManager.LocalPlayer.Buffs.Contains(VortexxBuddy.Nanos.NanoInfusion))
                {
                    foreach (Dynel notum in _notum.Where(c => c.DistanceFrom(DynelManager.LocalPlayer) > 1f))
                    {
                        if (notum != null)
                        VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(notum.Position);

                        if (notum == null)
                            return;
                        //VortexxBuddy.NavMeshMovementController.Halt();
                    }
                
                    

                }

            }
        }
    }
}