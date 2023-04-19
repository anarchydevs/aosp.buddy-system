using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace InfBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Normal)]
    public class NormalMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Normal;
    }
}
