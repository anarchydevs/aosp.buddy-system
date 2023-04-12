using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace KHBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.StartMode)]
    public class StartModeMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.StartMode;

        [AoMember(0)]
        public int Side { get; set; }
    }
}
