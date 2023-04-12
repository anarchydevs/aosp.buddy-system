using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;
//using static MitaarBuddy.MitaarBuddy;

namespace MitaarBuddy
{
    public class MediumState : IState
    {
        //private static bool _init = false;

        private static SimpleChar _sinuh;
        private static Corpse _sinuhCorpse;

        private static SimpleChar _alienCoccoon;

        private static SimpleChar _xanSpirits;
        private static SimpleChar _redXanSpirit;

        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.XanHubId
                && !Team.IsInTeam
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) <= 10f)
                return new ReformState();

            if (_sinuhCorpse != null
                && _xanSpirits == null
                && _alienCoccoon == null
                && Extensions.CanProceed()
                && MitaarBuddy._settings["Farming"].AsBool())
                return new FarmingState();

            //if (_sinuhCorpse != null
            //    && _xanSpirits == null
            //    && _alienCoccoon == null
            //    && Extensions.CanProceed()
            //    && !MitaarBuddy._settings["Farming"].AsBool())
            //    return new IdleState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Start on Red, Medium Mode");


            MovementController.Instance.SetDestination(Constants._startPosition);
        }

        public void OnStateExit()
        {
            //if (_sinuhCorpse != null && _alienCoccoon == null && _xanSpirits == null)
                Chat.WriteLine("Medium over");
        }

        public void Tick()
        {
            if (!Team.IsInTeam || Game.IsZoning) { return; }

            foreach (TeamMember member in Team.Members)
            {
                if (!ReformState._teamCache.Contains(member.Identity))
                    ReformState._teamCache.Add(member.Identity);
            }

            MitaarBuddy._leader = Team.Members
                .Where(c => c.Character?.Health > 0
                    && c.Character?.IsValid == true
                    && c.IsLeader)
                .FirstOrDefault()?.Character;

            if (Playfield.ModelIdentity.Instance == 6017)
                //&& MitaarBuddy.DifficultySelection.Medium == (MitaarBuddy.DifficultySelection)MitaarBuddy._settings["DifficultySelection"].AsInt32()
                //&& MitaarBuddy._settings["Toggle"].AsBool()
                //&& !Team.Members.Any(c => c.Character == null))
            {
                Mobs();

                //Attack and initial start
                if (_sinuh != null && _alienCoccoon == null)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                        DynelManager.LocalPlayer.Attack(_sinuh);

                }

                if (_sinuh != null && _alienCoccoon != null)
                {
                    if (DynelManager.LocalPlayer.FightingTarget != null
                        && DynelManager.LocalPlayer.FightingTarget.Name == _sinuh.Name)
                    {
                        DynelManager.LocalPlayer.StopAttack();
                    }
                }

                if (_alienCoccoon != null)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                    {
                        DynelManager.LocalPlayer.Attack(_alienCoccoon);
                    }
                }

                //Pathing to spirits
                if (_xanSpirits != null && !MovementController.Instance.IsNavigating)
                {
                    if (_redXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._redPodium) > 0.9f
                        && !DynelManager.LocalPlayer.Buffs.Contains(MitaarBuddy.SpiritNanos.BlessingofTheBlood))
                        MovementController.Instance.SetDestination(Constants._redPodium);

                }
            }
        }

        public static void Mobs()
        {

            _sinuh = DynelManager.NPCs
                 .Where(c => c.Health > 0
                  && c.Name.Contains("Technomaster Sinuh")
                  && !c.Name.Contains("Remains of"))
                  .FirstOrDefault();

            _alienCoccoon = DynelManager.NPCs
               .Where(c => c.Health > 0
                       && c.Name.Contains("Alien Coccoon"))
                   .FirstOrDefault();

            _redXanSpirit = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.Name.Contains("Xan Spirit")
                    && c.Buffs.Contains(MitaarBuddy.SpiritNanos.BlessingofTheBlood))
                    .FirstOrDefault();

            _sinuhCorpse = DynelManager.Corpses
              .Where(c => c.Name.Contains("Remains of Technomaster Sinuh"))
                  .FirstOrDefault();

        }

    }
}