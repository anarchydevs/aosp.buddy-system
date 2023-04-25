using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace VortexxBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.HardcoreMode)]
    public class HardecoreMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.HardcoreMode;
    }
}
