using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MitaarBuddy.IPCMessages
{
    public enum IPCOpcode
    {
        Start = 1011,
        Stop = 1012,
        Farming = 1013,
        NoFarming = 1014,
        EasyMode = 1015,
        MediumMode = 1016,
        HardcoreMode = 1017
    }
}
