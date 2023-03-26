using System;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace DB2Buddy.IPCMessages
{
    [AoContract((int)IPCOpcode.WarpRange)]
    public class WarpRangeMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.WarpRange;

        [AoMember(0)]
        public int Range { get; set; }
    }
}
