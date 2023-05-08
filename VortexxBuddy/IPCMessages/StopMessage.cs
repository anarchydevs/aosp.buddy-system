﻿using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace VortexxBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Stop)]
    public class StopMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Stop;
    }
}
