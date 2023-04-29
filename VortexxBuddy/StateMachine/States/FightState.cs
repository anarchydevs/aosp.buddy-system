using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading;
using SmokeLounge.AOtomation.Messaging.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using AOSharp.Core.Inventory;
using static VortexxBuddy.ImmunityState;
using System.Timers;

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

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId)
                return new IdleState();

            if (_desecratedSpirits == null
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

                if (_desecratedSpirits != null && _vortexx == null)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_desecratedSpirits);
                    }
                }

                //Attack and initial start
                if (_vortexx != null)
                {

                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                        DynelManager.LocalPlayer.Attack(_vortexx);

                    if (VortexxBuddy._settings["Immunity"].AsBool() && _releasedSpirit != null)
                        if (DynelManager.LocalPlayer.FightingTarget != null
                           && DynelManager.LocalPlayer.FightingTarget.Name == _vortexx.Name)
                        {
                            DynelManager.LocalPlayer.StopAttack();
                        }

                    if (_releasedSpirit != null && !VortexxBuddy._clearToEnter &&
                        _releasedSpirit.Position.DistanceFrom(Constants._bluePodium) < 3)
                    {
                        VortexxBuddy._clearToEnter = true;
                        Chat.WriteLine(" clear to enter");
                    }

                    if(_vortexx.Buffs.Contains(VortexxBuddy.Nanos.CrystalBossShapeChanger))
                        Chat.WriteLine($"{Targeting.TargetChar?.HealthPercent}");

                    //Pathing to Notum
                    if (_vortexx.HealthPercent == 64.0 && !DynelManager.LocalPlayer.Buffs.Contains(VortexxBuddy.Nanos.NanoInfusion))
                    {
                            foreach (Dynel notum in _notum.Where(c => c.DistanceFrom(DynelManager.LocalPlayer) > 1f))
                            {
                                if (notum != null)
                                    VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(notum.Position);
                                
                            }
                        
                    }
                }

            }
        }
    }
}