using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using KHBuddy.IPCMessages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace KHBuddy
{
    public class KHBuddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static Identity Leader = Identity.None;
        public static bool IsLeader = false;

        public static string PluginDirectory;


        public static Settings _settings = new Settings("KHBuddy");

        public static double _timer = 0f;

        public static DateTime RespawnTime;
        public static DateTime RespawnTimeEast;
        public static DateTime RespawnTimeWest;
        public static DateTime GameTime;

        public static double _stateTimeOut = Time.NormalTime;

        private static double _sitUpdateTimer;

        public static bool _doingEast = false;
        public static bool _doingWest = false;
        public static bool _started = false;
        public static bool _init = false;
        public static bool Sitting = false;

        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("KHBuddy Loaded!");
                Chat.WriteLine("/khbuddy for settings.");

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KHBuddy\\{Game.ClientInst}\\Config.json");
                IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[Game.ClientInst].IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.StartMode, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.StopMode, OnStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.MoveEast, OnMoveEastMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.MoveWest, OnMoveWestMessage);

                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;

                //Chat.RegisterCommand("buddy", KHBuddyCommand);

                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("Toggle", false);

                _settings["Toggle"] = false;

                _settings.AddVariable("SideSelection", (int)SideSelection.East);

                SettingsController.RegisterSettingsWindow("KHBuddy", pluginDir + "\\UI\\KHBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                PluginDirectory = pluginDir;
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        public static void IPCChannel_Changed(object s, int e)
        {
            IPCChannel.SetChannelId(Convert.ToByte(e));

            ////TODO: Change in config so it saves when needed to - interface name -> INotifyPropertyChanged
            Config.Save();
        }

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            if (Leader == Identity.None)
                Leader = new Identity(IdentityType.SimpleChar, sender);

            if (DynelManager.LocalPlayer.Identity == Leader)
                return;

            StartModeMessage startMsg = (StartModeMessage)msg;

            _settings["SideSelection"] = startMsg.Side;

            _settings["Toggle"] = true;

            Start();
        }

        private void OnMoveEastMessage(int sender, IPCMessage msg)
        {
            if (Leader == Identity.None)
                Leader = new Identity(IdentityType.SimpleChar, sender);

            if (DynelManager.LocalPlayer.Identity == Leader)
                return;

            if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(1091.7f, 26.5f, 1051.4f)) > 1f && !MovementController.Instance.IsNavigating)
            {
                MovementController.Instance.SetDestination(new Vector3(1091.7f, 26.5f, 1051.4f));
            }
        }

        private void OnMoveWestMessage(int sender, IPCMessage msg)
        {
            if (Leader == Identity.None)
                Leader = new Identity(IdentityType.SimpleChar, sender);

            if (DynelManager.LocalPlayer.Identity == Leader)
                return;

            if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(1064.4f, 25.6f, 1032.6f)) > 1f && !MovementController.Instance.IsNavigating)
            {
                MovementController.Instance.SetDestination(new Vector3(1064.4f, 25.6f, 1032.6f));
            }
        }

        private void OnStopMessage(int sender, IPCMessage msg)
        {
            if (Leader == Identity.None)
                Leader = new Identity(IdentityType.SimpleChar, sender);

            if (DynelManager.LocalPlayer.Identity == Leader)
                return;

            StopModeMessage stopMsg = (StopModeMessage)msg;

            _started = false;

            _settings["SideSelection"] = stopMsg.Side;

            _settings["Toggle"] = false;

            if (NavMeshMovementController != null)
                NavMeshMovementController.Halt();
        }

        public override void Teardown()
        {
            SettingsController.CleanUp();
        }

        private void Start()
        {
            if (!_started)
                _started = true;

            if (DynelManager.LocalPlayer.Profession == Profession.Enforcer && !(_stateMachine.CurrentState is PullState))
                _stateMachine.SetState(new PullState());

            if (DynelManager.LocalPlayer.Profession == Profession.NanoTechnician && !(_stateMachine.CurrentState is NukeState))
                _stateMachine.SetState(new NukeState());
        }

        private void Stop()
        {
            _started = false;

            _stateMachine.SetState(new IdleState());

            if (NavMeshMovementController != null)
                NavMeshMovementController.Halt();
        }

        private void InfoView(object s, ButtonBase button)
        {
            Window helpWindow = Window.CreateFromXml("Info", PluginDirectory + "\\UI\\KHBuddyInfoView.xml",
            windowSize: new Rect(0, 0, 455, 345),
            windowStyle: WindowStyle.Default,
            windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);
            helpWindow.Show(true);
        }

        private void OnUpdate(object s, float deltaTime)
        {
            //GameTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Time.NormalTime);

            if (Game.IsZoning)
                return;

            _stateMachine.Tick();

            if (Time.NormalTime > _sitUpdateTimer + 1)
            {
                ListenerSit();

                _sitUpdateTimer = Time.NormalTime;
            }

            if (DynelManager.LocalPlayer.Profession == Profession.Enforcer)
            {
                if (SideSelection.EastAndWest == (SideSelection)_settings["SideSelection"].AsInt32())
                {
                    Debug.DrawSphere(new Vector3(1115.9f, 1.6f, 1064.3f), 0.2f, DebuggingColor.White);
                    Debug.DrawLine(DynelManager.LocalPlayer.Position, new Vector3(1115.9f, 1.6f, 1064.3f), DebuggingColor.White); // East
                }

                if (SideSelection.East == (SideSelection)_settings["SideSelection"].AsInt32())
                {
                    Debug.DrawSphere(new Vector3(1115.9f, 1.6f, 1064.3f), 0.2f, DebuggingColor.White);
                    Debug.DrawLine(DynelManager.LocalPlayer.Position, new Vector3(1115.9f, 1.6f, 1064.3f), DebuggingColor.White); // East
                }

                if (SideSelection.West == (SideSelection)_settings["SideSelection"].AsInt32())
                {
                    Debug.DrawSphere(new Vector3(1043.2f, 1.6f, 1021.1f), 0.2f, DebuggingColor.White);
                    Debug.DrawLine(DynelManager.LocalPlayer.Position, new Vector3(1043.2f, 1.6f, 1021.1f), DebuggingColor.White); // West
                }

                if (SideSelection.Beach == (SideSelection)_settings["SideSelection"].AsInt32())
                {
                    Debug.DrawSphere(new Vector3(898.1f, 4.4f, 289.9f), 0.2f, DebuggingColor.White);
                    Debug.DrawLine(DynelManager.LocalPlayer.Position, new Vector3(898.1f, 4.4f, 289.9f), DebuggingColor.White); // beach
                }
            }

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelBox);

                if (channelBox != null && !string.IsNullOrEmpty(channelBox.Text))
                {
                    if (int.TryParse(channelBox.Text, out int channelValue))
                    {
                        if (Config.CharSettings[Game.ClientInst].IPCChannel != channelValue)
                        {
                            IPCChannel.SetChannelId(Convert.ToByte(channelValue));
                            Config.CharSettings[Game.ClientInst].IPCChannel = Convert.ToByte(channelValue);
                            Config.Save();
                        }
                    }
                }

                if (SettingsController.settingsWindow.FindView("KHBuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = InfoView;
                }

                if (!_settings["Toggle"].AsBool() && _started == true
                    && DynelManager.LocalPlayer.Profession == Profession.Enforcer)
                {
                    if (SideSelection.Beach == (SideSelection)_settings["SideSelection"].AsInt32())
                    {
                        IPCChannel.Broadcast(new StopModeMessage()
                        {
                            Side = (int)SideSelection.Beach
                        });
                    }
                    else if (SideSelection.East == (SideSelection)_settings["SideSelection"].AsInt32())
                    {
                        IPCChannel.Broadcast(new StopModeMessage()
                        {
                            Side = (int)SideSelection.East
                        });
                    }
                    else if (SideSelection.West == (SideSelection)_settings["SideSelection"].AsInt32())
                    {
                        IPCChannel.Broadcast(new StopModeMessage()
                        {
                            Side = (int)SideSelection.West
                        });
                    }
                    else if (SideSelection.EastAndWest == (SideSelection)_settings["SideSelection"].AsInt32())
                    {
                        IPCChannel.Broadcast(new StopModeMessage()
                        {
                            Side = (int)SideSelection.EastAndWest
                        });
                    }

                    Stop();
                }

                if (_settings["Toggle"].AsBool() && _started == false
                    && DynelManager.LocalPlayer.Profession == Profession.NanoTechnician)
                {
                    _started = true;

                    if (Leader == Identity.None)
                    {
                        IsLeader = true;
                        Leader = DynelManager.LocalPlayer.Identity;
                    }

                    if (SideSelection.East == (SideSelection)_settings["SideSelection"].AsInt32())
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(1090.2f, 28.1f, 1050.1f)) > 1f && !MovementController.Instance.IsNavigating)
                        {
                            MovementController.Instance.SetDestination(new Vector3(1090.2f, 28.1f, 1050.1f));
                        }
                    }
                    else if (SideSelection.West == (SideSelection)_settings["SideSelection"].AsInt32())
                    {
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(1065.4f, 26.2f, 1033.5f)) > 1f && !MovementController.Instance.IsNavigating)
                        {
                            MovementController.Instance.SetDestination(new Vector3(1065.4f, 26.2f, 1033.5f));
                        }
                    }

                    Start();
                }

                if (_settings["Toggle"].AsBool() && _started == false
                    && DynelManager.LocalPlayer.Profession == Profession.Enforcer)
                {
                    _started = true;

                    if (Leader == Identity.None)
                    {
                        IsLeader = true;
                        Leader = DynelManager.LocalPlayer.Identity;
                    }

                    if (SideSelection.Beach == (SideSelection)_settings["SideSelection"].AsInt32())
                    {
                        IPCChannel.Broadcast(new StartModeMessage()
                        {
                            Side = (int)SideSelection.Beach
                        });
                    }
                    else if (SideSelection.East == (SideSelection)_settings["SideSelection"].AsInt32())
                    {
                        IPCChannel.Broadcast(new StartModeMessage()
                        {
                            Side = (int)SideSelection.East
                        });
                        IPCChannel.Broadcast(new MoveEastMessage());
                    }
                    else if (SideSelection.West == (SideSelection)_settings["SideSelection"].AsInt32())
                    {
                        IPCChannel.Broadcast(new StartModeMessage()
                        {
                            Side = (int)SideSelection.West
                        });
                        IPCChannel.Broadcast(new MoveWestMessage());
                    }
                    else if (SideSelection.EastAndWest == (SideSelection)_settings["SideSelection"].AsInt32())
                    {
                        IPCChannel.Broadcast(new StartModeMessage()
                        {
                            Side = (int)SideSelection.EastAndWest
                        });
                        IPCChannel.Broadcast(new MoveEastMessage());

                        _doingEast = true;
                    }

                    Start();
                }
            }
        }

        private bool CanUseSitKit()
        {
            if (Inventory.Find(297274, out Item premSitKit))
                if (DynelManager.LocalPlayer.Health > 0 && DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0
                                    && !DynelManager.LocalPlayer.IsMoving && !Game.IsZoning) { return true; }

            if (DynelManager.LocalPlayer.IsAlive && DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0
                    && !DynelManager.LocalPlayer.IsMoving && !Game.IsZoning)
            {
                List<Item> sitKits = Inventory.FindAll("Health and Nano Recharger").Where(c => c.Id != 297274).ToList();

                if (!sitKits.Any()) { return false; }

                foreach (Item sitKit in sitKits.OrderBy(x => x.QualityLevel))
                {
                    int skillReq = (sitKit.QualityLevel > 200 ? (sitKit.QualityLevel % 200 * 3) + 1501 : (int)(sitKit.QualityLevel * 7.5f));

                    if (DynelManager.LocalPlayer.GetStat(Stat.FirstAid) >= skillReq || DynelManager.LocalPlayer.GetStat(Stat.Treatment) >= skillReq)
                        return true;
                }
            }

            return false;
        }

        private void ListenerSit()
        {
            Item kit = Inventory.Items.Where(x => RelevantItems.Kits.Contains(x.Id)).FirstOrDefault();

            if (kit == null) { return; }

            if (CanUseSitKit())
            {
                if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment) && Sitting == false
                    && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
                {
                    if (DynelManager.LocalPlayer.NanoPercent < 66 || DynelManager.LocalPlayer.HealthPercent < 66)
                    {
                        Task.Factory.StartNew(
                           async () =>
                           {
                               Sitting = true;

                               if (DynelManager.LocalPlayer.IsAttacking)
                                   DynelManager.LocalPlayer.StopAttack();

                               await Task.Delay(400);
                               NavMeshMovementController.Instance.SetMovement(MovementAction.SwitchToSit);
                               await Task.Delay(800);
                               NavMeshMovementController.Instance.SetMovement(MovementAction.LeaveSit);
                               await Task.Delay(200);
                               Sitting = false;
                           });
                    }
                }
            }
        }

        //private void KHBuddyCommand(string command, string[] param, ChatWindow chatWindow)
        //{
        //    try
        //    {
        //        if (param.Length < 1)
        //        {
        //            if (!KHBuddySettings["West"].AsBool() && !KHBuddySettings["East"].AsBool()
        //                && !KHBuddySettings["Beach"].AsBool())
        //            {
        //                KHBuddySettings["West"] = false;
        //                KHBuddySettings["East"] = false;
        //                KHBuddySettings["Beach"] = false;
        //                KHBuddySettings["BothSides"] = false;

        //                Chat.WriteLine($"Can only toggle one mode.");
        //                return;
        //            }

        //            if (KHBuddySettings["West"].AsBool() && KHBuddySettings["Beach"].AsBool()
        //                || (KHBuddySettings["East"].AsBool() && KHBuddySettings["Beach"].AsBool()))
        //            {
        //                KHBuddySettings["West"] = false;
        //                KHBuddySettings["East"] = false;
        //                KHBuddySettings["Beach"] = false;
        //                KHBuddySettings["BothSides"] = false;

        //                Chat.WriteLine($"Can only toggle one mode.");
        //                return;
        //            }

        //            if (!KHBuddySettings["Toggle"].AsBool() && !_started)
        //            {
        //                if (KHBuddySettings["West"].AsBool() && KHBuddySettings["East"].AsBool()
        //                    && !KHBuddySettings["Beach"].AsBool())
        //                {
        //                    IsLeader = true;
        //                    Leader = DynelManager.LocalPlayer.Identity;

        //                    KHBuddySettings["BothSides"] = true;

        //                    if (DynelManager.LocalPlayer.Identity == Leader)
        //                    {
        //                        if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(1115.9f, 1.6f, 1064.3f)) < 5f) // East
        //                        {
        //                            KHBuddySettings["West"] = false;

        //                            IPCChannel.Broadcast(new StartModeMessage()
        //                            {
        //                                West = KHBuddySettings["West"].AsBool(),
        //                                East = KHBuddySettings["East"].AsBool(),
        //                                Beach = KHBuddySettings["Beach"].AsBool(),
        //                                BothSides = KHBuddySettings["BothSides"].AsBool()
        //                            });
        //                        }

        //                        if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(1043.2f, 1.6f, 1021.1f)) < 5f) // West
        //                        {
        //                            KHBuddySettings["East"] = false;

        //                            IPCChannel.Broadcast(new StartModeMessage()
        //                            {
        //                                West = KHBuddySettings["West"].AsBool(),
        //                                East = KHBuddySettings["East"].AsBool(),
        //                                Beach = KHBuddySettings["Beach"].AsBool(),
        //                                BothSides = KHBuddySettings["BothSides"].AsBool()
        //                            });
        //                        }
        //                    }
        //                    Start();
        //                    Chat.WriteLine("Starting");
        //                    return;
        //                }
        //                else
        //                {
        //                    IsLeader = true;
        //                    Leader = DynelManager.LocalPlayer.Identity;

        //                    KHBuddySettings["BothSides"] = false;

        //                    if (DynelManager.LocalPlayer.Identity == Leader)
        //                    {
        //                        IPCChannel.Broadcast(new StartModeMessage()
        //                        {
        //                            West = KHBuddySettings["West"].AsBool(),
        //                            East = KHBuddySettings["East"].AsBool(),
        //                            Beach = KHBuddySettings["Beach"].AsBool(),
        //                            BothSides = KHBuddySettings["BothSides"].AsBool()
        //                        });
        //                    }
        //                    Start();
        //                    Chat.WriteLine("Starting");
        //                    return;
        //                }
        //            }
        //            else if (KHBuddySettings["Toggle"].AsBool() && _started)
        //            {
        //                if (DynelManager.LocalPlayer.Identity == Leader)
        //                {
        //                    IPCChannel.Broadcast(new StopModeMessage()
        //                    {
        //                        West = KHBuddySettings["West"].AsBool(),
        //                        East = KHBuddySettings["East"].AsBool(),
        //                        Beach = KHBuddySettings["Beach"].AsBool(),
        //                        BothSides = KHBuddySettings["BothSides"].AsBool()
        //                    });
        //                }

        //                Stop();
        //                Chat.WriteLine("Stopping");
        //                return;
        //            }
        //        }
        //        Config.Save();
        //    }
        //    catch (Exception e)
        //    {
        //        Chat.WriteLine(e.Message);
        //    }
        //}

        public enum SideSelection
        {
            Beach, East, West, EastAndWest
        }
        public static class RelevantItems
        {
            public static readonly int[] Kits = {
                297274, 293296, 291084, 291083, 291082
            };
        }
    }
}
