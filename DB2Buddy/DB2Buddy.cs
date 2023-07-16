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
using System.Text.RegularExpressions;

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

        public static bool Sitting = false;

        public static bool AuneCorpse = false;

        public static double _time = Time.NormalTime;
        public static double _sitUpdateTimer;

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

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\DB2Buddy\\{Game.ClientInst}\\Config.json");

                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);

                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Enter, OnEnterMessage);

                IPCChannel.RegisterCallback((int)IPCOpcode.Farming, FarmingMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.NoFarming, NoFarmingMessage);

                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;

                SettingsController.RegisterSettingsWindow("DB2Buddy", pluginDir + "\\UI\\DB2BuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Chat.WriteLine("DB2Buddy Loaded!");
                Chat.WriteLine("/db2buddy for settings.");

                _settings.AddVariable("Toggle", false);
                _settings.AddVariable("Farming", false);

                _settings["Toggle"] = false;
                _settings["Farming"] = false;

                Chat.RegisterCommand("buddy", DB2BuddyCommand);

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

        private void farmingEnabled()
        {
            Farming = true;
        }
        private void farmingDisabled()
        {
            Farming = false;
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

        private void FarmingMessage(int sender, IPCMessage msg)
        {
            _settings["Farming"] = true;
            farmingEnabled();
        }

        private void NoFarmingMessage(int sender, IPCMessage msg)
        {
            _settings["Farming"] = false;
            farmingDisabled();
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

                if (Time.NormalTime > _sitUpdateTimer + 1)
                {
                    ListenerSit();

                    _sitUpdateTimer = Time.NormalTime;
                }

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

                    if (!_settings["Farming"].AsBool() && Farming) // Farming off
                    {
                        IPCChannel.Broadcast(new NoFarmingMessage());
                        Chat.WriteLine("Farming disabled");
                        farmingDisabled();
                    }

                    if (_settings["Farming"].AsBool() && !Farming) // farming on
                    {
                        IPCChannel.Broadcast(new FarmingMessage());
                        Chat.WriteLine("Farming enabled.");
                        farmingEnabled();
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

        private void ListenerSit()
        {
            Spell spell = Spell.List.FirstOrDefault(x => x.IsReady);

            Item kit = Inventory.Items.Where(x => RelevantItems.Kits.Contains(x.Id)).FirstOrDefault();

            if (kit == null) { return; }

            if (spell != null)
            {
                if (!DynelManager.LocalPlayer.Buffs.Contains(280488) && Extensions.CanUseSitKit())
                {
                    if (spell != null && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment) && Sitting == false
                        && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
                    {
                        if (DynelManager.LocalPlayer.NanoPercent < 66 || DynelManager.LocalPlayer.HealthPercent < 66)
                        {
                            Task.Factory.StartNew(
                               async () =>
                               {
                                   Sitting = true;
                                   await Task.Delay(500);
                                   NavMeshMovementController.SetMovement(MovementAction.SwitchToSit);
                                   await Task.Delay(800);
                                   NavMeshMovementController.SetMovement(MovementAction.LeaveSit);
                                   await Task.Delay(500);
                                   Sitting = false;
                               });
                        }
                    }
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
