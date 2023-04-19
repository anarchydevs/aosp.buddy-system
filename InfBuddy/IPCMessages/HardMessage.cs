using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace InfBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Hard)]
    public class HardMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Hard;
    }
}
