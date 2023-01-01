using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using KHBuddy.IPCMessages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
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
            //if (DynelManager.LocalPlayer.Profession == Profession.Enforcer)
            //{
            //    Debug.DrawSphere(new Vector3(1115.9f, 1.6f, 1064.3f), 0.2f, DebuggingColor.White);
            //    Debug.DrawLine(DynelManager.LocalPlayer.Position, new Vector3(1115.9f, 1.6f, 1064.3f), DebuggingColor.White); // East

            //    Debug.DrawSphere(new Vector3(1043.2f, 1.6f, 1021.1f), 0.2f, DebuggingColor.White);
            //    Debug.DrawLine(DynelManager.LocalPlayer.Position, new Vector3(1043.2f, 1.6f, 1021.1f), DebuggingColor.White); // West
            //}
        }
    }
}
