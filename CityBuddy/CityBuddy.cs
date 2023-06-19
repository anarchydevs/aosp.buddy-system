using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using CityBuddy.IPCMessages;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CityBuddy
{
    public class CityBuddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static DateTime cloakTime;

        public static Vector3 ParkPos;

        public static double _combatTime;
        public static double _cloakTime;
        public static double _sitUpdateTimer;

        public static bool Toggle = false;
        public static bool Sitting = false;

        public static string PluginDirectory;

        public static Window _infoWindow;

        public static Settings _settings;

        public static string PluginDir;

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("CityBuddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\CityBuddy\\{Game.ClientInst}\\Config.json");
                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);
                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);

                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;

                SettingsController.RegisterSettingsWindow("CityBuddy", pluginDir + "\\UI\\CityBuddySettingWindow.xml", _settings);

                Chat.WriteLine("CityBuddy Loaded!");
                Chat.WriteLine("/citybuddy for settings.");

                _stateMachine = new StateMachine(new IdleState());

                _settings.AddVariable("Toggle", false);

                _settings["Toggle"] = false;

                Chat.RegisterCommand("buddy", CityBuddyCommand);

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
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\CityBuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning)
                return;

            if (Time.NormalTime > _sitUpdateTimer + 1)
            {
                ListenerSit();

                _sitUpdateTimer = Time.NormalTime;
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

                if (SettingsController.settingsWindow.FindView("CityBuddyInfoView", out Button infoView))
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
                                   await Task.Delay(400);
                                   NavMeshMovementController.SetMovement(MovementAction.SwitchToSit);
                                   await Task.Delay(800);
                                   NavMeshMovementController.SetMovement(MovementAction.LeaveSit);
                                   await Task.Delay(200);
                                   Sitting = false;
                               });
                        }
                    }
                }
            }
        }

        private void CityBuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Toggle"].AsBool())
                    {
                        _settings["Toggle"] = true;
                        Toggle = true;
                        Chat.WriteLine("Bot enabled.");
                    }
                    else if (_settings["Toggle"].AsBool())
                    {
                        _settings["Toggle"] = false;
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
    }
}
