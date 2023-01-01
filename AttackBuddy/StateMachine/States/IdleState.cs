using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (AttackBuddy.Toggle)
                return new ScanState();

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
