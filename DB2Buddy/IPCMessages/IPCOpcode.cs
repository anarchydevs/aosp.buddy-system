using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB2Buddy.IPCMessages
{
    public enum IPCOpcode
    {
        Start = 1001,
        Stop = 1002,
        Enter = 1003,
        Farming = 1004,
        NoFarming = 1005
    }
}
