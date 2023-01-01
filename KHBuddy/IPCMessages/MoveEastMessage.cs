using System;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace KHBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.MoveEast)]
    public class MoveEastMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.MoveEast;
    }
}
