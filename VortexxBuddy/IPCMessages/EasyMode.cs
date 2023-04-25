﻿using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace VortexxBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.EasyMode)]
    public class EasyMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.EasyMode;
    }
}
