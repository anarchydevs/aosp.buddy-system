﻿using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using Db1Buddy.IPCMessages;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        //public static bool _initCorpse = false;

        public static bool Easy = false;
        public static bool _easyToggled = false;
        public static bool Medium = false;
        public static bool _mediumToggled = false;
        public static bool Hardcore = false;
        public static bool _hardcoreToggled = false;

        public static bool Sitting = false;
        public static bool _died = false;


        public static double _stateTimeOut;
        public static double _sitUpdateTimer;

        public static Window _infoWindow;

        public static Settings _settings;

        public static string PluginDir;

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("Db1Buddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\Db1Buddy\\{Game.ClientInst}\\Config.json");
                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);
                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);

                IPCChannel.RegisterCallback((int)IPCOpcode.Farming, FarmingMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.NoFarming, NoFarmingMessage);

                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;

                Chat.RegisterCommand("buddy", BuddyCommand);

                SettingsController.RegisterSettingsWindow("Db1Buddy", pluginDir + "\\UI\\Db1BuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Team.TeamRequest += OnTeamRequest;
                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("DifficultySelection", (int)DifficultySelection.Easy);

                _settings.AddVariable("Toggle", false);
                _settings.AddVariable("Farming", false);

                _settings["Toggle"] = false;
                _settings["Farming"] = false;

                _settings["DifficultySelection"] = (int)DifficultySelection.Easy;

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

        private void farmingEnabled()
        {
            Farming = true;
        }
        private void farmingDisabled()
        {
            Farming = false;
        }

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            if (Leader == Identity.None
                && DynelManager.LocalPlayer.Identity.Instance != sender)
                Leader = new Identity(IdentityType.SimpleChar, sender);

            _settings["Toggle"] = true;
            Start();
        }

        private void OnStopMessage(int sender, IPCMessage msg)
        {
            _settings["Toggle"] = false;
            Stop();
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

                if (SettingsController.settingsWindow.FindView("Db1BuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = HandleInfoViewClick;
                }

                if (!_settings["Toggle"].AsBool() && Toggle)
                {
                    IPCChannel.Broadcast(new StopMessage());
                    Stop();
                }
                if (_settings["Toggle"].AsBool() && !Toggle)
                {
                    Leader = DynelManager.LocalPlayer.Identity;
                    IPCChannel.Broadcast(new StartMessage());
                    Start();
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

        private void BuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Toggle"].AsBool() && !Toggle)
                    {

                        IPCChannel.Broadcast(new StartMessage());
                        Start();
                    }
                    else
                    {
                        IPCChannel.Broadcast(new StopMessage());
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

        public enum DifficultySelection
        {
            Easy, Medium, Hardcore
        }

        public static class RelevantItems
        {
            public static readonly int[] Kits = {
                297274, 293296, 291084, 291083, 291082
            };
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