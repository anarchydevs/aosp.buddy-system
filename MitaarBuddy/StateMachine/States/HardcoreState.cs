﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace MitaarBuddy
{
    public class HardcoreState : IState
    {

        private static SimpleChar _sinuh;
        private static Corpse _sinuhCorpse;

        private static SimpleChar _alienCoccoon;

        private static SimpleChar _xanSpirits;
        private static SimpleChar _greenXanSpirit;
        private static SimpleChar _redXanSpirit;
        private static SimpleChar _blueXanSpirit;
        private static SimpleChar _yellowXanSpirit;

        public IState GetNextState()
        {

            _alienCoccoon = DynelManager.NPCs
               .Where(c => c.Health > 0
                       && c.Name.Contains("Alien Coccoon"))
                   .FirstOrDefault();

            _xanSpirits = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.Name.Contains("Xan Spirit"))
                    .FirstOrDefault();

            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && !Team.IsInTeam
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) <= 10f)
                return new ReformState();

            if (MitaarBuddy.SinuhCorpse
                 && _xanSpirits == null
                 && _alienCoccoon == null
                 && Extensions.CanProceed()
                 && MitaarBuddy._settings["Farming"].AsBool())
                return new FarmingState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Starting Hardcore Parkour");

        }

        public void OnStateExit()
        {
            Chat.WriteLine("Hardcore Parkour over");
        }

        public void Tick()
        {
            if (!Team.IsInTeam || Game.IsZoning) { return; }

            foreach (TeamMember member in Team.Members)
            {
                if (!ReformState._teamCache.Contains(member.Identity))
                    ReformState._teamCache.Add(member.Identity);
            }

            if (Playfield.ModelIdentity.Instance == Constants.MitaarId)

            {
                _sinuh = DynelManager.NPCs
               .Where(c => c.Health > 0
                && c.Name.Contains("Technomaster Sinuh")
                && !c.Name.Contains("Remains of"))
                .FirstOrDefault();

                _alienCoccoon = DynelManager.NPCs
                   .Where(c => c.Health > 0
                           && c.Name.Contains("Alien Coccoon"))
                       .FirstOrDefault();

                _xanSpirits = DynelManager.NPCs
                    .Where(c => c.Health > 0
                        && c.Name.Contains("Xan Spirit"))
                        .FirstOrDefault();

                _redXanSpirit = DynelManager.NPCs
                    .Where(c => c.Health > 0
                        && c.Name.Contains("Xan Spirit")
                        && c.Buffs.Contains(MitaarBuddy.SpiritNanos.BlessingofTheBlood))
                        .FirstOrDefault();

                _blueXanSpirit = DynelManager.NPCs
                    .Where(c => c.Health > 0
                        && c.Name.Contains("Xan Spirit")
                        && c.Buffs.Contains(MitaarBuddy.SpiritNanos.BlessingofTheSource))
                        .FirstOrDefault();

                _greenXanSpirit = DynelManager.NPCs
                   .Where(c => c.Health > 0
                       && c.Name.Contains("Xan Spirit")
                       && c.Buffs.Contains(MitaarBuddy.SpiritNanos.BlessingofTheOutsider))
                       .FirstOrDefault();

                _yellowXanSpirit = DynelManager.NPCs
                    .Where(c => c.Health > 0
                        && c.Name.Contains("Xan Spirit")
                        && c.Buffs.Contains(MitaarBuddy.SpiritNanos.BlessingofTheLight))
                        .FirstOrDefault();

                _sinuhCorpse = DynelManager.Corpses
                  .Where(c => c.Name.Contains("Remains of Technomaster Sinuh"))
                      .FirstOrDefault();

                if (_sinuhCorpse != null)
                    MitaarBuddy.SinuhCorpse = true;

                //Attack and initial start
                if (_sinuh != null && _alienCoccoon == null)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                        DynelManager.LocalPlayer.Attack(_sinuh);
                }

                if (_sinuh != null && _alienCoccoon != null)
                {
                    if (DynelManager.LocalPlayer.FightingTarget != null
                        && DynelManager.LocalPlayer.FightingTarget.Name == _sinuh.Name)
                    {
                        DynelManager.LocalPlayer.StopAttack();
                    }
                }

                if (_alienCoccoon != null)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_alienCoccoon);
                    }
                }

                //Pathing to spirits
                if (_xanSpirits != null && !MovementController.Instance.IsNavigating)
                {
                    // Define the spirits and their corresponding podiums and buffs
                    (SimpleChar spirit, Vector3 podium, int buff)[] spiritInfo =
                    {
                        (_redXanSpirit, Constants._redPodium, MitaarBuddy.SpiritNanos.BlessingofTheBlood),
                        (_blueXanSpirit, Constants._bluePodium, MitaarBuddy.SpiritNanos.BlessingofTheSource),
                        (_greenXanSpirit, Constants._greenPodium, MitaarBuddy.SpiritNanos.BlessingofTheOutsider),
                        (_yellowXanSpirit, Constants._yellowPodium, MitaarBuddy.SpiritNanos.BlessingofTheLight)
                    };

                    foreach (var (spirit, podium, buff) in spiritInfo)
                    {
                        if (spirit != null)
                        {
                            var hasBuff = DynelManager.LocalPlayer.Buffs.Contains(buff);
                            var spiritBuff = spirit.Buffs.FirstOrDefault(b => b.Id == buff);
                            var playerBuffRemainingTime = DynelManager.LocalPlayer.Buffs.FirstOrDefault(b => b.Id == buff)?.RemainingTime ?? int.MaxValue;
                            var spiritBuffRemainingTime = spiritBuff?.RemainingTime ?? int.MaxValue;

                            if (!hasBuff || playerBuffRemainingTime < 5 || spiritBuffRemainingTime < 5)
                            {
                                MovementController.Instance.SetDestination(podium);
                                break; // Exit the loop after setting the destination for the first eligible spirit
                            }
                        }
                    }
                }

                //if (_xanSpirits != null && !MovementController.Instance.IsNavigating)
                //{
                //    if (_redXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._redPodium) > 0.9f
                //        && !DynelManager.LocalPlayer.Buffs.Contains(MitaarBuddy.SpiritNanos.BlessingofTheBlood))
                //        MovementController.Instance.SetDestination(Constants._redPodium);
                //    else if (_redXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._redPodium) > 0.9f
                //        && DynelManager.LocalPlayer.Buffs.Find(MitaarBuddy.SpiritNanos.BlessingofTheBlood, out Buff redbuff) && redbuff.RemainingTime < 5)
                //        MovementController.Instance.SetDestination(Constants._redPodium);

                //    if (_blueXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._bluePodium) > 0.9f
                //        && !DynelManager.LocalPlayer.Buffs.Contains(MitaarBuddy.SpiritNanos.BlessingofTheSource))
                //        MovementController.Instance.SetDestination(Constants._bluePodium);
                //    else if (_blueXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._bluePodium) > 0.9f
                //        && DynelManager.LocalPlayer.Buffs.Find(MitaarBuddy.SpiritNanos.BlessingofTheSource, out Buff bluebuff) && bluebuff.RemainingTime < 4)
                //        MovementController.Instance.SetDestination(Constants._bluePodium);

                //    if (_greenXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._greenPodium) > 0.9f
                //        && !DynelManager.LocalPlayer.Buffs.Contains(MitaarBuddy.SpiritNanos.BlessingofTheOutsider))
                //        MovementController.Instance.SetDestination(Constants._greenPodium);
                //    else if (_greenXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._greenPodium) > 0.9f
                //       && DynelManager.LocalPlayer.Buffs.Find(MitaarBuddy.SpiritNanos.BlessingofTheOutsider, out Buff greenbuff) && greenbuff.RemainingTime < 3)
                //        MovementController.Instance.SetDestination(Constants._greenPodium);

                //    if (_yellowXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._yellowPodium) > 0.9f
                //        && !DynelManager.LocalPlayer.Buffs.Contains(MitaarBuddy.SpiritNanos.BlessingofTheLight))
                //        MovementController.Instance.SetDestination(Constants._yellowPodium);
                //    else if (_yellowXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._yellowPodium) > 0.9f
                //       && DynelManager.LocalPlayer.Buffs.Find(MitaarBuddy.SpiritNanos.BlessingofTheLight, out Buff yellowbuff) && yellowbuff.RemainingTime < 2)
                //        MovementController.Instance.SetDestination(Constants._yellowPodium);
                //}

            }
        }
    }
}