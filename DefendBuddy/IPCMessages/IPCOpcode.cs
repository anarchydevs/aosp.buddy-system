using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefendBuddy.IPCMessages
{
    public enum IPCOpcode
    {
        Start = 1001,
        Stop = 1002,
        SetPos = 1003,
        SetResetPos = 1004,
        AttackRange = 1005,
        ScanRange = 1006
    }
}
