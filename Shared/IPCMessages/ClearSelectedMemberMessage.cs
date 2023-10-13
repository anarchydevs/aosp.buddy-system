﻿using AOSharp.Common.GameData;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace Shared.IPCMessages
{
    [AoContract((int)IPCOpcode.ClearSelectedMember)]
    public class ClearSelectedMemberMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.ClearSelectedMember;
    }

}
