using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Pathfinding;
using AOSharp.Core.UI;
using VortexxBuddy.IPCMessages;
using System;
using System.Diagnostics;

namespace VortexxBuddy
{
    public class VortexxBuddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static Identity Leader = Identity.None;
        public static SimpleChar _leader;
        public static Vector3 _leaderPos = Vector3.Zero;

        public static Vector3 _vortexxPos = Vector3.Zero;
        public static Vector3 _vortexxCorpsePos = Vector3.Zero;

        private Stopwatch _kitTimer = new Stopwatch();

        public static bool Toggle = false;
        public static bool Farming = false;
        public static bool _clearToEnter = false;

        public static bool VortexxCorpse = false;

        public static bool Sitting = false;
        public static bool _died = false;

        public static bool _yellow = false;
        public static bool _blue = false;
        public static bool _green = false;
        public static bool _red = false;

        public static double _stateTimeOut;
        private static double _time;

        public static Window _infoWindow;

        public static Settings _settings;

        public static string PluginDir;

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("VortexxBuddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\VortexxBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");
                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);
                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.StartStop, OnStartStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Farming, OnFarmingStatusMessage);

                IPCChannel.RegisterCallback((int)IPCOpcode.Enter, EnterMessage);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                Chat.RegisterCommand("buddy", BuddyCommand);

                SettingsController.RegisterSettingsWindow("VortexxBuddy", pluginDir + "\\UI\\VortexxBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Team.TeamRequest += OnTeamRequest;
                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("Toggle", false);
                _settings["Toggle"] = false;

                _settings.AddVariable("Farming", false);
                _settings.AddVariable("Leader", false);
                _settings.AddVariable("Immunity", false);
                _settings.AddVariable("Clear", false);

                Chat.WriteLine("VortexxBuddy Loaded!");
                Chat.WriteLine("/vortbuddy for settings.");
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

            if (_settings["Leader"].AsBool())
            {
                Leader = DynelManager.LocalPlayer.Identity;
            }

            Chat.WriteLine("VortexxBuddy enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Toggle = false;

            Chat.WriteLine("VortexxBuddy disabled.");

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

        private void EnterMessage(int sender, IPCMessage msg)
        {
            if (!(_stateMachine.CurrentState is EnterState))
                _stateMachine.SetState(new EnterState());
        }

        private void HandleInfoViewClick(object s, ButtonBase button)
        {
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\VortexxBuddyInfoView.xml",
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

                if (SettingsController.settingsWindow.FindView("VortexxBuddyInfoView", out Button infoView))
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

            if (_settings["Toggle"].AsBool())
            {
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
            public const int CrystalBossShapeChanger = 280867; //boss change to spider

            public const int NanoInfusion = 280870; //anti fear buf

            public const int BloodRedNanoInfusion = 280559;
            public const int CobaltBlueNanoInfusion = 280560;
            public const int PulsatingGreenNanoInfusion = 280561;
            public const int GoldenNanoInfusion = 280562;

            public const int AncientMist = 280799; // nano drain
            public const int EmptyHusk = 280731;
            public const int CreepingIllness = 280751;
            public const int FlamesofConsequence = 280753;

            public const int Terrified = 280868; // fear

        }
    }
}