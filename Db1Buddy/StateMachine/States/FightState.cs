using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using System.Linq;
using System.Threading;


namespace Db1Buddy
{
    public class FightState : IState
    {

        private static SimpleChar _mikkelsen;
        private static Corpse _mikkelsenCorpse;

        private static SimpleChar _maskedCommando;

        private static bool _yellow = false;
        private static bool _blue = false;
        private static bool _green = false;
        private static bool _red = false;

        public IState GetNextState()
        {


            //if (Playfield.ModelIdentity.Instance == Constants.PWId
            //    && !Team.IsInTeam
            //    && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) < 10f)
            //    return new ReformState();

            if (Playfield.ModelIdentity.Instance == Constants.DB1Id
               && !DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.ThriceBlessedbytheAncients))
                return new GetBuffState();


            if (Db1Buddy.MikkelsenCorpse
                && Extensions.CanProceed()
                && Db1Buddy._settings["Farming"].AsBool())
                return new FarmingState();


            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("FightState");
        }

        public void OnStateExit()
        {
            Chat.WriteLine("Exit FightState");
        }

        public void Tick()
        {
            if (!Team.IsInTeam || Game.IsZoning) { return; }

            foreach (TeamMember member in Team.Members)
            {
                if (!ReformState._teamCache.Contains(member.Identity))
                    ReformState._teamCache.Add(member.Identity);
            }

            if (Playfield.ModelIdentity.Instance == 6003)
            {
                Mobs();

                if (_mikkelsenCorpse != null)
                    Db1Buddy.MikkelsenCorpse = true;

                //Attack and initial start
                if (_mikkelsen != null 
                    && DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.ThriceBlessedbytheAncients)
                    && !DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.CallofRust))
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                        DynelManager.LocalPlayer.Attack(_mikkelsen);
                }

                if (_mikkelsen != null && DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.CallofRust))
                {
                    if (DynelManager.LocalPlayer.FightingTarget != null
                        && DynelManager.LocalPlayer.FightingTarget.Name == _mikkelsen.Name)
                    {
                        DynelManager.LocalPlayer.StopAttack();
                    }
                }

                if (_maskedCommando != null)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_maskedCommando);
                    }
                }

                //Pathing to podiums
                if (!MovementController.Instance.IsNavigating)
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._redPodium) > 0.9f
                        && DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.HealingBlight))
                        MovementController.Instance.SetDestination(Constants._redPodium);

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._bluePodium) > 0.9f
                        && DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.CallofRust))
                        MovementController.Instance.SetDestination(Constants._bluePodium);

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._greenPodium) > 0.9f
                        && DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.CrawlingSkin))
                        MovementController.Instance.SetDestination(Constants._greenPodium);

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._yellowPodium) > 0.9f
                        && DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.GreedoftheSource))
                        MovementController.Instance.SetDestination(Constants._yellowPodium);
                }

                if (!DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.CallofRust)
                   && !DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.CrawlingSkin)
                   && !DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.HealingBlight)
                   && !DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.Nanos.GreedoftheSource))
                {
                    if (DynelManager.LocalPlayer.Position.DistanceFrom(Constants._returnPosition) > 10f)
                        Db1Buddy.NavMeshMovementController.SetNavMeshDestination(Constants._returnPosition);
                }
            }
        }

        public static void Mobs()
        {

            _mikkelsen = DynelManager.NPCs
                 .Where(c => c.Health > 0
                  && c.Name.Contains("Ground Chief Mikkelsen")
                  && !c.Name.Contains("Remains of"))
                  .FirstOrDefault();

            _maskedCommando = DynelManager.NPCs
               .Where(c => c.Health > 0
                       && c.Name.Contains("Masked Commando"))
                   .FirstOrDefault();

            _mikkelsenCorpse = DynelManager.Corpses
              .Where(c => c.Name.Contains("Remains of Ground Chief Mikkelsen"))
                  .FirstOrDefault();

        }

    }
}