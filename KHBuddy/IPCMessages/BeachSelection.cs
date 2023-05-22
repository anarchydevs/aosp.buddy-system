using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace KHBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Beach)]
    public class BeachSelection : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Beach;
    }
}
