using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALBBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (ALBBuddy.Toggle == true && Team.IsInTeam && ALBBuddy._settings["Toggle"].AsBool())
            {
                return new EnterAlbtraumState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("IdleState::OnStateEnter");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("IdleState::OnStateExit");
        }

        public void Tick()
        {
        }
    }
}
