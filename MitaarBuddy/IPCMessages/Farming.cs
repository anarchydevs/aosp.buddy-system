using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace MitaarBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Farming)]
    public class FarmingMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Farming;
    }
}
