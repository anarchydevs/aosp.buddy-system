﻿using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace VortexxBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.NoFarming)]
    public class NoFarmingMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.NoFarming;
    }
}