using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace CityBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.SettingsUpdate)]
    public class SettingsUpdateMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.SettingsUpdate;

        [AoMember(0)]
        public bool StopAttack { get; set; }
        [AoMember(1)]
        public bool Red { get; set; }
        [AoMember(2)]
        public bool Blue { get; set; }
        [AoMember(3)]
        public bool Yellow { get; set; }
        [AoMember(4)]
        public bool Green { get; set; }
    }
}

