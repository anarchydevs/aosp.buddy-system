using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;
using static KHBuddy.KHBuddy;

namespace KHBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.SideSelections)]
    public class SideSelectionsIPCMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.SideSelections;

        [AoMember(0)] public SideSelection Side { get; set; }
       
    }
}
