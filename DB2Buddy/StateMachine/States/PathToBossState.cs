﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DB2Buddy
{
    public class PathToBossState : IState
    {
        private static bool _init = false;

        private static double _time;
        private static double _startTime;

        private static SimpleChar _aune;
        private static SimpleChar _redTower;
        private static SimpleChar _blueTower;

        public IState GetNextState()
        {
            _aune = DynelManager.NPCs
            .Where(c => c.Health > 0
                && c.Name.Contains("Ground Chief Aune"))
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

            if (!DB2Buddy._settings["Toggle"].AsBool())
            {
                DB2Buddy.NavMeshMovementController.Halt();
            }

            if (Playfield.ModelIdentity.Instance != Constants.DB2Id)
                return new IdleState();

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

                if (_aune != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._startPosition) < 1)
                {
                    return new FightState();
                }

                if (_aune == null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._startPosition) < 5)
                {
                    return new IdleState();
                }
<<<<<<< HEAD
=======

                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants.first) < 60)
                    return new FellState();
>>>>>>> aab7ee3ccaa03c6ad6b10dee74da529f4148bb84
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("PathToBossState");
            _time = Time.NormalTime;
            _startTime = Time.NormalTime;
            _init = true;
        }

        public void OnStateExit()
        {
            Chat.WriteLine(" Exit PathToBossState");

            _init = false;
        }

        public void Tick()
        {
            if (Game.IsZoning) { return; }

            if (Playfield.ModelIdentity.Instance == Constants.DB2Id)
            {
                if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._centerPosition) > 5f)
                {
                    DB2Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._startPosition);
                }
            }
        }
    }
}