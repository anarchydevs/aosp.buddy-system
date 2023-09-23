using System;
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

        private bool? lastSentIsReadyState = null;

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

                IPCChannel.RegisterCallback((int)IPCOpcode.StartStop, OnStartStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.LeaderInfo, OnLeaderInfoMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.WaitAndReady, OnWaitAndReadyMessage);


                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                Chat.RegisterCommand("buddy", BuddyCommand);

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

        private void OnStartStopMessage(int sender, IPCMessage msg)
        {
            if (msg is StartStopIPCMessage startStopMessage)
            {
                if (startStopMessage.IsStarting)
                {
                    // Only set the Leader if "Merge" is not checked.
                    if (!_settings["Merge"].AsBool())
                    {
                        Leader = new Identity(IdentityType.SimpleChar, sender);
                    }

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
        private void OnWaitAndReadyMessage(int sender, IPCMessage msg)
        {
            if (msg is WaitAndReadyIPCMessage waitAndReadyMessage)
            {
                var localPlayer = DynelManager.LocalPlayer;
                if (Leader == localPlayer.Identity)
                {
                    if (waitAndReadyMessage.IsReady)
                    {
                        _settings["Toggle"] = true;
                        Start();
                    }
                    else
                    {
                        _settings["Toggle"] = false;
                        Stop();
                    }
                }
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

                if (Leader == Identity.None)
                {
                    if (_settings["Merge"].AsBool())
                    {
                        SimpleChar teamLeader = Team.Members.FirstOrDefault(member => member.IsLeader)?.Character;

                        Leader = teamLeader?.Identity ?? Identity.None;
                    }
                    else
                    {
                        IPCChannel.Broadcast(new LeaderInfoIPCMessage() { IsRequest = true });
                    }
                }

                var localPlayer = DynelManager.LocalPlayer;
                bool currentIsReadyState = true;

                // Check if Nano or Health is below 66% and not in combat
                if (!InCombat())
                {
                    if (Spell.HasPendingCast || localPlayer.NanoPercent < 66 || localPlayer.HealthPercent < 66)
                    {
                        currentIsReadyState = false;
                    }
                }
                   
                // Check if Nano and Health are above 66%
                else if (localPlayer.NanoPercent > 70 && localPlayer.HealthPercent > 70)
                {
                    currentIsReadyState = true;
                }

                // Only send a message if the state has changed.
                if (currentIsReadyState != lastSentIsReadyState)
                {
                    IPCChannel.Broadcast(new WaitAndReadyIPCMessage { IsReady = currentIsReadyState });
                    lastSentIsReadyState = currentIsReadyState; // Update the last sent state
                }

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

                    if (!_settings["Toggle"].AsBool() && Toggle)
                    {
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
                        Stop();
                    }
                    if (_settings["Toggle"].AsBool() && !Toggle)
                    {
                        if (!_settings["Merge"].AsBool())
                            Leader = DynelManager.LocalPlayer.Identity;

                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = true });
                        Start();
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
            if (InCombat())
                return;

            Spell spell = Spell.List.FirstOrDefault(x => x.IsReady);
            Item kit = Inventory.Items.FirstOrDefault(x => RelevantItems.Kits.Contains(x.Id));
            var localPlayer = DynelManager.LocalPlayer;

            if (kit == null || spell == null)
                return;

            if (!localPlayer.Buffs.Contains(280488) && CanUseSitKit())
            {
                HandleSitState();
            }

            if (localPlayer.MovementState == MovementState.Sit && (!_kitTimer.IsRunning || _kitTimer.ElapsedMilliseconds >= 2000))
            {
                if (localPlayer.NanoPercent < 90 || localPlayer.HealthPercent < 90)
                {
                    kit.Use(localPlayer, true);
                    _kitTimer.Restart();
                }
            }
        }

        private void HandleSitState()
        {
            var localPlayer = DynelManager.LocalPlayer;

            bool shouldSit = localPlayer.NanoPercent < 66 || localPlayer.HealthPercent < 66;
            bool canSit = !localPlayer.Cooldowns.ContainsKey(Stat.Treatment) && localPlayer.MovementState != MovementState.Sit;

            bool shouldStand = localPlayer.NanoPercent > 66 || localPlayer.HealthPercent > 66;
            bool onCooldown = localPlayer.Cooldowns.ContainsKey(Stat.Treatment);

            if (shouldSit && canSit)
            {
                MovementController.Instance.SetMovement(MovementAction.SwitchToSit);
            }
            else if (shouldStand && onCooldown)
            {
                MovementController.Instance.SetMovement(MovementAction.LeaveSit);
            }
        }

        private bool CanUseSitKit()
        {
            if (!DynelManager.LocalPlayer.IsAlive || DynelManager.LocalPlayer.IsMoving || Game.IsZoning)
            {
                return false;
            }

            List<Item> sitKits = Inventory.FindAll("Health and Nano Recharger").Where(c => c.Id != 297274).ToList();
            if (sitKits.Any())
            {
                return sitKits.OrderBy(x => x.QualityLevel).Any(sitKit => MeetsSkillRequirement(sitKit));
            }

            return Inventory.Find(297274, out Item premSitKit);
        }

        private bool MeetsSkillRequirement(Item sitKit)
        {
            var localPlayer = DynelManager.LocalPlayer;
            int skillReq = sitKit.QualityLevel > 200 ? (sitKit.QualityLevel % 200 * 3) + 1501 : (int)(sitKit.QualityLevel * 7.5f);

            return localPlayer.GetStat(Stat.FirstAid) >= skillReq || localPlayer.GetStat(Stat.Treatment) >= skillReq;
        }

        public static bool InCombat()
        {
            var localPlayer = DynelManager.LocalPlayer;

            if (Team.IsInTeam)
            {
                return DynelManager.Characters
                    .Any(c => c.FightingTarget != null
                        && Team.Members.Select(m => m.Name).Contains(c.FightingTarget.Name));
            }

            return DynelManager.Characters
                    .Any(c => c.FightingTarget != null
                        && c.FightingTarget.Name == localPlayer.Name)
                    || localPlayer.GetStat(Stat.NumFightingOpponents) > 0
                    || Team.IsInCombat()
                    || localPlayer.FightingTarget != null;
        }
        private void OnTeamRequest(object sender, TeamRequestEventArgs e)
        {
            // Set the leader to the sender of the team request
            Leader = e.Requester;
        }

        private void BuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    bool currentToggle = _settings["Toggle"].AsBool();
                    if (!currentToggle)
                    {
                        Leader = DynelManager.LocalPlayer.Identity;
                        _settings["Toggle"] = true;
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = true });
                        Start();
                    }
                    else
                    {
                        _settings["Toggle"] = false;
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
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