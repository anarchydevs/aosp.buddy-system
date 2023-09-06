﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Core.Movement;
using AOSharp.Core.IPC;
using AOSharp.Pathfinding;
using AXPBuddy.IPCMessages;
using AOSharp.Common.GameData;
using AOSharp.Core.Inventory;
using AOSharp.Common.GameData.UI;
using System.Security.Cryptography;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace AXPBuddy
{
    public class AXPBuddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static Identity Leader = Identity.None;
        public static string LeaderName = string.Empty;
        public static SimpleChar _leader;
        public static Vector3 _leaderPos = Vector3.Zero;
        public static Vector3 _ourPos = Vector3.Zero;

        private Stopwatch _kitTimer = new Stopwatch();

        public static float Tick = 0;

        public static bool _initMerge = false;

        public static bool Toggle = false;

        public static double _stateTimeOut;

        public static double _mainUpdate;

        public static double _lastZonedTime = Time.NormalTime;

        public static Vector3 _pos = Vector3.Zero;

        public static Window _infoWindow;

        public static Settings _settings;

        public static string PluginDir;

        public static string previousErrorMessage = string.Empty;

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("AXPBuddy");

                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\AXPBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");
               
                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);

                MovementController.Set(NavMeshMovementController);

                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Leader, HandleBroadcastLeader);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                Chat.RegisterCommand("buddy", AXPBuddyCommand);

                SettingsController.RegisterSettingsWindow("AXPBuddy", pluginDir + "\\UI\\AXPBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Game.OnUpdate += OnUpdate;
                Team.TeamRequest += OnTeamRequest;

                _settings.AddVariable("ModeSelection", (int)ModeSelection.Path);

                _settings.AddVariable("Toggle", false);
                _settings.AddVariable("Merge", false);

                _settings["Toggle"] = false;

                Chat.WriteLine("AXPBuddy Loaded!");
                Chat.WriteLine("/axpbuddy for settings.");

                SimpleChar teamLeader = Team.Members.FirstOrDefault(member => member.IsLeader)?.Character;

                if (teamLeader != null && teamLeader.Identity == DynelManager.LocalPlayer.Identity)
                {
                    if (Leader != teamLeader.Identity)
                    {
                        Leader = teamLeader.Identity;
                        LeaderMessage leaderMessage = new LeaderMessage { Leader = Leader };
                        IPCChannel.Broadcast(leaderMessage);
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

        public override void Teardown()
        {
            SettingsController.CleanUp();
        }

        public static void IPCChannel_Changed(object s, int e)
        {
            IPCChannel.SetChannelId(Convert.ToByte(e));
            Config.Save();
        }

        private void Start()
        {
            Toggle = true;

            Chat.WriteLine("Buddy enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Toggle = false;

            Chat.WriteLine("Buddy disabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());

            if (DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.StopAttack();
            if (MovementController.Instance.IsNavigating)
                MovementController.Instance.Halt();
        }

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            if (DynelManager.LocalPlayer.Identity == Leader || _settings["Merge"].AsBool())
                return;

            Chat.WriteLine("Buddy enabled.");
            _settings["Toggle"] = true;

            Toggle = true;

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void OnStopMessage(int sender, IPCMessage msg)
        {
            if (DynelManager.LocalPlayer.Identity == Leader || _settings["Merge"].AsBool())
                return;

            Toggle = false;

            _settings["Toggle"] = false;
            Chat.WriteLine("Buddy disabled.");

            if (DynelManager.LocalPlayer.IsAttacking)
                DynelManager.LocalPlayer.StopAttack();
            if (MovementController.Instance.IsNavigating)
                MovementController.Instance.Halt();

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void HandleBroadcastLeader(int sender, IPCMessage msg)
        {
            LeaderMessage message = msg as LeaderMessage;
            if (message != null)
            {
                Leader = message.Leader;
            }
        }

        private void HandleInfoViewClick(object s, ButtonBase button)
        {
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\AXPBuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }

        private void OnUpdate(object s, float deltaTime)
        {
            try
            {
                if (Game.IsZoning) { return; }

                SitAndUseKit();

                #region UI Update

                if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
                {
                    SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);

                    if (channelInput != null && !string.IsNullOrEmpty(channelInput.Text))
                        if (int.TryParse(channelInput.Text, out int channelValue)
                            && Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel != channelValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel = channelValue;

                    if (SettingsController.settingsWindow.FindView("AXPBuddyInfoView", out Button infoView))
                    {
                        infoView.Tag = SettingsController.settingsWindow;
                        infoView.Clicked = HandleInfoViewClick;
                    }

                    if (_settings["Toggle"].AsBool() && !Toggle)
                    {
                        if (DynelManager.LocalPlayer.Identity == Leader)
                            IPCChannel.Broadcast(new StartMessage());

                        Start();
                    }

                    if (!_settings["Toggle"].AsBool() && Toggle)
                    {
                        Stop();

                        if (DynelManager.LocalPlayer.Identity == Leader)
                            IPCChannel.Broadcast(new StopMessage());
                    }
                }

                #endregion

                _stateMachine.Tick();
            }

            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + AXPBuddy.GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    previousErrorMessage = errorMessage;
                }
            }
        }

        private void SitAndUseKit()
        {
            Spell spell = Spell.List.FirstOrDefault(x => x.IsReady);

            Item kit = Inventory.Items.FirstOrDefault(x => RelevantItems.Kits.Contains(x.Id));

            if (kit == null || spell == null)
            {
                return;
            }

            if (!DynelManager.LocalPlayer.Buffs.Contains(280488) && CanUseSitKit())
            {
                if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment) &&
                    DynelManager.LocalPlayer.MovementState != MovementState.Sit)
                {
                    if (DynelManager.LocalPlayer.NanoPercent < 66 || DynelManager.LocalPlayer.HealthPercent < 66)
                    {
                        // Switch to sitting
                        MovementController.Instance.SetMovement(MovementAction.SwitchToSit);
                    }
                }
            }

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit
           && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment))
            {
                // If the Stopwatch hasn't been started or 2 seconds have elapsed
                if (!_kitTimer.IsRunning || _kitTimer.ElapsedMilliseconds >= 2000)
                {
                    if (DynelManager.LocalPlayer.NanoPercent < 90 || DynelManager.LocalPlayer.HealthPercent < 90)
                    {
                        // Use the kit
                        kit.Use(DynelManager.LocalPlayer, true);

                        // Reset and start the Stopwatch
                        _kitTimer.Restart();
                    }
                }
            }

            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit
            && DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment))
            {
                if (DynelManager.LocalPlayer.NanoPercent > 66 || DynelManager.LocalPlayer.HealthPercent > 66)
                {
                    // Leave sitting if treatment cooldown is active
                    MovementController.Instance.SetMovement(MovementAction.LeaveSit);
                }
            }
        }

        private bool CanUseSitKit()
        {
            if (Inventory.Find(297274, out Item premSitKit))
                if (DynelManager.LocalPlayer.Health > 0 && !Extensions.InCombat()
                                    && !DynelManager.LocalPlayer.IsMoving && !Game.IsZoning) { return true; }

            if (DynelManager.LocalPlayer.Health > 0 && !Extensions.InCombat()
                    && !DynelManager.LocalPlayer.IsMoving && !Game.IsZoning)
            {
                List<Item> sitKits = Inventory.FindAll("Health and Nano Recharger").Where(c => c.Id != 297274).ToList();

                if (!sitKits.Any()) { return false; }

                foreach (Item sitKit in sitKits.OrderBy(x => x.QualityLevel))
                {
                    int skillReq = (sitKit.QualityLevel > 200 ? (sitKit.QualityLevel % 200 * 3) + 1501 : (int)(sitKit.QualityLevel * 7.5f));

                    if (DynelManager.LocalPlayer.GetStat(Stat.FirstAid) >= skillReq || DynelManager.LocalPlayer.GetStat(Stat.Treatment) >= skillReq)
                        return true;
                }
            }

            return false;
        }

        private void OnTeamRequest(object sender, TeamRequestEventArgs e)
        {
            // Set the leader to the sender of the team request
            Leader = e.Requester;
        }

        private void AXPBuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Toggle"].AsBool() && !Toggle)
                    {
                        if (DynelManager.LocalPlayer.Identity == Leader)
                            IPCChannel.Broadcast(new StartMessage());

                        _settings["Toggle"] = true;
                        Start();

                    }
                    else
                    {
                        Stop();
                        _settings["Toggle"] = false;
                        IPCChannel.Broadcast(new StopMessage());
                    }
                }
                Config.Save();
            }
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + AXPBuddy.GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != previousErrorMessage)
                {
                    Chat.WriteLine(errorMessage);
                    Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                    previousErrorMessage = errorMessage;
                }
            }
        }

        public enum ModeSelection
        {
            Pull, Path, Leech
        }

        public static class RelevantItems
        {
            public static readonly int[] Kits = { 297274, 293296, 291084, 291083, 291082 };
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