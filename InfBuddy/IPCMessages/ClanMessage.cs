using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace InfBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Clan)]
    public class ClanMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Clan;
    }
}
