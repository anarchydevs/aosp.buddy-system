using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace InfBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Omni)]
    public class OmniMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Omni;
    }
}
