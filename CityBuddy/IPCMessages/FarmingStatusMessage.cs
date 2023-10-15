using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace CityBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Farming)]
    public class FarmingStatusMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Farming;

        [AoMember(0)]
        public bool IsFarming { get; set; }
    }
}
