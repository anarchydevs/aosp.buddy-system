using AOSharp.Common.GameData;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace CityBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Leader)]
    public class LeaderMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Leader;

        [AoMember(0)]
        public Identity Leader { get; set; }
    }
}
