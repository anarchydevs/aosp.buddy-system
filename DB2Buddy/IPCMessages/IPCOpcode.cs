using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB2Buddy.IPCMessages
{
    public enum IPCOpcode
    {
        StartStop = 1001,
        LeaderInfo = 1002,
        WaitAndReady = 1003,
        Farming = 1004,
        //ModeSelections = 1005,
        SettingsUpdate = 1006,
        RangeInfo = 1007,
        Enter = 1008,
        SelectedMemberUpdate = 1009,
        ClearSelectedMember = 1010,
    }
}
