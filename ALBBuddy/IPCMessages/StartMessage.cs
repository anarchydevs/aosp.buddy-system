using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace ALBBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Start)]
    public class StartMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Start;
    }
}
