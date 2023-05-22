using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace KHBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.East)]
    public class EastSelection : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.East;
    }
}
