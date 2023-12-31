﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB2Buddy
{
    public class IdleState : IState
    {
        private SimpleChar _aune;
        private Corpse _auneCorpse;
        private SimpleChar _redTower;
        private SimpleChar _blueTower;
        private Dynel _exitBeacon;

        public IState GetNextState()
        {
            _aune = DynelManager.NPCs
              .Where(c => c.Health > 0
                  && c.Name.Contains("Ground Chief Aune")
                  && !c.Name.Contains("Remains of "))
              .FirstOrDefault();

            _auneCorpse = DynelManager.Corpses
               .Where(c => c.Name.Contains("Remains of Ground Chief Aune"))
               .FirstOrDefault();

            _redTower = DynelManager.NPCs
             .Where(c => c.Health > 0
                 && c.Name.Contains("Strange Xan Artifact")
                 && !c.Name.Contains("Remains of ")
                 && c.Buffs.Contains(274119))
             .FirstOrDefault();

            _blueTower = DynelManager.NPCs
               .Where(c => c.Health > 0
                   && c.Name.Contains("Strange Xan Artifact")
                   && !c.Name.Contains("Remains of ")
                   && !c.Buffs.Contains(274119))
               .FirstOrDefault();

            _exitBeacon = DynelManager.AllDynels
                .Where(c => c.Name.Contains("Dust Brigade Exit Beacon"))
                .FirstOrDefault();

            if (!DB2Buddy._settings["Toggle"].AsBool())
                DB2Buddy.NavMeshMovementController.Halt();

            if (DB2Buddy._settings["Toggle"].AsBool())

            {
                if (Playfield.ModelIdentity.Instance == Constants.PWId)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 30f
                       && !Team.IsInTeam)
                        return new ReformState();

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 30f
                        && Team.IsInTeam
                        && Extensions.CanProceed())
                        return new EnterState();
                }

                if (Playfield.ModelIdentity.Instance == Constants.DB2Id)
                {
                    Network.ChatMessageReceived += (s, msg) =>
                    {
                        if (msg.PacketType != ChatMessageType.NpcMessage)
                            return;

                        var npcMsg = (NpcMessage)msg;

                        string[] triggerMsg = new string[2] { "Know the power of the Xan", "You will never know the secrets of the machine" };

                        if (triggerMsg.Any(x => npcMsg.Text.Contains(x)))
                        {
                            DB2Buddy._taggedNotum = true;
                        }
                    };

                    if (DB2Buddy._taggedNotum)
                    {
                        return new NotumState();
                    }

                    if (_aune != null)
                    {
                        if (_redTower != null || DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
                        {
                            return new FightTowerState();
                        }

                        if (_blueTower != null || _aune.Buffs.Contains(DB2Buddy.Nanos.StrengthOfTheAncients))
                        {
                            if (!DynelManager.LocalPlayer.Buffs.Contains(DB2Buddy.Nanos.XanBlessingoftheEnemy))
                                return new FightTowerState();
                        }
                    }

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPosition) > 50f
                     && Team.IsInTeam)
                        return new PathToBossState();

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPosition) < 50f
                         && Team.IsInTeam)
                        return new FightState();

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.first) < 60)
                        return new FellState();

                    if ((DB2Buddy.AuneCorpse || _exitBeacon != null)
                       && Extensions.CanProceed()
                       && DB2Buddy._settings["Farming"].AsBool())
                        return new FarmingState();

                }
            }
        
            return null;
        }

    public void OnStateEnter()
    {
        Chat.WriteLine("IdleState");
    }

    public void OnStateExit()
    {
       Chat.WriteLine("Exit IdleState");
    }

    public void Tick()
    {

    }
}
}
