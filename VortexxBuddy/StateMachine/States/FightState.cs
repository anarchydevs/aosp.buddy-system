﻿using AOSharp.Common.GameData;
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
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using AOSharp.Core.IPC;
using VortexxBuddy.IPCMessages;

namespace VortexxBuddy
{
    public class FightState : IState
    {


        private static SimpleChar _vortexx;
        private static SimpleChar _releasedSpirit;
        private static SimpleChar _desecratedSpirits;

        private static Corpse _vortexxCorpse;
        private static Corpse _releasedSpiritCorpse;


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
            //Chat.WriteLine("FightState");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("Exit FightState");
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

                _releasedSpiritCorpse = DynelManager.Corpses
                  .Where(c => c.Name.Contains("Remains of Released Spirit"))
                      .FirstOrDefault();

                List<Dynel> _notum = DynelManager.AllDynels
                  .Where(c => c.Name.Contains("Notum Erruption"))
                  .OrderBy(c => c.Position.DistanceFrom(Constants._centerPodium))
                  .ToList();

                //return to center
                if (_vortexx != null || _desecratedSpirits != null)
                { 
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPodium) > 5f
                    && !MovementController.Instance.IsNavigating)
                        VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._centerPodium); 
                }

                //Attack and initial start
                if (_vortexx != null)
                {
                    // attacking vortexx
                    if (DynelManager.LocalPlayer.FightingTarget == null 
                        && !DynelManager.LocalPlayer.IsAttackPending
                        && !MovementController.Instance.IsNavigating)
                        DynelManager.LocalPlayer.Attack(_vortexx);

                   
                    //if (_vortexx.HealthPercent < 63 && _vortexx.HealthPercent > 62)
                    //    VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(Constants._redPodium);

                    //if (_notum != null && !DynelManager.LocalPlayer.Buffs.Contains(VortexxBuddy.Nanos.NanoInfusion))
                    //{
                    //    if (_vortexx.HealthPercent < 44 && _vortexx.HealthPercent > 42)
                    //        foreach (Dynel notum in _notum.Where(c => c.DistanceFrom(DynelManager.LocalPlayer) > 1f))
                    //        {
                    //            VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(notum.Position);
                    //        }
                    //    if (_vortexx.HealthPercent < 29 && _vortexx.HealthPercent > 28)
                    //        foreach (Dynel notum in _notum.Where(c => c.DistanceFrom(DynelManager.LocalPlayer) > 1f))
                    //        {
                    //            VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(notum.Position);
                    //        }
                    //    if (_vortexx.HealthPercent < 19 && _vortexx.HealthPercent > 18)
                    //        foreach (Dynel notum in _notum.Where(c => c.DistanceFrom(DynelManager.LocalPlayer) > 1f))
                    //        {
                    //            VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(notum.Position);
                    //        }
                    //    if (_vortexx.HealthPercent < 14 && _vortexx.HealthPercent > 13)
                    //        foreach (Dynel notum in _notum.Where(c => c.DistanceFrom(DynelManager.LocalPlayer) > 1f))
                    //        {
                    //            VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(notum.Position);
                    //        }
                    //    if (_vortexx.HealthPercent < 7 && _vortexx.HealthPercent > 5) // missed
                    //        foreach (Dynel notum in _notum.Where(c => c.DistanceFrom(DynelManager.LocalPlayer) > 1f))
                    //        {
                    //            VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(notum.Position);
                    //        }
                    //}

                    if (_notum != null)
                    {
                        if (!DynelManager.LocalPlayer.Buffs.Contains(VortexxBuddy.Nanos.NanoInfusion)
                            || DynelManager.LocalPlayer.Buffs.Find(VortexxBuddy.Nanos.NanoInfusion, out Buff antiFear) && antiFear.RemainingTime < 2)
                        {
                            foreach (Dynel notum in _notum.Where(c => c.DistanceFrom(DynelManager.LocalPlayer) > 1f))
                            {
                                VortexxBuddy.NavMeshMovementController.SetNavMeshDestination(notum.Position);
                            }
                        }
                    }
                   
                }

                if (_releasedSpiritCorpse != null)
                {
                    //Get immune
                    if (VortexxBuddy._settings["Immunity"].AsBool())
                        if (DynelManager.LocalPlayer.FightingTarget != null
                           && DynelManager.LocalPlayer.FightingTarget.Name == _vortexx.Name)
                        {
                            DynelManager.LocalPlayer.StopAttack();
                        }

                    //Not Immune
                    if (!VortexxBuddy._clearToEnter &&
                        _releasedSpiritCorpse.Position.DistanceFrom(Constants._bluePodium) < 5)
                    {
                        //Chat.WriteLine("Clear to enter");
                        VortexxBuddy.IPCChannel.Broadcast(new EnterMessage());
                        VortexxBuddy._clearToEnter = true;
                    }
                }

                //Attacking adds
                if (_desecratedSpirits != null && _vortexx == null)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null 
                        && !DynelManager.LocalPlayer.IsAttackPending
                        && !MovementController.Instance.IsNavigating)
                    {
                        DynelManager.LocalPlayer.Attack(_desecratedSpirits);
                    }
                }

                //Toggle for farming
                if (_vortexxCorpse != null)
                {
                    VortexxBuddy.VortexxCorpse = true;
                    VortexxBuddy._clearToEnter = false;
                }
            }
        }
    }
}