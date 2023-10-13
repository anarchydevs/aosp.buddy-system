using AOSharp.Common.GameData;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace Shared.IPCMessages
{
    [AoContract((int)IPCOpcode.SelectedMemberUpdate)]
    public class SelectedMemberUpdateMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.SelectedMemberUpdate;

        [AoMember(0)]
        public Identity SelectedMemberIdentity { get; set; }
    }
}
