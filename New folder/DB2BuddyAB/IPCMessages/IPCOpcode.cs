using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB2Buddy.IPCMessages
{
    public enum IPCOpcode
    {
        Start = 1021,
        Stop = 1022,
        AttackRange = 1023,
        WarpRange = 1024
    }
}
