using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSTBuddy
{
    public interface IState
    {
        void Tick();
        IState GetNextState();
        void OnStateEnter();
        void OnStateExit();
    }
}
