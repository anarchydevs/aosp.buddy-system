using AOSharp.Common.GameData;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace CityBuddy.IPCMessages
{
    [AoContract((int)IPCOpcode.WaitAndReady)]
    public class WaitAndReadyIPCMessage : IPCMessage
    {
        [AoMember(0)]
        public Identity PlayerIdentity { get; set; }

        [AoMember(1)]
        
        public bool IsReady { get; set; }

        public override short Opcode => (short)IPCOpcode.WaitAndReady;
    }
}
