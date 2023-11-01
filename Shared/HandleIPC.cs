using AOSharp.Core;
using AOSharp.Common.GameData;
using System.Diagnostics;
using System.Linq;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using System.Collections.Generic;
using System.Security.Policy;
using AOSharp.Core.IPC;
using Shared.IPCMessages;

namespace Shared
{
    public class HandleIPC
    {
        // Make sure these are defined
        ModeSelection internalMode;
        FactionSelection internalFaction;
        DifficultySelection internalDifficulty;

        public void HandleIncomingIPCMessage(IPCMessage message)
        {
            if (message.Opcode == (short)IPCOpcode.ModeSelections)
            {
                ModeSelectionsIPCMessage modeSelectionsMessage = (ModeSelectionsIPCMessage)message;

                internalMode = modeSelectionsMessage.Mode;
                internalFaction = modeSelectionsMessage.Faction;
                internalDifficulty = modeSelectionsMessage.Difficulty;
            }
        }

        public enum ModeSelection
        {
            Normal,
            Roam
        }
        public enum FactionSelection
        {
            Neutral,
            Clan,
            Omni
        }
        public enum DifficultySelection
        {
            Easy,
            Medium,
            Hard
        }
    }
}
