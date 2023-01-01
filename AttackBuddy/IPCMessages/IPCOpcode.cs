using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackBuddy.IPCMessages
{
    public enum IPCOpcode
    {
        Start = 1001,
        Stop = 1002,
        AttackRange = 1003,
        ScanRange = 1004
    }
}
