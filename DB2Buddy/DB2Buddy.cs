using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using DB2Buddy.IPCMessages;
using System;
using System.Collections.Generic;

namespace DB2Buddy
{
    public class DB2Buddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static bool Toggle = false;
        public static bool _init = false;
        public static bool _initLol = false;
        public static bool _initStart = false;
        public static bool _initTower = false;
        public static bool _initCorpse = false;
        public static bool IsLeader = false;
        public static bool _repeat = false;

        public static double _time = Time.NormalTime;

        public static Identity Leader = Identity.None;

        public static string PluginDirectory;

        public static Window _infoWindow;

        public static Settings _settings;

        public static List<Identity> _teamCache = new List<Identity>();

        public static List<Vector3> _towerPositions = new List<Vector3>()
        {
            new Vector3(294.2f, 135.3f, 199.0f),
            new Vector3(250.9f, 135.3f, 225.1f),
            new Vector3(277.0f, 135.3f, 267.1f),
            new Vector3(320.2f, 135.3f, 242.1f)
        };

        public static List<Vector3> _mistLocations = new List<Vector3>();

        public static string PluginDir;


        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("DB2Buddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\DB2Buddy\\{Game.ClientInst}\\Config.json");

                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);

                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Enter, OnEnterMessage);

                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;

                SettingsController.RegisterSettingsWindow("DB2Buddy", pluginDir + "\\UI\\DB2BuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Chat.WriteLine("DB2Buddy Loaded!");
                Chat.WriteLine("/db2buddy for settings.");

                _settings.AddVariable("Toggle", false);

                _settings["Toggle"] = false;

                Chat.RegisterCommand("buddy", DB2BuddyCommand);

                Game.OnUpdate += OnUpdate;
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

        private void OnEnterMessage(int sender, IPCMessage msg)
        {
            if (IsLeader)
                return;

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            Toggle = true;

            if (Leader == Identity.None
                && DynelManager.LocalPlayer.Identity.Instance != sender)
                Leader = new Identity(IdentityType.SimpleChar, sender);

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());

            _settings["Toggle"] = true;
        }

        private void OnStopMessage(int sender, IPCMessage msg)
        {
            Toggle = false;

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());

            _settings["Toggle"] = false;
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
            if (Game.IsZoning)
                return;

            _stateMachine.Tick();

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);

                if (channelInput != null)
                {
                    if (int.TryParse(channelInput.Text, out int channelValue)
                        && Config.CharSettings[Game.ClientInst].IPCChannel != channelValue)
                    {
                        Config.CharSettings[Game.ClientInst].IPCChannel = channelValue;
                    }
                }

                if (SettingsController.settingsWindow.FindView("DB2BuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = HandleInfoViewClick;
                }

                if (!_settings["Toggle"].AsBool() && Toggle)
                {
                    IPCChannel.Broadcast(new StopMessage());
                    Toggle = false;
                }
                if (_settings["Toggle"].AsBool() && !Toggle)
                {
                    IPCChannel.Broadcast(new StartMessage());
                    if (Team.IsLeader)
                    {
                        IsLeader = true;
                        Leader = DynelManager.LocalPlayer.Identity;
                    }
                    Toggle = true;
                }
            }
        }

        private void DB2BuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Toggle"].AsBool())
                    {
                        _settings["Toggle"] = true;
                        IPCChannel.Broadcast(new StartMessage());
                        Toggle = true;
                        IsLeader = true;
                        Leader = DynelManager.LocalPlayer.Identity;
                        Chat.WriteLine("Bot enabled.");
                    }
                    else if (_settings["Toggle"].AsBool())
                    {
                        _settings["Toggle"] = false;
                        IPCChannel.Broadcast(new StopMessage());
                        Toggle = false;
                        Chat.WriteLine("Bot disabled.");
                    }
                }
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
