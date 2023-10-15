using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using MitaarBuddy.IPCMessages;
using System;

namespace MitaarBuddy
{
    public class MitaarBuddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static Identity Leader = Identity.None;
        public static SimpleChar _leader;

        public static Vector3 _sinuhCorpsePos = Vector3.Zero;

        public static bool Enable = false;
        public static bool Farming = false;

        public static bool SinuhCorpse = false;

        public static bool _died = false;

        private bool previousStopAttack = false;
        private bool previousRed = false;
        private bool previousBlue = false;
        private bool previousYellow = false;
        private bool previousGreen = false;


        public static double _stateTimeOut;

        public static Window _infoWindow;

        public static Settings _settings;

        public static string PluginDir;

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("MitaarBuddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\MitaarBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");
                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);
                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.StartStop, OnStartStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Farming, OnFarmingStatusMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.SettingsUpdate, OnSettingsUpdateMessage);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                Chat.RegisterCommand("buddy", BuddyCommand);

                SettingsController.RegisterSettingsWindow("MitaarBuddy", pluginDir + "\\UI\\MitaarBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Team.TeamRequest += OnTeamRequest;
                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("Enable", false);
                _settings.AddVariable("Farming", false);
                _settings.AddVariable("Leader", false);

                _settings.AddVariable("StopAttack", false);
                _settings.AddVariable("Red", false);
                _settings.AddVariable("Blue", false);
                _settings.AddVariable("Yellow", false);
                _settings.AddVariable("Green", false);

                Chat.WriteLine("MitaarBuddy Loaded!");
                Chat.WriteLine("/mitaar for settings.");
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
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
            if (_settings["Leader"].AsBool())
            {
                Leader = DynelManager.LocalPlayer.Identity;
            }

            Enable = true;

            Chat.WriteLine("MitaarBuddy Enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Enable = false;

            Chat.WriteLine("MitaarBuddy disabled.");

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

        private void OnStartStopMessage(int sender, IPCMessage msg)
        {
            if (msg is StartStopIPCMessage startStopMessage)
            {
                if (startStopMessage.IsStarting)
                {
                    // Update the setting and start the process.
                    _settings["Enable"] = true;
                    Start();
                }
                else
                {
                    // Update the setting and stop the process.
                    _settings["Enable"] = false;
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

        private void OnSettingsUpdateMessage(int sender, IPCMessage msg)
        {
            if (msg is SettingsUpdateMessage settingsUpdateMessage)
            {
                _settings["StopAttack"] = settingsUpdateMessage.StopAttack;
                _settings["Red"] = settingsUpdateMessage.Red;
                _settings["Blue"] = settingsUpdateMessage.Blue;
                _settings["Yellow"] = settingsUpdateMessage.Yellow;
                _settings["Green"] = settingsUpdateMessage.Green;
            }
        }

        private void HandleInfoViewClick(object s, ButtonBase button)
        {
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\MitaarBuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }


        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning)
                return;

            Shared.Kits kitsInstance = new Shared.Kits();

            kitsInstance.SitAndUseKit();

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

                if (SettingsController.settingsWindow.FindView("MitaarBuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = HandleInfoViewClick;
                }

                if (!_settings["Enable"].AsBool() && Enable)
                {
                    IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
                    Stop();
                }
                if (_settings["Enable"].AsBool() && !Enable)
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

                BroadcastSettingsUpdateIfNeeded();
            }

            if (_settings["Enable"].AsBool())
            {
                _stateMachine.Tick();
            }
        }

        private void BroadcastSettingsUpdateIfNeeded()
        {
            bool stopAttack = _settings["StopAttack"].AsBool();
            bool red = _settings["Red"].AsBool();
            bool blue = _settings["Blue"].AsBool();
            bool yellow = _settings["Yellow"].AsBool();
            bool green = _settings["Green"].AsBool();

            if (stopAttack != previousStopAttack || red != previousRed || blue != previousBlue
                || yellow != previousYellow || green != previousGreen)
            {
                var settingsUpdateMessage = new SettingsUpdateMessage
                {
                    StopAttack = stopAttack,
                    Red = red,
                    Blue = blue,
                    Yellow = yellow,
                    Green = green
                };

                IPCChannel.Broadcast(settingsUpdateMessage);

                previousStopAttack = stopAttack;
                previousRed = red;
                previousBlue = blue;
                previousYellow = yellow;
                previousGreen = green;
            }
        }

        private void OnTeamRequest(object s, TeamRequestEventArgs e)
        {
            if (e.Requester != Leader)
            {
                if (Enable)
                    e.Ignore();

                return;
            }

            e.Accept();
        }

        private void BuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    bool currentEnable = _settings["Enable"].AsBool();

                    if (!currentEnable)
                    {
                        _settings["Enable"] = true;
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = true });
                        Start();
                    }
                    else
                    {
                        _settings["Enable"] = false;
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
                        Stop();
                    }
                }
                Config.Save();
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        public static class SpiritNanos
        {
            public const int BlessingofTheBlood = 280472; //Red
            public const int BlessingofTheSource = 280521; //Blue
            public const int BlessingofTheOutsider = 280493; //Green
            public const int BlessingofTheLight = 280496;  //Yellow
        }
    }
}