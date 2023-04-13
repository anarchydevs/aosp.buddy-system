using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace MitaarBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Start)]
    public class StartMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Start;
    }
}
