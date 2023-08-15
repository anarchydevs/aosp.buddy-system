using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace LeBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Enter)]
    public class EnterMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Enter;
    }
}
