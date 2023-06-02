using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpDB2.IPCMessages
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
