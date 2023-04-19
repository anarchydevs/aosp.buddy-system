using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System.Linq;
using System.Threading;


namespace Db1Buddy
{
    public class GetBuffState : IState
    {
       
        private static SimpleChar _mikkelsen;

        private static bool _yellow = false;
        private static bool _blue = false;
        private static bool _green = false;
        private static bool _red = false;

        public IState GetNextState()
        {

            //if (Playfield.ModelIdentity.Instance == Constants.DB1Id
            //    && DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.ThriceBlessedbytheAncients))
            //    return new IdleState();

            if (Playfield.ModelIdentity.Instance == Constants.DB1Id
                && DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.ThriceBlessedbytheAncients))
                return new FightState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("GetBuffState");
        }

        public void OnStateExit()
        {
            _yellow = false;
            _blue = false;
            _green = false;
            _red = false;

            Chat.WriteLine("Exit GetBuffState");
        }

        public void Tick()
        {

            _mikkelsen = DynelManager.NPCs
                .Where(c => c.Health > 0
                 && c.Name.Contains("Ground Chief Mikkelsen")
                 && !c.Name.Contains("Remains of"))
                 .FirstOrDefault();

            if (!Team.IsInTeam || Game.IsZoning) { return; }

            if (Playfield.ModelIdentity.Instance == 6003)
            {

                if (_mikkelsen != null && DynelManager.LocalPlayer.Identity == Db1Buddy.Leader)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                        DynelManager.LocalPlayer.Attack(_mikkelsen);
                }


                foreach (TeamMember member in Team.Members)
                {
                        if (!_yellow && !_blue && !_green && !_red)
                        {
                            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._yellowPodium) < 2f
                            && DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.BlessingoftheAncientMachinist))
                                _yellow = true;
                            else
                                Db1Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._yellowPodium);
                        }
                        else if (_yellow && !_blue && !_green && !_red)
                        {
                            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._bluePodium) < 2f
                            && DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.BlessingoftheEternalCraftsman))
                                _blue = true;
                            else
                                Db1Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._bluePodium);
                        }
                        else if (_yellow && _blue && !_green && !_red)
                        {
                            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._greenPodium) < 2f
                            && DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.BlessingoftheAncientForm))
                                _green = true;
                            else
                                Db1Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._greenPodium);
                        }
                        else if (_yellow && _blue && _green && !_red)
                        {
                            if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._redPodium) < 2f
                            && DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.BlessingoftheEternalCleric))
                                _red = true;
                            else
                                Db1Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._redPodium);
                        }
                        else
                    {
                        _yellow = false;
                        _blue = false;
                        _green = false;
                        _red = false;
                    }
                    
                }
            }

        }
    }
}