using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace MitaarBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.MediumMode)]
    public class MediumMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.MediumMode;
    }
}
