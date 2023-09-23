using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace AXPBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.WaitAndReady)]
    public class WaitAndReadyIPCMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.WaitAndReady;
        [AoMember(0)]
        public bool IsReady { get; set; }
    }
}
