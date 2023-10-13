using System;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using AOSharp.Pathfinding;
using AOSharp.Core.IPC;
using AOSharp.Common.GameData.UI;
using AOSharp.Core.Movement;
using Shared.IPCMessages;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DB2Buddy
{
    public class DB2Buddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static bool Toggle = false;
        public static bool Farming = false;

        public static bool _init = false;
        public static bool _initLol = false;
        public static bool _initStart = false;
        public static bool _initTower = false;
        public static bool _initCorpse = false;
        public static bool IsLeader = false;
        public static bool _repeat = false;

        public static bool _taggedNotum = false;

        public static bool AuneCorpse = false;

        public static double _time = Time.NormalTime;

        public static Identity Leader = Identity.None;

        public static string PluginDirectory;

        public static Window _infoWindow;

        public static Settings _settings;

        public static List<Identity> _teamCache = new List<Identity>();

        public static List<Vector3> _mistLocations = new List<Vector3>();

        public static string PluginDir;

        private string previousErrorMessage = string.Empty;

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("DB2Buddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\DB2Buddy\\{DynelManager.LocalPlayer.Name}\\Config.json");

                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);

                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.StartStop, OnStartStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Farming, OnFarmingStatusMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Enter, OnEnterMessage);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                SettingsController.RegisterSettingsWindow("DB2Buddy", pluginDir + "\\UI\\DB2BuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Chat.WriteLine("DB2Buddy Loaded!");
                Chat.WriteLine("/db2buddy for settings.");

                _settings.AddVariable("Toggle", false);
                _settings.AddVariable("Farming", false);

                _settings["Toggle"] = false;
                _settings["Farming"] = false;

                Chat.RegisterCommand("buddy", BuddyCommand);

                Game.OnUpdate += OnUpdate;
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    previousErrorMessage = errorMessage;
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

            Chat.WriteLine("Db2Buddy enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Toggle = false;

            Chat.WriteLine("Db2Buddy disabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());

            NavMeshMovementController.Halt();
        }

        private void FarmingEnabled()
        {
            Chat.WriteLine("Farming Enabled.");
            Farming = true;
        }
        private void FarmingDisabled()
        {
            Chat.WriteLine("Farming Disabled");
            Farming = false;
        }

        private void OnEnterMessage(int sender, IPCMessage msg)
        {
            if (IsLeader)
                return;

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void OnStartStopMessage(int sender, IPCMessage msg)
        {
            if (msg is StartStopIPCMessage startStopMessage)
            {
                if (startStopMessage.IsStarting)
                {
                    if (Leader == Identity.None
                     && DynelManager.LocalPlayer.Identity.Instance != sender)
                    {
                        Leader = new Identity(IdentityType.SimpleChar, sender);
                    }
                        
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

        private void OnFarmingStatusMessage(int sender, IPCMessage msg)
        {
            if (msg is FarmingStatusMessage farmingStatusMessage)
            {

                if (farmingStatusMessage.IsFarming)
                {
                    _settings["Farming"] = true;
                    FarmingEnabled();
                }
                else
                {
                    _settings["Farming"] = false;
                    FarmingDisabled();
                }
            }
        }
       
        private void HandleInfoViewClick(object s, ButtonBase button)
        {
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\DB2BuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }

        private void OnUpdate(object s, float deltaTime)
        {
            try
            {
                if (Game.IsZoning)
                    return;

                Shared.Kits kitsInstance = new Shared.Kits();

                kitsInstance.SitAndUseKit();

                if (_settings["Toggle"].AsBool())
                {
                    _stateMachine.Tick();
                }

                if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
                {
                    SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);

                    if (channelInput != null)
                    {
                        if (int.TryParse(channelInput.Text, out int channelValue)
                            && Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel != channelValue)
                        {
                            Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel = channelValue;
                        }
                    }

                    if (SettingsController.settingsWindow.FindView("DB2BuddyInfoView", out Button infoView))
                    {
                        infoView.Tag = SettingsController.settingsWindow;
                        infoView.Clicked = HandleInfoViewClick;
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

                    if (!_settings["Farming"].AsBool() && Farming)// Farming is off
                    {
                        IPCChannel.Broadcast(new FarmingStatusMessage { IsFarming = false });
                        FarmingDisabled();
                    }
                    if (_settings["Farming"].AsBool() && !Farming) // Farming is on
                    {
                        IPCChannel.Broadcast(new FarmingStatusMessage { IsFarming = true });
                        FarmingEnabled();
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    previousErrorMessage = errorMessage;
                }
            }
        }

        private void BuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Toggle"].AsBool())
                    {
                        Leader = DynelManager.LocalPlayer.Identity;
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = true });
                        Start();
                    }
                    else
                    {
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
                        Stop();
                    }
                }
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        public static class RelevantItems
        {
            public static readonly int[] Kits = {
                297274, 293296, 291084, 291083, 291082
            };
        }

        public static class Nanos
        {
            public const int XanBlessingoftheEnemy = 274101; //boss heal
            public const int StrengthOfTheAncients = 273220;
            public const int SeismicActivity = 270742;
            public const int ActivatingtheMachine = 274200;
            public const int NotumPull = 274359;

            public const int PathtoElevation1 = 277947;
            public const int PathtoElevation2 = 277958;
            public const int PathtoElevation3 = 277959;
            public const int PathtoElevation4 = 277952;

        }
        private int GetLineNumber(Exception ex)
        {
            var lineNumber = 0;

            var lineMatch = Regex.Match(ex.StackTrace ?? "", @":line (\d+)$", RegexOptions.Multiline);

            if (lineMatch.Success)
                lineNumber = int.Parse(lineMatch.Groups[1].Value);

            return lineNumber;
        }
    }
}
