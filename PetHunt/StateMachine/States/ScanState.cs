using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace PetHunt
{
    public class ScanState : IState
    {
        private SimpleChar _target;

        public IState GetNextState()
        {

            if (PetHunt._settings["Enable"].AsBool())
            {
                if (_target != null)
                {
                    return new PetAttackState(_target);
                }
            }
            else
            {
                return new IdleState();
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("Scanning");
        }

        public void Tick()
        {
            if (PetHunt._mob.Count >= 1)
            {
                if (PetHunt._mob.FirstOrDefault().Health == 0) { return; }

                _target = PetHunt._mob.FirstOrDefault();
                //Chat.WriteLine($"Found _target: {_target.Name}.");
            }
            else if (PetHunt._bossMob.Count >= 1)
            {
                if (PetHunt._bossMob.FirstOrDefault().Health == 0) { return; }

                _target = PetHunt._bossMob.FirstOrDefault();
                //Chat.WriteLine($"Found _target: {_target.Name}.");
            }
        }

        public void OnStateExit()
        {
        }
    }
}
