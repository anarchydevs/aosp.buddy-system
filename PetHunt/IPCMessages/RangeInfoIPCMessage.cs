using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace PetHunt.IPCMessages
{
    [AoContract((int)IPCOpcode.RangeInfo)]
    public class RangeInfoIPCMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.RangeInfo;

        [AoMember(0)]
        public int HuntRange { get; set; }

        
    }
}
