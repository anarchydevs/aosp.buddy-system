using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;
using static InfBuddy.InfBuddy;

namespace InfBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Faction)]
    public class FactionIPCMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Faction;
        public FactionSelection Faction { get; set; }
    }
}
