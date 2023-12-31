﻿using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace DefendBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.AttackRange)]
    public class AttackRangeMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.AttackRange;

        [AoMember(0)]
        public int Range { get; set; }
    }
}
