using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace DefendBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.ScanRange)]
    public class ScanRangeMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.ScanRange;

        [AoMember(0)]
        public int Range { get; set; }
    }
}
