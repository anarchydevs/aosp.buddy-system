using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace InfBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Neutral)]
    public class NeutralMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Neutral;
    }
}
