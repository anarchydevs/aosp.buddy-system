using System;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace DefendBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.SetPos)]
    public class SetPosMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.SetPos;
    }
}
