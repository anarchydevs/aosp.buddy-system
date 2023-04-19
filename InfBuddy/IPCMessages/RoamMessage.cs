using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace InfBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Roam)]
    public class RoamMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Roam;
    }
}
