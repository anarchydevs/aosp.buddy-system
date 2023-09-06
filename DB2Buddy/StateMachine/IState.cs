using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB2Buddy
{
    public interface IState
    {
        void Tick();
        IState GetNextState();
        void OnStateEnter();
        void OnStateExit();
    }
}
