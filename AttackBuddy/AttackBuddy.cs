﻿using AOSharp.Common.GameData;
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
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace AttackBuddy
{
    public class AttackBuddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static int AttackRange;
        public static int ScanRange;

        public static bool Enable = false;
        public static bool _init = false;

        public static Identity Leader = Identity.None;

        public static double _stateTimeOut = Time.NormalTime;

        public static List<SimpleChar> _mob = new List<SimpleChar>();
        public static List<SimpleChar> _bossMob = new List<SimpleChar>();
        public static List<SimpleChar> _switchMob = new List<SimpleChar>();

        public static List<SimpleChar> _switchMobPrecision = new List<SimpleChar>();
        public static List<SimpleChar> _switchMobCharging = new List<SimpleChar>();
        public static List<SimpleChar> _switchMobShield = new List<SimpleChar>();

        private static double _refreshList;
        private static double _uiDelay;

        private static Window _infoWindow;
        private static Window _helperWindow;

        private static View _helperView;

        public static string PluginDir;

        public static Settings _settings;

        public static List<string> _helpers = new List<string>();

        public static string previousErrorMessage = string.Empty;

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("AttackBuddy");
                PluginDir = pluginDir;
                
                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\AttackBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");
                
                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.StartStop, OnStartStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.RangeInfo, OnRangeInfoMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.LeaderInfo, OnLeaderInfoMessage);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;
                Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRangeChangedEvent += AttackRange_Changed;
                Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRangeChangedEvent += ScanRange_Changed;

                Chat.RegisterCommand("buddy", BuddyCommand);

                SettingsController.RegisterSettingsWindow("AttackBuddy", pluginDir + "\\UI\\AttackBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("Enable", false);
                _settings["Enable"] = false;
                _settings.AddVariable("Taunt", false);

                Chat.WriteLine("AttackBuddy Loaded!");
                Chat.WriteLine("/attackbuddy for settings.");

                AttackRange = Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange;
                ScanRange = Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange;
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

        public Window[] _windows => new Window[] { _helperWindow };

        public static void IPCChannel_Changed(object s, int e)
        {
            IPCChannel.SetChannelId(Convert.ToByte(e));
            Config.Save();
        }
        public static void AttackRange_Changed(object s, int e)
        {
            Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange = e;
            AttackRange = e;
            Config.Save();
        }
        public static void ScanRange_Changed(object s, int e)
        {
            Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange = e;
            ScanRange = e;
            Config.Save();
        }

        private void Start()
        {
            Enable = true;

            Chat.WriteLine("AttackBuddy enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Enable = false;

            Chat.WriteLine("AttackBuddy disabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());

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

        private void OnRangeInfoMessage(int sender, IPCMessage msg)
        {
            if (msg is RangeInfoIPCMessage rangeInfoMessage)
            {
                Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange = rangeInfoMessage.AttackRange;
                Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange = rangeInfoMessage.ScanRange;
            }
        }

        private void OnLeaderInfoMessage(int sender, IPCMessage msg)
        {
            if (msg is LeaderInfoIPCMessage leaderInfoMessage)
            {
                if (leaderInfoMessage.IsRequest)
                {
                    if (Leader != Identity.None)
                    {
                        IPCChannel.Broadcast(new LeaderInfoIPCMessage() { LeaderIdentity = Leader, IsRequest = false });
                    }
                }
                else
                {
                    Leader = leaderInfoMessage.LeaderIdentity;
                }
            }
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
            switch (Playfield.ModelIdentity.Instance)
            {
                case 6123:
                    ScanningInstance6123();
                    break;

                case 6015:
                    ScanningInstance6015();
                    break;

                case 9070:
                    ScanningInstance9070();
                    break;

                case 9061:
                    ScanningInstance9061();
                    break;

                case 4389:
                case 4328:
                case 4391:
                    ScanningInstance4389();
                    break;

                default:
                    ScanningDefault();
                    break;
            }

            _refreshList = Time.NormalTime;
        }

        private void OnUpdate(object s, float deltaTime)
        {
            try
            {
                if (Game.IsZoning)
                {
                    Enable = false;
                    _settings["Enable"] = false;

                    return;
                }

                if (Leader == Identity.None)
                {
                    IPCChannel.Broadcast(new LeaderInfoIPCMessage() { IsRequest = true });
                }

                if (Time.NormalTime > _refreshList + 0.5f && Enable == true)
                {
                    Scanning();
                }
                    
                #region UI

                if (Time.NormalTime > _uiDelay + 1.0)
                {
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
                                && Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel != channelValue)
                            {
                                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel = channelValue;
                            }
                        }

                        //if (attackRangeInput != null && !string.IsNullOrEmpty(attackRangeInput.Text))
                        //{
                        //    if (int.TryParse(attackRangeInput.Text, out int attackRangeInputValue)
                        //        && Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange != attackRangeInputValue)
                        //    {
                        //        Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange = attackRangeInputValue;
                        //        IPCChannel.Broadcast(new AttackRangeMessage()
                        //        {
                        //            Range = attackRangeInputValue
                        //        });
                        //    }
                        //}
                        //if (scanRangeInput != null && !string.IsNullOrEmpty(scanRangeInput.Text))
                        //{
                        //    if (int.TryParse(scanRangeInput.Text, out int scanRangeInputValue)
                        //        && Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange != scanRangeInputValue)
                        //    {
                        //        Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange = scanRangeInputValue;
                        //        IPCChannel.Broadcast(new ScanRangeMessage()
                        //        {
                        //            Range = scanRangeInputValue
                        //        });
                        //    }
                        //}

                        bool attackRangeChanged = false;
                        bool scanRangeChanged = false;

                        if (int.TryParse(attackRangeInput.Text, out int attackRangeInputValue)
                            && Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange != attackRangeInputValue)
                        {
                            Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange = attackRangeInputValue;
                            attackRangeChanged = true;
                        }

                        if (int.TryParse(scanRangeInput.Text, out int scanRangeInputValue)
                            && Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange != scanRangeInputValue)
                        {
                            Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange = scanRangeInputValue;
                            scanRangeChanged = true;
                        }

                        if (attackRangeChanged || scanRangeChanged)
                        {
                            IPCChannel.Broadcast(new RangeInfoIPCMessage()
                            {
                                AttackRange = Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange,
                                ScanRange = Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange
                            });
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

                        _uiDelay = Time.NormalTime;
                    }

                    #endregion

                }
                
                _stateMachine.Tick();
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

        private void BuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Enable"].AsBool())
                    {
                        Leader = DynelManager.LocalPlayer.Identity;
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
                    return;
                }

                switch (param[0].ToLower())
                {
                    case "ignore":
                        if (param.Length > 1)
                        {
                            string name = string.Join(" ", param.Skip(1));

                            if (!Constants._ignores.Contains(name))
                            {
                                Constants._ignores.Add(name);
                                chatWindow.WriteLine($"Added \"{name}\" to ignored mob list");
                            }
                            else if (Constants._ignores.Contains(name))
                            {
                                Constants._ignores.Remove(name);
                                chatWindow.WriteLine($"Removed \"{name}\" from ignored mob list");
                            }
                        }
                        else
                        {
                            chatWindow.WriteLine("Please specify a name");
                        }
                        break;

                    default:
                        return;
                }
                Config.Save();
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != previousErrorMessage)
                {
                    chatWindow.WriteLine(errorMessage);
                    chatWindow.WriteLine("Stack Trace: " + ex.StackTrace);
                    previousErrorMessage = errorMessage;
                }
            }
        }

        // Separate voids for each instance
        private void ScanningInstance6123()
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

        private void ScanningInstance6015() //12m
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

            _switchMob = DynelManager.NPCs
              .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                  && !Constants._ignores.Contains(c.Name)
                  && c.Health > 0 && c.IsInLineOfSight && c.MaxHealth < 1000000
                  && Extensions.IsFightingAny(c))
              .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
              .OrderBy(c => c.HealthPercent)
              .OrderByDescending(c => c.Name == "Green Tower")
                .OrderByDescending(c => c.Name == "Blue Tower")
              .ToList();

        }

        private void ScanningInstance9070()//Subway Raid
        {
            _bossMob = DynelManager.NPCs
                          .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                              && !(c.Name == "Harbinger of Pestilence" || c.Name == "Curse Rot" || c.Name == "Scalding Flames"
                              || c.Name == "Searing Flames" || c.Name == "Vergil Doppelganger" || c.Name == "Oblivion" || c.Name == "Ire of Gilgamesh")
                              && c.Health > 0 && c.IsInLineOfSight
                              && !c.Buffs.Contains(253953) && !c.Buffs.Contains(205607)
                              && c.MaxHealth >= 1000000)
                          .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                          .ToList();

            _switchMob = DynelManager.NPCs
               .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                   && !(c.Name == "Harbinger of Pestilence" || c.Name == "Curse Rot" || c.Name == "Scalding Flames"
                       || c.Name == "Searing Flames" || c.Name == "Vergil Doppelganger" || c.Name == "Oblivion" || c.Name == "Ire of Gilgamesh")
                   && c.Health > 0 && c.IsInLineOfSight && c.MaxHealth < 1000000
                   && Extensions.IsFightingAny(c))
               .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
               .OrderBy(c => c.HealthPercent)
               .OrderByDescending(c => c.Name == "Stim Fiend")
               .OrderByDescending(c => c.Name == "Lost Thought")
               .ToList();

            _mob = DynelManager.Characters
                .Where(c => !c.IsPlayer && c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                    && !(c.Name == "Harbinger of Pestilence" || c.Name == "Curse Rot" || c.Name == "Scalding Flames"
                       || c.Name == "Searing Flames" || c.Name == "Vergil Doppelganger" || c.Name == "Oblivion" || c.Name == "Ire of Gilgamesh")
                    && c.Health > 0 && c.IsInLineOfSight
                    && c.MaxHealth < 1000000 && Extensions.IsFightingAny(c)
                    && !c.IsPet)
                .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                .OrderBy(c => c.HealthPercent)
                .OrderByDescending(c => c.Name == "Stim Fiend")
                .OrderByDescending(c => c.Name == "Lost Thought")
                .ToList();
        }

        private void ScanningInstance9061()//TOTW Raid
        {
            {
                _bossMob = DynelManager.NPCs
                   .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                       && !Constants._ignores.Contains(c.Name)
                       && c.Health > 0 && c.IsInLineOfSight
                       && !c.Buffs.Contains(253953) && !c.Buffs.Contains(205607)
                       && c.MaxHealth >= 1000000)
                   .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                   .OrderByDescending(c => c.Name == "Uklesh the Beguiling")
                   .OrderByDescending(c => c.Name == "Khalum the Weaver of Flesh")

                   .ToList();

                _switchMob = DynelManager.NPCs
                   .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                                   && !Constants._ignores.Contains(c.Name)
                                   && c.Health > 0 && c.IsInLineOfSight && c.MaxHealth < 1000000
                                   && Extensions.IsFightingAny(c)
                                   && (c.Name == "Devoted Fanatic"
                                   || c.Name == "Hallowed Acolyte"
                                   || c.Name == "Fanatic"
                                   || c.Name == "Turbulent Windcaller"
                                   || c.Name == "Ruinous Reverend"
                                   || c.Name == "Eternal Guardian"
                                   || c.Name == "Defensive Drone"
                                   || c.Name == "Confounding Bloated Carcass"))
                               .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                               .OrderBy(c => c.HealthPercent)
                               .OrderByDescending(c => c.Name == "Hallowed Acolyte")
                               .OrderByDescending(c => c.Name == "Confounding Bloated Carcass")
                               .OrderByDescending(c => c.Name == "Devoted Fanatic")
                               .ToList();

                _mob = DynelManager.Characters
                    .Where(c => !c.IsPlayer && c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                                    && !Constants._ignores.Contains(c.Name) && c.Health > 0
                                    && c.IsInLineOfSight && c.MaxHealth < 1000000 && Extensions.IsFightingAny(c)
                                    && !c.IsPet)
                                .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                                .OrderBy(c => c.HealthPercent)
                                .OrderByDescending(c => c.Name == "Faithful Cultist")
                                .OrderByDescending(c => c.Name == "Ruinous Reverend")
                                .OrderByDescending(c => c.Name == "Hallowed Acolyte")
                                .OrderByDescending(c => c.Name == "Turbulent Windcaller")
                                .OrderByDescending(c => c.Name == "Seraphic Exarch")
                                .OrderByDescending(c => c.Name == "Cultist Silencer")
                                .OrderByDescending(c => c.Name == "Devoted Fanatic")
                                .ToList();
            }
        }

        private void ScanningInstance4389()//IPande/Pande
        {
            _bossMob = DynelManager.NPCs
                       .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                           && !Constants._ignores.Contains(c.Name)
                           && c.Health > 0 && c.IsInLineOfSight
                           && !c.Buffs.Contains(253953) && !c.Buffs.Contains(205607)
                           && c.MaxHealth >= 1000000)
                       .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                       .ToList();

            _switchMob = DynelManager.NPCs
               .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                   && !Constants._ignores.Contains(c.Name)
                   && c.Health > 0 && c.IsInLineOfSight && c.MaxHealth < 1000000
                   && Extensions.IsFightingAny(c) && (c.Name == "Corrupted Xan-Len"))
               .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
               .OrderBy(c => c.HealthPercent)
               .ToList();

            _mob = DynelManager.Characters
                .Where(c => !c.IsPlayer && c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                    && !Constants._ignores.Contains(c.Name)
                    && c.Health > 0
                    && c.IsInLineOfSight && c.MaxHealth < 1000000
                    && Extensions.IsFightingAny(c)
                    && !c.IsPet)
                .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                .OrderBy(c => c.HealthPercent)
                .OrderByDescending(c => c.Name == "Corrupted Hiisi Berserker")
                .OrderByDescending(c => c.Name == "Corrupted Xan-Cur")
                .OrderByDescending(c => c.Name == "Corrupted Xan-Kuir")
                .OrderByDescending(c => c.Name == "Corrupted Xan-Len")
                .ToList();
        }

        private void ScanningDefault()
        {
            _bossMob = DynelManager.NPCs
                       .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                           && !Constants._ignores.Contains(c.Name)
                           && c.Health > 0 && c.IsInLineOfSight
                           && !c.Buffs.Contains(253953) && !c.Buffs.Contains(205607)
                           && c.MaxHealth >= 1000000)
                       .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
                       .OrderByDescending(c => c.Name == "Field Support  - Cha'Khaz")
                       .OrderByDescending(c => c.Name == "Ground Chief Aune")


                       .ToList();

            _switchMob = DynelManager.NPCs
               .Where(c => c.DistanceFrom(Extensions.GetLeader(Leader)) <= ScanRange
                   && !Constants._ignores.Contains(c.Name)
                   && c.Name != "Zix" && !c.Name.Contains("sapling")
                   && c.Health > 0 && c.IsInLineOfSight && c.MaxHealth < 1000000
                   && Extensions.IsFightingAny(c) && (c.Name == "Hand of the Colonel"
                  || c.Name == "Hacker'Uri"
                  || c.Name == "The Sacrifice"
                  || c.Name == "Drone Harvester - Jaax'Sinuh"
                  || c.Name == "Support Sentry - Ilari'Uri"
                  || c.Name == "Alien Coccoon"
                  || c.Name == "Alien Cocoon"
                  || c.Name == "Stasis Containment Field"))
               .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(Leader).Position))
               .OrderBy(c => c.HealthPercent)
               .OrderByDescending(c => c.Name == "Drone Harvester - Jaax'Sinuh")
               .OrderByDescending(c => c.Name == "Lost Thought")
               .OrderByDescending(c => c.Name == "Support Sentry - Ilari'Uri")
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
                .OrderByDescending(c => c.Name == "Drone Harvester - Jaax'Sinuh")
                .OrderByDescending(c => c.Name == "Support Sentry - Ilari'Uri")
                .OrderByDescending(c => c.Name == "Alien Cocoon")
                .OrderByDescending(c => c.Name == "Alien Coccoon" && c.MaxHealth < 40001)
                .OrderByDescending(c => c.Name == "Masked Operator")
                .OrderByDescending(c => c.Name == "Masked Technician")
                .OrderByDescending(c => c.Name == "Masked Engineer")
                .OrderByDescending(c => c.Name == "Masked Superior Commando")
                .OrderByDescending(c => c.Name == "The Sacrifice")
                .OrderByDescending(c => c.Name == "Hacker'Uri")
                .OrderByDescending(c => c.Name == "Hand of the Colonel")
                .OrderByDescending(c => c.Name == "Ground Chief Aune")
                .ToList();
        }

        public static int GetLineNumber(Exception ex)
        {
            var lineNumber = 0;

            var lineMatch = Regex.Match(ex.StackTrace ?? "", @":line (\d+)$", RegexOptions.Multiline);

            if (lineMatch.Success)
                lineNumber = int.Parse(lineMatch.Groups[1].Value);

            return lineNumber;
        }
    }
}
