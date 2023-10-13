using AOSharp.Common.GameData;
using AOSharp.Core;

namespace KHBuddy
{
    public class IdleState : IState
    {
        public IState GetNextState()
        {
            if (KHBuddy._settings["Toggle"].AsBool())
            {
                if (DynelManager.LocalPlayer.Profession == Profession.Enforcer)
                {
                    return new PullState();
                }

                if (DynelManager.LocalPlayer.Profession == Profession.NanoTechnician)
                {
                    return new NukeState();
                }

            }

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
