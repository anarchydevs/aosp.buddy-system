using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace InfBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Medium)]
    public class MediumMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Medium;
    }
}
