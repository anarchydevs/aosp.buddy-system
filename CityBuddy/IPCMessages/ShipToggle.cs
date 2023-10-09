using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace CityBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Ship)]
    public class ShipToggle : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Ship;

        [AoMember(0)]
        public bool ShipOnOff { get; set; }
    }
}
