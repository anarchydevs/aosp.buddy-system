using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace KHBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.West)]
    public class WestSelection : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.West;
    }
}
