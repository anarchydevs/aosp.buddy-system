using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using KHBuddy.IPCMessages;
using System;
using System.Diagnostics;
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

        public static string PluginDirectory;

        public static Settings _settings = new Settings("KHBuddy");

        public static double _timer = 0f;

        public static DateTime RespawnTime;
        public static DateTime RespawnTimeEast;
        public static DateTime RespawnTimeWest;
        public static DateTime GameTime;

        public static double _stateTimeOut = Time.NormalTime;

        public static bool Toggle = false;

        SideSelection currentSide;

        public static bool _doingEast = true;
        public static bool _doingWest = false;
        public static bool _started = false;

        public static bool _init = false;
        //public static bool NeedsKit = false;

        public static string previousErrorMessage = string.Empty;

        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("KHBuddy Loaded!");
                Chat.WriteLine("/khbuddy for settings.");

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\KHBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");
                IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.StartStop, OnStartStopMessage);
                IPCChannel.RegisterCallback((short)IPCOpcode.SideSelections, OnSideSelectionsMessage);

                IPCChannel.RegisterCallback((int)IPCOpcode.MoveEast, OnMoveEastMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.MoveWest, OnMoveWestMessage);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("Toggle", false);
                _settings["Toggle"] = false; //to save

                _settings.AddVariable("SideSelection", (int)SideSelection.East);

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

        public override void Teardown()
        {
            SettingsController.CleanUp();
        }

        public static void IPCChannel_Changed(object s, int e)
        {
            IPCChannel.SetChannelId(Convert.ToByte(e));

            Config.Save();
        }

        public static void Start()
        {
            Toggle = true;

            Chat.WriteLine("KHBuddy enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Toggle = false;

            Chat.WriteLine("KHBuddy disabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());

            MovementController.Instance.Halt();
        }

        private void OnStartStopMessage(int sender, IPCMessage msg)
        {
            if (msg is StartStopIPCMessage startStopMessage)
            {
                if (startStopMessage.IsStarting)
                {
                    // Update the setting and start the process.
                    _settings["Toggle"] = true;
                    Start();
                }
                else
                {
                    // Update the setting and stop the process.
                    _settings["Toggle"] = false;
                    Stop();
                }
            }
        }

        private void OnSideSelectionsMessage(int sender, IPCMessage msg)
        {
            if (msg is SideSelectionsIPCMessage sideSelectionsMessage)
            {
                currentSide = sideSelectionsMessage.Side;

                _settings["SideSelection"] = (int)currentSide;

                //Chat.WriteLine($"Received Mode: {currentMode}");
            }
        }

        private void OnMoveEastMessage(int sender, IPCMessage msg)
        {
            if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(1091.7f, 26.5f, 1051.4f)) > 1f && !MovementController.Instance.IsNavigating)
            {
                MovementController.Instance.SetDestination(new Vector3(1091.7f, 26.5f, 1051.4f));
            }
        }

        private void OnMoveWestMessage(int sender, IPCMessage msg)
        {
            if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(1064.4f, 25.6f, 1032.6f)) > 1f && !MovementController.Instance.IsNavigating)
            {
                MovementController.Instance.SetDestination(new Vector3(1064.4f, 25.6f, 1032.6f));
            }
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

                //Selection();

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

                    if (!_settings["Toggle"].AsBool() && Toggle)
                    {
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
                        Stop();
                    }
                    if (_settings["Toggle"].AsBool() && !Toggle)
                    {
                        
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = true });
                        Start();
                    }

                    SideSelection newSide = (SideSelection)_settings["SideSelection"].AsInt32();

                    bool sideChanged = newSide != currentSide;

                    if (sideChanged)
                    {
                        // Populate a SideSelectionsIPCMessage
                        SideSelectionsIPCMessage sideSelectionsMessage = new SideSelectionsIPCMessage
                        {
                            Side = newSide
                        };

                        // Broadcast the message
                        IPCChannel.Broadcast(sideSelectionsMessage);

                        // Update the current settings
                        if (sideChanged)
                        {
                            currentSide = newSide;
                        }
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

        //public static void Selection()
        //{
        //    if (SideSelection.Beach == (SideSelection)_settings["SideSelection"].AsInt32() && !_beachToggled)
        //    {
        //        Beach = true;
        //        East = false;
        //        West = false;
        //        EastandWest = false;

        //        _beachToggled = true;
        //        _eastToggled = false;
        //        _westToggled = false;
        //        _eastandWestToggled = false;

        //        Chat.WriteLine("Beach selected");
        //    }
        //    if (SideSelection.East == (SideSelection)_settings["SideSelection"].AsInt32() && !_eastToggled)
        //    {
        //        Beach = false;
        //        East = true;
        //        West = false;
        //        EastandWest = false;

        //        _beachToggled = false;
        //        _eastToggled = true;
        //        _westToggled = false;
        //        _eastandWestToggled = false;

        //        Chat.WriteLine("East selected");
        //    }
        //    if (SideSelection.West == (SideSelection)_settings["SideSelection"].AsInt32() && !_westToggled)
        //    {
        //        Beach = false;
        //        East = false;
        //        West = true;
        //        EastandWest = false;

        //        _beachToggled = false;
        //        _eastToggled = false;
        //        _westToggled = true;
        //        _eastandWestToggled = false;

        //        Chat.WriteLine("West selected");
        //    }
        //    if (SideSelection.EastAndWest == (SideSelection)_settings["SideSelection"].AsInt32() && !_eastandWestToggled)
        //    {
        //        Beach = false;
        //        East = false;
        //        West = false;
        //        EastandWest = true;

        //        _beachToggled = false;
        //        _eastToggled = false;
        //        _westToggled = false;
        //        _eastandWestToggled = true;

        //        Chat.WriteLine("East and West selected");
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
