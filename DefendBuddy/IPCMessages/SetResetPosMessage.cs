using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace DefendBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.SetResetPos)]
    public class SetResetPosMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.SetResetPos;
    }
}
