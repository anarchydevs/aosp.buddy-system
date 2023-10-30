using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace Shared.IPCMessages
{
    [AoContract((int)IPCOpcode.ModeSelections)]
    public class ModeSelectionsIPCMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.ModeSelections;

        [AoMember(0)] public HandleIPC.ModeSelection Mode { get; set; }
        [AoMember(1)] public HandleIPC.FactionSelection Faction { get; set; }
        [AoMember(2)] public HandleIPC.DifficultySelection Difficulty { get; set; }
    }
}
