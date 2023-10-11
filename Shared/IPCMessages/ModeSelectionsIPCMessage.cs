using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace Shared.IPCMessages
{
    [AoContract((int)IPCOpcode.ModeSelections)]
    public class ModeSelectionsIPCMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.ModeSelections;

        [AoMember(0)] public ModeSelection Mode { get; set; }
        [AoMember(1)] public FactionSelection Faction { get; set; }
        [AoMember(2)] public DifficultySelection Difficulty { get; set; }
    }
}
