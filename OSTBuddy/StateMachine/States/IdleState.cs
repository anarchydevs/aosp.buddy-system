using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSTBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (OSTBuddy.Toggle == true)
                return new PullState();

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("IdleState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("IdleState::OnStateExit");
        }

        public void Tick()
        {

        }
    }
}
