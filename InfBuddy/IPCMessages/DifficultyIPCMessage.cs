using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace InfBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.Difficulty)]
    public class DifficultyIPCMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Difficulty;
        public int Difficulty { get; set; }
    }
}
