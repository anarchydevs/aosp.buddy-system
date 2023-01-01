using System;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace KHBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.MoveWest)]
    public class MoveWestMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.MoveWest;
    }
}
