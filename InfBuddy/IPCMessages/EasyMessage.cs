using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace InfBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Easy)]
    public class EasyMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Easy;
    }
}
