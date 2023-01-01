using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHBuddy.IPCMessages
{
    public enum IPCOpcode
    {
        StartMode = 1001,
        StopMode = 1002,
        MoveWest = 1003,
        MoveEast = 1004
    }
}
