using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace AttackBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.RangeInfo)]
    public class RangeInfoIPCMessage : IPCMessage
    {
        [AoMember(0)]
        public int AttackRange { get; set; }

        [AoMember(1)]
        public int ScanRange { get; set; }

        public override short Opcode => (short)IPCOpcode.RangeInfo;
    }
}
