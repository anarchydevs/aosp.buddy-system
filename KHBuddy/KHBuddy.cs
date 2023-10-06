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
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Debug = AOSharp.Core.Debug;

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

        private Stopwatch _kitTimer = new Stopwatch();

        public static string PluginDirectory;

        public static Settings _settings = new Settings("KHBuddy");

        public static double _timer = 0f;

        public static DateTime RespawnTime;
        public static DateTime RespawnTimeEast;
        public static DateTime RespawnTimeWest;
        public static DateTime GameTime;

        public static double _stateTimeOut = Time.NormalTime;

        public static bool _doingEast = false;
        public static bool _doingWest = false;
        public static bool _started = false;

        public static bool _init = false;
        public static bool NeedsKit = false;

        public static bool Beach = false;
        public static bool East = false;
        public static bool West = false;
        public static bool EastandWest = false;

        public static bool _beachToggled = false;
        public static bool _eastToggled = false;
        public static bool _westToggled = false;
        public static bool _eastandWestToggled = false;

        public static string previousErrorMessage = string.Empty;

        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("KHBuddy Loaded!");
                Chat.WriteLine("/khbuddy for settings.");

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\KHBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");
                IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.StartMode, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.StopMode, OnStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.MoveEast, OnMoveEastMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.MoveWest, OnMoveWestMessage);

                IPCChannel.RegisterCallback((int)IPCOpcode.Beach, BeachMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.East, EastMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.West, WestMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.EastandWest, EastAndWestMessage);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("Toggle", false);

                _settings["Toggle"] = false;

                _settings.AddVariable("SideSelection", (int)SideSelection.East);
                _settings["SideSelection"] = (int)SideSelection.East;

                SettingsController.RegisterSettingsWindow("KHBuddy", pluginDir + "\\UI\\KHBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                PluginDirectory = pluginDir;
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + KHBuddy.GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != KHBuddy.previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    KHBuddy.previousErrorMessage = errorMessage;
                }
            }
        }

        public static void IPCChannel_Changed(object s, int e)
        {
            IPCChannel.SetChannelId(Convert.ToByte(e));

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

        private void BeachMessage(int sender, IPCMessage msg)
        {
            _settings["SideSelection"] = (int)SideSelection.Beach;
        }
        private void EastMessage(int sender, IPCMessage msg)
        {
            _settings["SideSelection"] = (int)SideSelection.East;
        }
        private void WestMessage(int sender, IPCMessage msg)
        {
            _settings["SideSelection"] = (int)SideSelection.West;
        }
        private void EastAndWestMessage(int sender, IPCMessage msg)
        {
            _settings["SideSelection"] = (int)SideSelection.EastAndWest;
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
            try
            {
                if (Game.IsZoning)
                    return;

                _stateMachine.Tick();

                Selection();

                Shared.Kits kitsInstance = new Shared.Kits();

                kitsInstance.SitAndUseKit();

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
                            if (Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel != channelValue)
                            {
                                IPCChannel.SetChannelId(Convert.ToByte(channelValue));
                                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel = Convert.ToByte(channelValue);
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

                    if (Beach)
                    {
                        IPCChannel.Broadcast(new BeachSelection());
                        Beach = false;
                    }

                    if (East)
                    {
                        IPCChannel.Broadcast(new EastSelection());
                        East = false;
                    }

                    if (West)
                    {
                        IPCChannel.Broadcast(new WestSelection());
                        West = false;
                    }

                    if (EastandWest)
                    {
                        IPCChannel.Broadcast(new EastandWestSelection());
                        EastandWest = false;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + KHBuddy.GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != KHBuddy.previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    KHBuddy.previousErrorMessage = errorMessage;
                }
            }
        }

        public static void Selection()
        {
            if (SideSelection.Beach == (SideSelection)_settings["SideSelection"].AsInt32() && !_beachToggled)
            {
                Beach = true;
                East = false;
                West = false;
                EastandWest = false;

                _beachToggled = true;
                _eastToggled = false;
                _westToggled = false;
                _eastandWestToggled = false;

                Chat.WriteLine("Beach selected");
            }
            if (SideSelection.East == (SideSelection)_settings["SideSelection"].AsInt32() && !_eastToggled)
            {
                Beach = false;
                East = true;
                West = false;
                EastandWest = false;

                _beachToggled = false;
                _eastToggled = true;
                _westToggled = false;
                _eastandWestToggled = false;

                Chat.WriteLine("East selected");
            }
            if (SideSelection.West == (SideSelection)_settings["SideSelection"].AsInt32() && !_westToggled)
            {
                Beach = false;
                East = false;
                West = true;
                EastandWest = false;

                _beachToggled = false;
                _eastToggled = false;
                _westToggled = true;
                _eastandWestToggled = false;

                Chat.WriteLine("West selected");
            }
            if (SideSelection.EastAndWest == (SideSelection)_settings["SideSelection"].AsInt32() && !_eastandWestToggled)
            {
                Beach = false;
                East = false;
                West = false;
                EastandWest = true;

                _beachToggled = false;
                _eastToggled = false;
                _westToggled = false;
                _eastandWestToggled = true;

                Chat.WriteLine("East and West selected");
            }
        }

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

        public static int GetLineNumber(Exception ex)
        {
            var lineNumber = 0;

            var lineMatch = Regex.Match(ex.StackTrace ?? "", @":line (\d+)$", RegexOptions.Multiline);

            if (lineMatch.Success)
                lineNumber = int.Parse(lineMatch.Groups[1].Value);

            return lineNumber;
        }
    }
}
