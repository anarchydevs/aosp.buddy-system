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

        public static bool Ready = true;

        private Dictionary<Identity, bool> teamReadiness = new Dictionary<Identity, bool>();

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
                Identity senderIdentity = waitAndReadyMessage.PlayerIdentity; // Get the Identity from the IPCMessage

                teamReadiness[senderIdentity] = waitAndReadyMessage.IsReady;

                //Chat.WriteLine($"IPC received. Sender: {senderIdentity}, IsReady: {waitAndReadyMessage.IsReady}"); // Debugging line added

                bool allReady = true;

                // Check team members against the readiness dictionary
                foreach (var teamMember in Team.Members)
                {
                    if (teamReadiness.ContainsKey(teamMember.Identity) && !teamReadiness[teamMember.Identity])
                    {
                        allReady = false;
                        break;
                    }
                }

                if (Leader == DynelManager.LocalPlayer.Identity)
                {
                    Ready = allReady;
                    
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

                Shared.Kits kitsInstance = new Shared.Kits();

                kitsInstance.SitAndUseKit();

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
                
                if (Playfield.ModelId == PlayfieldId.Sector13 && DynelManager.LocalPlayer.Identity != Leader)
                {
                    var localPlayer = DynelManager.LocalPlayer;
                    bool currentIsReadyState = true;

                    // Check if Nano or Health is below 66% and not in combat
                    if (!Shared.Kits.InCombat())
                    {
                        if (Spell.HasPendingCast || localPlayer.NanoPercent < 66 || localPlayer.HealthPercent < 66
                            || !Spell.List.Any(spell => spell.IsReady))
                        {
                            currentIsReadyState = false;
                        }
                    }

                    // Check if Nano and Health are above 66%
                    else if (!Spell.HasPendingCast && localPlayer.NanoPercent > 70 
                        && localPlayer.HealthPercent > 70 && Spell.List.Any(spell => spell.IsReady))
                    {
                        currentIsReadyState = true;
                    }

                    // Only send a message if the state has changed.
                    if (currentIsReadyState != lastSentIsReadyState)
                    {
                        Identity localPlayerIdentity = DynelManager.LocalPlayer.Identity;
                        //Chat.WriteLine($"Broadcasting IPC. Local player identity: {localPlayerIdentity}"); // Debugging line added

                        IPCChannel.Broadcast(new WaitAndReadyIPCMessage
                        {
                            IsReady = currentIsReadyState,
                            PlayerIdentity = localPlayerIdentity
                        });
                        lastSentIsReadyState = currentIsReadyState; // Update the last sent state
                    }
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