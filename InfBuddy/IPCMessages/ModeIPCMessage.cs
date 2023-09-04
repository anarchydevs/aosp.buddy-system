using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;
using static InfBuddy.InfBuddy;

namespace InfBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Mode)]
    public class ModeIPCMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Mode;
        public ModeSelection Mode { get; set; }
    }
}
