using System;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using AOSharp.Pathfinding;
using AOSharp.Core.IPC;
using AOSharp.Common.GameData.UI;
using AOSharp.Core.Movement;
using DB2Buddy.IPCMessages;
using AOSharp.Core.Inventory;
using System.Linq;
using System.Threading.Tasks;
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

        public static double _time;

        public static string PluginDirectory;

        public static Window _infoWindow;

        public static Settings _settings;

        public static Identity Leader = Identity.None;
        public static SimpleChar _leader;

        //public static List<Identity> _teamCache = new List<Identity>();

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

                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;

                SettingsController.RegisterSettingsWindow("DB2Buddy", pluginDir + "\\UI\\DB2BuddySettingWindow.xml", _settings);

                Chat.WriteLine("DB2Buddy Loaded!");
                Chat.WriteLine("/db2buddy for settings.");

                _settings.AddVariable("Toggle", false);

                _settings["Toggle"] = false;

                Chat.RegisterCommand("buddy", DB2BuddyCommand);

                _stateMachine = new StateMachine(new IdleState());

                Team.TeamRequest += OnTeamRequest;
                Game.OnUpdate += OnUpdate;
            }
            catch(Exception e)
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

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            Toggle = true;
            _settings["Toggle"] = true;
        }

        private void OnStopMessage(int sender, IPCMessage msg)
        {
            Toggle = false;
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

            if (_settings["Toggle"].AsBool())
            {

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
                        Toggle = true;
                    }
                }

                _stateMachine.Tick();
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
                Config.Save();
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
