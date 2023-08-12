using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static float Tick = 0;

        public static bool _initMerge = false;
        public static bool Toggle = false;
        public static bool _initSit = false;

        public static double _stateTimeOut;
        public static double _sitUpdateTimer;
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

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;
                Config.CharSettings[DynelManager.LocalPlayer.Name].LeaderChangedEvent += Leader_Changed;
                Config.CharSettings[DynelManager.LocalPlayer.Name].TickChangedEvent += Tick_Changed;

                Chat.RegisterCommand("buddy", AXPBuddyCommand);

                SettingsController.RegisterSettingsWindow("AXPBuddy", pluginDir + "\\UI\\AXPBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Game.OnUpdate += OnUpdate;
                Game.TeleportEnded += OnEndZoned;

                _settings.AddVariable("ModeSelection", (int)ModeSelection.Path);

                _settings.AddVariable("Toggle", false);
                _settings.AddVariable("Merge", false);

                _settings["Toggle"] = false;

                Chat.WriteLine("AXPBuddy Loaded!");
                Chat.WriteLine("/axpbuddy for settings.");

                LeaderName = Config.CharSettings[DynelManager.LocalPlayer.Name].Leader;
                Tick = Config.CharSettings[DynelManager.LocalPlayer.Name].Tick;
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

        public static void Leader_Changed(object s, string e)
        {
            Config.CharSettings[DynelManager.LocalPlayer.Name].Leader = e;
            LeaderName = e;
            Config.Save();
        }
        public static void Tick_Changed(object s, float e)
        {
            Config.CharSettings[DynelManager.LocalPlayer.Name].Tick = e;
            Tick = e;
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
            if (!_settings["Merge"].AsBool() && Leader == Identity.None)
                Leader = new Identity(IdentityType.SimpleChar, sender);

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
            if (Leader == Identity.None)
                Leader = new Identity(IdentityType.SimpleChar, sender);

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
                if (Game.IsZoning || Time.NormalTime < _lastZonedTime + 2f) { return; }

                if (Time.NormalTime > _mainUpdate + Tick)
                {
                    if (Time.NormalTime > _sitUpdateTimer + 1.5f)
                    {
                        ListenerSit();
                    }

                    _stateMachine.Tick();
                    _mainUpdate = Time.NormalTime;
                }

                #region UI Update

                if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
                {
                    SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);
                    SettingsController.settingsWindow.FindView("LeaderBox", out TextInputView leaderInput);
                    SettingsController.settingsWindow.FindView("TickBox", out TextInputView tickInput);

                    if (channelInput != null && !string.IsNullOrEmpty(channelInput.Text))
                        if (int.TryParse(channelInput.Text, out int channelValue)
                            && Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel != channelValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel = channelValue;
                    if (tickInput != null && !string.IsNullOrEmpty(tickInput.Text))
                        if (float.TryParse(tickInput.Text, out float tickValue)
                            && Config.CharSettings[DynelManager.LocalPlayer.Name].Tick != tickValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].Tick = tickValue;

                    if (leaderInput != null && !string.IsNullOrEmpty(leaderInput.Text))
                    {
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].Leader != leaderInput.Text)
                        {
                            Config.CharSettings[DynelManager.LocalPlayer.Name].Leader = leaderInput.Text;
                        }
                    }

                    if (SettingsController.settingsWindow.FindView("AXPBuddyInfoView", out Button infoView))
                    {
                        infoView.Tag = SettingsController.settingsWindow;
                        infoView.Clicked = HandleInfoViewClick;
                    }


                    if (_settings["Toggle"].AsBool() && !Toggle)
                    {
                        if (!_settings["Merge"].AsBool() && Leader == Identity.None)
                            Leader = DynelManager.LocalPlayer.Identity;

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
            }

            #endregion
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

        private void OnEndZoned(object s, EventArgs e)
        {
            _lastZonedTime = Time.NormalTime;
        }

        private void ListenerSit()
        {
            Spell spell = Spell.List.FirstOrDefault(x => x.IsReady);

            Item kit = Inventory.Items.Where(x => RelevantItems.Kits.Contains(x.Id)).FirstOrDefault();

            if (kit == null) { return; }

            if (_initSit == false && spell != null)
            {
                if (!DynelManager.LocalPlayer.Buffs.Contains(280488) && Extensions.CanUseSitKit())
                {
                    if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment) && _initSit == false
                        && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
                    {
                        if (DynelManager.LocalPlayer.NanoPercent < 66 || DynelManager.LocalPlayer.HealthPercent < 66)
                        {
                            Task.Factory.StartNew(
                               async () =>
                               {
                                   _initSit = true;
                                   await Task.Delay(400);
                                   NavMeshMovementController.SetMovement(MovementAction.SwitchToSit);
                                   await Task.Delay(1200);
                                   NavMeshMovementController.SetMovement(MovementAction.LeaveSit);
                                   await Task.Delay(400);
                                   _initSit = false;
                                   _sitUpdateTimer = Time.NormalTime;
                               });
                        }
                    }
                }
            }
        }

        private void AXPBuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Toggle"].AsBool() && !Toggle)
                    {
                        if (!_settings["Merge"].AsBool() && Leader == Identity.None)
                            Leader = DynelManager.LocalPlayer.Identity;

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
            public static readonly int[] Kits = {
                297274, 293296, 291084, 291083, 291082
            };
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