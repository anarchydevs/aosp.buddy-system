using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using AttackBuddy.IPCMessages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AttackBuddy
{
    public class AttackBuddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static int AttackRange;
        public static int ScanRange;

        public static bool Toggle = false;
        public static bool _init = false;

        public static Identity Leader = Identity.None;
        public static bool IsLeader = false;

        public static double _stateTimeOut = Time.NormalTime;

        public static List<SimpleChar> _mob = new List<SimpleChar>();
        public static List<SimpleChar> _bossMob = new List<SimpleChar>();
        public static List<SimpleChar> _switchMob = new List<SimpleChar>();

        public static List<SimpleChar> _switchMobPrecision = new List<SimpleChar>();
        public static List<SimpleChar> _switchMobCharging = new List<SimpleChar>();
        public static List<SimpleChar> _switchMobShield = new List<SimpleChar>();

        private static double _refreshList;

        private static Window _infoWindow;
        private static Window _helperWindow;

        private static View _helperView;

        public static string PluginDir;

        public static Settings _settings;

        public static List<string> _helpers = new List<string>();

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("AttackBuddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\AOSP\\AttackBuddy\\{Game.ClientInst}\\Config.json");
                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.AttackRange, OnAttackRangeMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.ScanRange, OnScanRangeMessage);

                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;
                Config.CharSettings[Game.ClientInst].AttackRangeChangedEvent += AttackRange_Changed;
                Config.CharSettings[Game.ClientInst].ScanRangeChangedEvent += ScanRange_Changed;

                Chat.RegisterCommand("buddy", AttackBuddyCommand);

                SettingsController.RegisterSettingsWindow("AttackBuddy", pluginDir + "\\UI\\AttackBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("Toggle", false);
                _settings.AddVariable("Taunt", false);

                _settings["Toggle"] = false;

                Chat.WriteLine("AttackBuddy Loaded!");
                Chat.WriteLine("/attackbuddy for settings.");

                AttackRange = Config.CharSettings[Game.ClientInst].AttackRange;
                ScanRange = Config.CharSettings[Game.ClientInst].ScanRange;
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

        public Window[] _windows => new Window[] { _helperWindow };

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
        public static void ScanRange_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].ScanRange = e;
            ScanRange = e;
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

        private void OnScanRangeMessage(int sender, IPCMessage msg)
        {
            ScanRangeMessage rangeMsg = (ScanRangeMessage)msg;

            Config.CharSettings[Game.ClientInst].ScanRange = rangeMsg.Range;
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
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\AttackBuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }

        private void HandleHelpersViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Do we need this?
                if (window.Views.Contains(_helperView)) { return; }

                _helperView = View.CreateFromXml(PluginDir + "\\UI\\AttackBuddyHelpersView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Helpers", XmlViewName = "AttackBuddyHelpersView" }, _helperView);
            }
            else if (_helperWindow == null || (_helperWindow != null && !_helperWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_helperWindow, PluginDir, new WindowOptions() { Name = "Helpers", XmlViewName = "AttackBuddyHelpersView" }, _helperView, out var container);
                _helperWindow = container;
            }
        }

        private void HandleAddHelperViewClick(object s, ButtonBase button)
        {
            var window = SettingsController.FindValidWindow(_windows);

            if (window != null && window.IsValid)
            {
                window.FindView("HelperNameBox", out TextInputView nameInput);

                if (nameInput != null && !string.IsNullOrEmpty(nameInput.Text))
                {
                    _helpers.Add(nameInput.Text);
                }
            }
        }
        private void HandleRemoveHelperViewClick(object s, ButtonBase button)
        {
            var window = SettingsController.FindValidWindow(_windows);

            if (window != null && window.IsValid)
            {
                window.FindView("HelperNameBox", out TextInputView nameInput);

                if (nameInput != null && !string.IsNullOrEmpty(nameInput.Text))
                {
                    _helpers.Remove(nameInput.Text);
                }
            }
        }

        private void HandleClearHelpersViewClick(object s, ButtonBase button)
        {
            _helpers.Clear();
        }

        private void HandlePrintHelpersViewClick(object s, ButtonBase button)
        {
            foreach (string str in _helpers)
                Chat.WriteLine(str);
        }

        private void Scanning()
        {
            if (Playfield.ModelIdentity.Instance == 6123)
            {
                _switchMob = DynelManager.NPCs
                    .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                        && !Constants._ignores.Contains(c.Name)
                        && c.Name != "Alien Cocoon" && c.Name != "Alien Coccoon"
                        && c.Name != "Zix"
                        && c.Health > 0 && c.IsInLineOfSight && c.Name != "Kyr'Ozch Maid"
                        && c.Name != "Kyr'Ozch Technician" && c.Name != "Defense Drone Tower"
                        && c.Name != "Control Leech"
                        && c.Name != "Alien Precision Tower" && c.Name != "Alien Charging Tower"
                        && c.Name != "Alien Shield Tower" && c.Name != "Kyr'Ozch Technician"
                        && c.MaxHealth < 1000000 && Extensions.IsFightingAny(c))
                    .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                    .OrderBy(c => c.HealthPercent)
                    .OrderByDescending(c => c.Name == "Kyr'Ozch Nurse")
                    .OrderByDescending(c => c.Name == "Kyr'Ozch Offspring")
                    .OrderByDescending(c => c.Name == "Rimah Corsuezo")
                    .ToList();

                _switchMobPrecision = DynelManager.NPCs
                    .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                        && Extensions.BossHasCorrespondingBuff(287525) && c.Name == "Alien Precision Tower")
                    .ToList();

                _switchMobCharging = DynelManager.NPCs
                    .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                        && Extensions.BossHasCorrespondingBuff(287515) && c.Name == "Alien Charging Tower")
                    .ToList();

                _switchMobShield = DynelManager.NPCs
                    .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                        && Extensions.BossHasCorrespondingBuff(287526) && c.Name == "Alien Shield Tower")
                    .ToList();

                _mob = DynelManager.Characters
                    .Where(c => !c.IsPlayer && c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                        && !Constants._ignores.Contains(c.Name)
                        && c.Name != "Zix" && !c.Name.Contains("sapling") && c.Health > 0
                        && c.IsInLineOfSight && c.MaxHealth < 1000000 && Extensions.IsFightingAny(c)
                        && (!c.IsPet || c.Name == "Drop Trooper - Ilari'Ra"))
                    .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                    .OrderBy(c => c.HealthPercent)
                    .ToList();

            }
            if (Playfield.ModelIdentity.Instance == 6015)
            {
                _bossMob = DynelManager.NPCs
                    .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                        && !Constants._ignores.Contains(c.Name)
                        && c.Health > 0 && c.IsInLineOfSight
                        && !c.Buffs.Contains(253953) && !c.Buffs.Contains(205607)
                        && c.MaxHealth >= 1000000)
                    .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                    .OrderByDescending(c => c.Name == "Right Hand of Madness")
                    .OrderByDescending(c => c.Name == "Left Hand of Insanity")
                    .ToList();

                _mob = DynelManager.Characters
                    .Where(c => !c.IsPlayer && c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                        && !Constants._ignores.Contains(c.Name)
                        && c.Health > 0
                        && c.IsInLineOfSight && c.MaxHealth < 1000000 && Extensions.IsFightingAny(c)
                        && (!c.IsPet))
                    .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                    .OrderBy(c => c.HealthPercent)
                    .OrderByDescending(c => c.Name == "Green Tower")
                    .OrderByDescending(c => c.Name == "Blue Tower")
                    .ToList();
            }
            else
            {
                _bossMob = DynelManager.NPCs
                    .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                        && !Constants._ignores.Contains(c.Name)
                        && c.Health > 0 && c.IsInLineOfSight
                        && !c.Buffs.Contains(253953) && !c.Buffs.Contains(205607)
                        //&& !c.Buffs.Contains(302745)
                        //&& !c.Buffs.Contains(NanoLine.ShovelBuffs)
                        && c.MaxHealth >= 1000000)
                    .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                    .OrderByDescending(c => c.Name == "Uklesh the Beguiling")
                    .OrderByDescending(c => c.Name == "Khalum the Weaver of Flesh")
                    .OrderByDescending(c => c.Name == "Field Support  - Cha'Khaz")

                    .ToList();

                _switchMob = DynelManager.NPCs
                   .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                       && !Constants._ignores.Contains(c.Name)
                       && c.Name != "Zix" && !c.Name.Contains("sapling")
                       && c.Health > 0 && c.IsInLineOfSight && c.MaxHealth < 1000000
                       && Extensions.IsFightingAny(c) && (c.Name == "Devoted Fanatic" || c.Name == "Hallowed Acolyte" || c.Name == "Hand of the Colonel"
                    || c.Name == "Hacker'Uri" || c.Name == "The Sacrifice" || c.Name == "Corrupted Xan-Len"
                     || c.Name == "Blue Tower" || c.Name == "Green Tower" || c.Name == "Drone Harvester - Jaax'Sinuh"
                      || c.Name == "Support Sentry - Ilari'Uri" || c.Name == "Fanatic" || c.Name == "Alien Coccoon" || c.Name == "Alien Cocoon" || c.Name == "Stasis Containment Field"))
                   .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                   .OrderBy(c => c.HealthPercent)
                   .OrderByDescending(c => c.Name == "Drone Harvester - Jaax'Sinuh")
                   .OrderByDescending(c => c.Name == "Lost Thought")
                   .OrderByDescending(c => c.Name == "Support Sentry - Ilari'Uri")
                   .OrderByDescending(c => c.Name == "Ruinous Reverends")
                   .OrderByDescending(c => c.Name == "Alien Cocoon")
                   .OrderByDescending(c => c.Name == "Alien Coccoon" && c.MaxHealth < 40001)
                   .ToList();

                _mob = DynelManager.Characters
                    .Where(c => !c.IsPlayer && c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                        && !Constants._ignores.Contains(c.Name)
                        && c.Name != "Zix" && !c.Name.Contains("sapling") && c.Health > 0
                        && c.IsInLineOfSight && c.MaxHealth < 1000000 && Extensions.IsFightingAny(c)
                        && (!c.IsPet || c.Name == "Drop Trooper - Ilari'Ra"))
                    .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                    .OrderBy(c => c.HealthPercent)
                    .OrderByDescending(c => c.Name == "Corrupted Hiisi Berserker")
                    .OrderByDescending(c => c.Name == "Corrupted Xan-Cur")
                    .OrderByDescending(c => c.Name == "Corrupted Xan-Kuir")
                    .OrderByDescending(c => c.Name == "Cultist Silencer")
                    .OrderByDescending(c => c.Name == "Drone Harvester - Jaax'Sinuh")
                    .OrderByDescending(c => c.Name == "Support Sentry - Ilari'Uri")
                    .OrderByDescending(c => c.Name == "Alien Cocoon")
                    .OrderByDescending(c => c.Name == "Alien Coccoon" && c.MaxHealth < 40001)
                    .OrderByDescending(c => c.Name == "Stim Fiend")
                    .OrderByDescending(c => c.Name == "Lost Thought")
                    .OrderByDescending(c => c.Name == "Masked Operator")
                    .OrderByDescending(c => c.Name == "Masked Technician")
                    .OrderByDescending(c => c.Name == "Masked Engineer")
                    .OrderByDescending(c => c.Name == "Masked Superior Commando")
                    .OrderByDescending(c => c.Name == "Green Tower")
                    .OrderByDescending(c => c.Name == "Blue Tower")
                    .OrderByDescending(c => c.Name == "The Sacrifice")
                    .OrderByDescending(c => c.Name == "Hacker'Uri")
                    .OrderByDescending(c => c.Name == "Hand of the Colonel")
                    .OrderByDescending(c => c.Name == "Corrupted Xan-Len")
                   .OrderByDescending(c => c.Name == "Ruinous Reverends")
                    .OrderByDescending(c => c.Name == "Hallowed Acolyte")
                    .OrderByDescending(c => c.Name == "Devoted Fanatic")
                    .ToList();
            }

            _refreshList = Time.NormalTime;
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning)
            {
                Toggle = false;
                _settings["Toggle"] = false;

                return;
            }


            if (Time.NormalTime > _refreshList + 0.5f
                && Toggle == true)
                Scanning();

            var window = SettingsController.FindValidWindow(_windows);

            if (window != null && window.IsValid)
            {
                if (window.FindView("AttackBuddyAddHelper", out Button addHelperView))
                {
                    addHelperView.Tag = window;
                    addHelperView.Clicked = HandleAddHelperViewClick;
                }

                if (window.FindView("AttackBuddyRemoveHelper", out Button removeHelperView))
                {
                    removeHelperView.Tag = window;
                    removeHelperView.Clicked = HandleRemoveHelperViewClick;
                }

                if (window.FindView("AttackBuddyClearHelpers", out Button clearHelpersView))
                {
                    clearHelpersView.Tag = window;
                    clearHelpersView.Clicked = HandleClearHelpersViewClick;
                }

                if (window.FindView("AttackBuddyPrintHelpers", out Button printHelpersView))
                {
                    printHelpersView.Tag = window;
                    printHelpersView.Clicked = HandlePrintHelpersViewClick;
                }
            }

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);
                SettingsController.settingsWindow.FindView("AttackRangeBox", out TextInputView attackRangeInput);
                SettingsController.settingsWindow.FindView("ScanRangeBox", out TextInputView scanRangeInput);

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
                if (scanRangeInput != null && !string.IsNullOrEmpty(scanRangeInput.Text))
                {
                    if (int.TryParse(scanRangeInput.Text, out int scanRangeInputValue)
                        && Config.CharSettings[Game.ClientInst].ScanRange != scanRangeInputValue)
                    {
                        Config.CharSettings[Game.ClientInst].ScanRange = scanRangeInputValue;
                        IPCChannel.Broadcast(new ScanRangeMessage()
                        {
                            Range = scanRangeInputValue
                        });
                    }
                }

                if (SettingsController.settingsWindow.FindView("AttackBuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = HandleInfoViewClick;
                }

                if (SettingsController.settingsWindow.FindView("AttackBuddyHelpersView", out Button helperView))
                {
                    helperView.Tag = SettingsController.settingsWindow;
                    helperView.Clicked = HandleHelpersViewClick;
                }

                if (_settings["Toggle"].AsBool() && !Toggle)
                {
                    IsLeader = true;
                    Leader = DynelManager.LocalPlayer.Identity;

                    if (DynelManager.LocalPlayer.Identity == Leader)
                        IPCChannel.Broadcast(new StartMessage());

                    Chat.WriteLine("AttackBuddy enabled.");
                    Start();
                }
                if (!_settings["Toggle"].AsBool() && Toggle)
                {
                    Stop();
                    Chat.WriteLine("AttackBuddy disabled.");
                    IPCChannel.Broadcast(new StopMessage());
                }

            }

            _stateMachine.Tick();

        }

        private void AttackBuddyCommand(string command, string[] param, ChatWindow chatWindow)
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
                        Chat.WriteLine("AttackBuddy enabled.");
                        Start();

                    }
                    else
                    {
                        Stop();
                        Chat.WriteLine("AttackBuddy disabled.");
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
