using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System.Linq;
//using static Db1Buddy.Db1Buddy;

namespace Db1Buddy
{
    public class HardcoreState : IState
    {
        //private static bool _init = false;

        private static SimpleChar _sinuh;
        private static Corpse _sinuhCorpse;

        private static SimpleChar _alienCoccoon;

        private static SimpleChar _xanSpirits;
        private static SimpleChar _greenXanSpirit;
        private static SimpleChar _redXanSpirit;
        private static SimpleChar _blueXanSpirit;
        private static SimpleChar _yellowXanSpirit;



        public IState GetNextState()
        {
            if (Extensions.HasDied())
                return new DiedState();

            if (Playfield.ModelIdentity.Instance == Constants.PWId
                && !Team.IsInTeam
                && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._entrance) <= 10f)
                return new ReformState();

            if (_sinuhCorpse != null
                 && _xanSpirits == null
                 && _alienCoccoon == null
                 && Extensions.CanProceed()
                 && Db1Buddy._settings["Farming"].AsBool())
                return new FarmingState();

            //if (_sinuhCorpse != null
            //    && _xanSpirits == null
            //    && _alienCoccoon == null
            //    && Extensions.CanProceed()
            //    && !Db1Buddy._settings["Farming"].AsBool())
            //    return new IdleState();

            return null;
        }

        public void OnStateEnter()
        {
            Chat.WriteLine("Starting Hardcore Parkour");

            //MovementController.Instance.SetDestination(Constants._startPosition);
        }

        public void OnStateExit()
        {
            //if (_sinuhCorpse != null && _alienCoccoon == null && _xanSpirits == null)
            Chat.WriteLine("Hardcore Parkour over");
        }

        public void Tick()
        {
            if (!Team.IsInTeam || Game.IsZoning) { return; }

            foreach (TeamMember member in Team.Members)
            {
                if (!ReformState._teamCache.Contains(member.Identity))
                    ReformState._teamCache.Add(member.Identity);
            }

            Db1Buddy._leader = Team.Members
                .Where(c => c.Character?.Health > 0
                    && c.Character?.IsValid == true
                    && c.IsLeader)
                .FirstOrDefault()?.Character;

            if (Playfield.ModelIdentity.Instance == 6017)
                //&& Db1Buddy.DifficultySelection.Hardcore == (Db1Buddy.DifficultySelection)Db1Buddy._settings["DifficultySelection"].AsInt32()
                //&& Db1Buddy._settings["Toggle"].AsBool()
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
                        && !DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.SpiritNanos.BlessingofTheBlood))
                        MovementController.Instance.SetDestination(Constants._redPodium);
                    else if (_redXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._redPodium) > 0.9f
                        && DynelManager.LocalPlayer.Buffs.Find(Db1Buddy.SpiritNanos.BlessingofTheBlood, out Buff redbuff) && redbuff.RemainingTime < 5)
                        MovementController.Instance.SetDestination(Constants._redPodium);

                    if (_blueXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._bluePodium) > 0.9f
                        && !DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.SpiritNanos.BlessingofTheSource))
                        MovementController.Instance.SetDestination(Constants._bluePodium);
                    else if (_blueXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._bluePodium) > 0.9f
                        && DynelManager.LocalPlayer.Buffs.Find(Db1Buddy.SpiritNanos.BlessingofTheSource, out Buff bluebuff) && bluebuff.RemainingTime < 4)
                        MovementController.Instance.SetDestination(Constants._bluePodium);

                    if (_greenXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._greenPodium) > 0.9f
                        && !DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.SpiritNanos.BlessingofTheOutsider))
                        MovementController.Instance.SetDestination(Constants._greenPodium);
                    else if (_greenXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._greenPodium) > 0.9f
                       && DynelManager.LocalPlayer.Buffs.Find(Db1Buddy.SpiritNanos.BlessingofTheOutsider, out Buff greenbuff) && greenbuff.RemainingTime < 3)
                        MovementController.Instance.SetDestination(Constants._greenPodium);

                    if (_yellowXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._yellowPodium) > 0.9f
                        && !DynelManager.LocalPlayer.Buffs.Contains(Db1Buddy.SpiritNanos.BlessingofTheLight))
                        MovementController.Instance.SetDestination(Constants._yellowPodium);
                    else if (_yellowXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(Constants._yellowPodium) > 0.9f
                       && DynelManager.LocalPlayer.Buffs.Find(Db1Buddy.SpiritNanos.BlessingofTheLight, out Buff yellowbuff) && yellowbuff.RemainingTime < 2)
                        MovementController.Instance.SetDestination(Constants._yellowPodium);
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

            _xanSpirits = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.Name.Contains("Xan Spirit"))
                    .FirstOrDefault();

            _redXanSpirit = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.Name.Contains("Xan Spirit")
                    && c.Buffs.Contains(Db1Buddy.SpiritNanos.BlessingofTheBlood))
                    .FirstOrDefault();

            _blueXanSpirit = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.Name.Contains("Xan Spirit")
                    && c.Buffs.Contains(Db1Buddy.SpiritNanos.BlessingofTheSource))
                    .FirstOrDefault();

            _greenXanSpirit = DynelManager.NPCs
               .Where(c => c.Health > 0
                   && c.Name.Contains("Xan Spirit")
                   && c.Buffs.Contains(Db1Buddy.SpiritNanos.BlessingofTheOutsider))
                   .FirstOrDefault();

            _yellowXanSpirit = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.Name.Contains("Xan Spirit")
                    && c.Buffs.Contains(Db1Buddy.SpiritNanos.BlessingofTheLight))
                    .FirstOrDefault();

            _sinuhCorpse = DynelManager.Corpses
              .Where(c => c.Name.Contains("Remains of Technomaster Sinuh"))
                  .FirstOrDefault();

        }

    }
}