using AOSharp.Common.GameData;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace CityBuddy.IPCMessages
{
    // Combined IPC Message for Leader's identity
    [AoContract((int)IPCOpcode.LeaderInfo)]
    public class LeaderInfoIPCMessage : IPCMessage
    {
        [AoMember(0)]
        public Identity LeaderIdentity { get; set; }

        [AoMember(1)]
        public bool IsRequest { get; set; }

        public override short Opcode => (short)IPCOpcode.LeaderInfo;
    }

}
