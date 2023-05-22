using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace KHBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.EastandWest)]
    public class EastandWestSelection : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.EastandWest;
    }
}
