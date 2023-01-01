using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace OSTBuddy
{
    public class WaitState : IState
    {
        public const double RefreshMongoTime = 8f;
        public const double RefreshAbsorbTime = 11f;

        public double _timer;
        public double _refreshMongoTimer;
        public double _refreshAbsorbTimer;

        public SimpleChar LeaderChar;
        Spell absorb = null;


        public IState GetNextState()
        {
            if (DynelManager.LocalPlayer.Profession == Profession.Enforcer)
            {
                if (!OSTBuddy.Toggle)
                {
                    MovementController.Instance.Halt();
                    return new IdleState();
                }

                if (Time.NormalTime > _timer + OSTBuddy.Config.CharSettings[Game.ClientInst].RespawnDelay && OSTBuddy.MobsAllDead)
                {
                    OSTBuddy.MobsAllDead = false;
                    return new PullState();
                }
            }

            return null;
        }

        public void OnStateEnter()
        {
            //Chat.WriteLine("NukeState::OnStateEnter");
        }

        public void OnStateExit()
        {
            //Chat.WriteLine("NukeState::OnStateExit");
        }

        public void Tick()
        {
            if (OSTBuddy._waypoints.Count >= 1)
            {
                foreach (Vector3 pos in OSTBuddy._waypoints)
                {
                    Debug.DrawSphere(pos, 0.2f, DebuggingColor.White);
                }
            }

            if (DynelManager.LocalPlayer.Profession == Profession.Enforcer)
            {
                if (absorb == null)
                    absorb = Spell.List.Where(x => x.Nanoline == NanoLine.AbsorbACBuff).OrderBy(x => x.StackingOrder).FirstOrDefault();

                List<SimpleChar> mobsatend = DynelManager.NPCs
                    .Where(x => x.DistanceFrom(DynelManager.LocalPlayer) <= 43f
                        && x.IsAlive && x.FightingTarget != null
                        && x.Position.DistanceFrom(OSTBuddy._waypoints.Last()) < 10f)
                    .ToList();

                List<Corpse> mobsatenddead = DynelManager.Corpses
                    .Where(x => x.DistanceFrom(DynelManager.LocalPlayer) <= 43f
                        && x.Position.DistanceFrom(OSTBuddy._waypoints.Last()) < 10f)
                    .ToList();

                if (mobsatend.Count == 0 && mobsatenddead.Count >= 1 && !OSTBuddy.MobsAllDead)
                {
                    _timer = Time.NormalTime;
                    OSTBuddy.MobsAllDead = true;
                }

                if (mobsatend.Count >= 1)
                {
                    Spell mongobuff = Spell.GetSpellsForNanoline(NanoLine.MongoBuff).OrderByStackingOrder().FirstOrDefault();

                    if (mongobuff == null) { return; }

                    if (!Spell.HasPendingCast && mongobuff.IsReady && Time.NormalTime > _refreshMongoTimer + RefreshMongoTime)
                    {
                        mongobuff.Cast();
                        _refreshMongoTimer = Time.NormalTime;
                    }
                    if (!Spell.HasPendingCast && absorb.IsReady && Time.NormalTime > _refreshAbsorbTimer + RefreshAbsorbTime)
                    {
                        absorb.Cast();
                        _refreshAbsorbTimer = Time.NormalTime;
                    }
                }
            }
        }
    }
}
