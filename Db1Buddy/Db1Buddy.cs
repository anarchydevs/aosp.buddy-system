using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using Shared.IPCMessages;
using System;
using System.Diagnostics;

namespace Db1Buddy
{
    public class Db1Buddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static Identity Leader = Identity.None;
        public static SimpleChar _leader;
        public static Vector3 _leaderPos = Vector3.Zero;

        public static Vector3 _mikkelsenPos = Vector3.Zero;
        public static Vector3 _mikkelsenCorpsePos = Vector3.Zero;

        public static bool Toggle = false;
        public static bool Farming = false;

        public static bool MikkelsenCorpse = false;

        public static bool _died = false;

        public static double _stateTimeOut;

        public static Window _infoWindow;

        public static Settings _settings;

        public static string PluginDir;

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("Db1Buddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\Db1Buddy\\{DynelManager.LocalPlayer.Name}\\Config.json");
                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);
                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.StartStop, OnStartStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Farming, OnFarmingStatusMessage);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                Chat.RegisterCommand("buddy", BuddyCommand);

                SettingsController.RegisterSettingsWindow("Db1Buddy", pluginDir + "\\UI\\Db1BuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Team.TeamRequest += OnTeamRequest;
                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("Toggle", false);
                _settings.AddVariable("Farming", false);

                Chat.WriteLine("Db1Buddy Loaded!");
                Chat.WriteLine("/db1buddy for settings.");
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
            Toggle = true;

            Chat.WriteLine("Db1Buddy enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Toggle = false;

            Chat.WriteLine("Db1Buddy disabled.");

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
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\Db1BuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }


        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning)
                return;

            if (_settings["Toggle"].AsBool())
            {
                _stateMachine.Tick();
            }

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

                if (SettingsController.settingsWindow.FindView("Db1BuddyInfoView", out Button infoView))
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

        private void OnTeamRequest(object s, TeamRequestEventArgs e)
        {
            if (e.Requester != Leader)
            {
                if (Toggle)
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

        public static class Nanos
        {
            public const int ThriceBlessedbytheAncients = 269711;
            public const int BlessingoftheAncientMachinist = 269543;//Yellow get buff
            public const int BlessingoftheEternalCleric = 269543;//Red get buff
            public const int BlessingoftheAncientForm = 269534;//Green get buff
            public const int BlessingoftheEternalCraftsman = 269540;//Blue get buff

            public const int CallofRust = 270011; //blue
            public const int CrawlingSkin = 270010; //green
            public const int HealingBlight = 270013; //red
            public const int GreedoftheSource = 270012; //yellow

        }
    }
}