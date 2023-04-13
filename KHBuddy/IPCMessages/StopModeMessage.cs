using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace KHBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.StopMode)]
    public class StopModeMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.StopMode;

        [AoMember(0)]
        public int Side { get; set; }
    }
}
