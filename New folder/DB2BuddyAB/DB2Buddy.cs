using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Core.Movement;
using AOSharp.Common.GameData;
using System.IO;
using AOSharp.Core.GameData;
using AOSharp.Core.UI.Options;
using AOSharp.Pathfinding;
using System.Data;
using AOSharp.Core.IPC;
using DB2Buddy.IPCMessages;
using AOSharp.Core.Inventory;
using System.Collections.Concurrent;
using AOSharp.Common.GameData.UI;
using System.Globalization;

namespace DB2Buddy
{
    public class DB2Buddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;

        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static int AttackRange;
        public static int WarpRange;

        public static bool Toggle = false;
        public static bool _init = false;

        public static Identity Leader = Identity.None;
        public static bool IsLeader = false;

        public static double _stateTimeOut = Time.NormalTime;

        public static List<SimpleChar> _mob = new List<SimpleChar>();

        private static double _refreshList;

        private static Window _infoWindow;

        public static string PluginDir;

        public static Settings _settings;


        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("DB2Buddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\DB2Buddy\\{Game.ClientInst}\\Config.json");
                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.AttackRange, OnAttackRangeMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.WarpRange, OnWarpRangeMessage);

                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;
                Config.CharSettings[Game.ClientInst].AttackRangeChangedEvent += AttackRange_Changed;
                Config.CharSettings[Game.ClientInst].WarpRangeChangedEvent += WarpRange_Changed;

                Chat.RegisterCommand("buddy", DB2BuddyCommand);

                SettingsController.RegisterSettingsWindow("DB2Buddy", pluginDir + "\\UI\\DB2BuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("Toggle", false);

                _settings["Toggle"] = false;

                Chat.WriteLine("DB2Buddy Loaded!");
                Chat.WriteLine("/DB2Buddy for settings.");

                AttackRange = Config.CharSettings[Game.ClientInst].AttackRange;
                WarpRange = Config.CharSettings[Game.ClientInst].WarpRange;
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

        public Window[] _windows => new Window[] { };

        public static void IPCChannel_Changed(object s, int e)
        {
            IPCChannel.SetChannelId(Convert.ToByte(e));
            Config.Save();
        }

        public static void AttackRange_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].AttackRange = e;
            AttackRange = e;
            Config.Save();
        }
        public static void WarpRange_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].WarpRange = e;
            WarpRange = e;
            Config.Save();
        }

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            if (DynelManager.LocalPlayer.Identity == Leader)
                return;

            Toggle = true;

            _settings["Toggle"] = true;


            Leader = new Identity(IdentityType.SimpleChar, sender);

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void OnStopMessage(int sender, IPCMessage msg)
        {
            StopMessage stopMsg = (StopMessage)msg;

            Toggle = false;

            _settings["Toggle"] = false;

            _stateMachine.SetState(new IdleState());


            if (DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.StopAttack();
            if (MovementController.Instance.IsNavigating)
                MovementController.Instance.Halt();
        }

        private void OnAttackRangeMessage(int sender, IPCMessage msg)
        {
            AttackRangeMessage rangeMsg = (AttackRangeMessage)msg;

            Config.CharSettings[Game.ClientInst].AttackRange = rangeMsg.Range;
        }

        private void OnWarpRangeMessage(int sender, IPCMessage msg)
        {
            WarpRangeMessage rangeMsg = (WarpRangeMessage)msg;

            Config.CharSettings[Game.ClientInst].WarpRange = rangeMsg.Range;
        }

        private void Start()
        {
            Toggle = true;

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Toggle = false;

            _settings["Toggle"] = false;

            _stateMachine.SetState(new IdleState());

            if (DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.StopAttack();
            if (MovementController.Instance.IsNavigating)
                MovementController.Instance.Halt();
        }

        private void HandleInfoViewClick(object s, ButtonBase button)
        {
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\DB2BuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }

        private void Scanning()
        {
            {
               
                _mob = DynelManager.NPCs
                   .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= WarpRange
                       && !Constants._ignores.Contains(c.Name)
                       && c.Health > 0
                       && Extensions.IsFightingAny(c))
                   .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                   .OrderByDescending(c => c.Name == "Notum Irregularity")
                   .OrderByDescending(c => c.Name == "Strange Xan Artifact")
                   .OrderByDescending(c => c.Name == "Ground Chief Aune")
                   .ToList();
            }

            _refreshList = Time.NormalTime;
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning)
                return;

            if (Time.NormalTime > _refreshList + 0.5f
                && Toggle == true)
                Scanning();

            var window = SettingsController.FindValidWindow(_windows);

            if (window != null && window.IsValid)
            {

            }

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);
                SettingsController.settingsWindow.FindView("AttackRangeBox", out TextInputView attackRangeInput);
                SettingsController.settingsWindow.FindView("WarpRangeBox", out TextInputView warpRangeInput);

                if (channelInput != null)
                {
                    if (int.TryParse(channelInput.Text, out int channelValue)
                        && Config.CharSettings[Game.ClientInst].IPCChannel != channelValue)
                    {
                        Config.CharSettings[Game.ClientInst].IPCChannel = channelValue;
                    }
                }
                if (attackRangeInput != null && !string.IsNullOrEmpty(attackRangeInput.Text))
                {
                    if (int.TryParse(attackRangeInput.Text, out int attackRangeInputValue)
                        && Config.CharSettings[Game.ClientInst].AttackRange != attackRangeInputValue)
                    {
                        Config.CharSettings[Game.ClientInst].AttackRange = attackRangeInputValue;
                        IPCChannel.Broadcast(new AttackRangeMessage()
                        {
                            Range = attackRangeInputValue
                        });
                    }
                }
                if (warpRangeInput != null && !string.IsNullOrEmpty(warpRangeInput.Text))
                {
                    if (int.TryParse(warpRangeInput.Text, out int warpRangeInputValue)
                        && Config.CharSettings[Game.ClientInst].WarpRange != warpRangeInputValue)
                    {
                        Config.CharSettings[Game.ClientInst].WarpRange = warpRangeInputValue;
                        IPCChannel.Broadcast(new WarpRangeMessage()
                        {
                            Range = warpRangeInputValue
                        });
                    }
                }

                if (SettingsController.settingsWindow.FindView("DB2BuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = HandleInfoViewClick;
                }

                if (_settings["Toggle"].AsBool() && !Toggle)
                {
                    IsLeader = true;
                    Leader = DynelManager.LocalPlayer.Identity;

                    if (DynelManager.LocalPlayer.Identity == Leader)
                        IPCChannel.Broadcast(new StartMessage());

                    Chat.WriteLine("DB2Buddy enabled.");
                    Start();
                }
                if (!_settings["Toggle"].AsBool() && Toggle)
                {
                    Stop();
                    Chat.WriteLine("DB2Buddy disabled.");
                    IPCChannel.Broadcast(new StopMessage());
                }

            }

            _stateMachine.Tick();

        }

        private void DB2BuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Toggle"].AsBool() && !Toggle)
                    {
                        IsLeader = true;
                        Leader = DynelManager.LocalPlayer.Identity;

                        if (DynelManager.LocalPlayer.Identity == Leader)
                            IPCChannel.Broadcast(new StartMessage());

                        _settings["Toggle"] = true;
                        Chat.WriteLine("DB2Buddy enabled.");
                        Start();

                    }
                    else 
                    {
                        Stop();
                        Chat.WriteLine("DB2Buddy disabled.");
                        IPCChannel.Broadcast(new StopMessage());
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
