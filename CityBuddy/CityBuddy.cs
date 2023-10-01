using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using CityBuddy.IPCMessages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using SmokeLounge.AOtomation.Messaging.GameData;
using System.Diagnostics;

namespace CityBuddy
{
    public class CityBuddy : AOPluginEntry
    {

        public static StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static bool Enable = false;
        public static bool CityUnderAttack = false;
        public static bool CTWindowIsOpen = false;

        private Stopwatch _kitTimer = new Stopwatch();
        private Stopwatch _sitTimer = new Stopwatch();

        public static SimpleChar _leader;
        public static Identity Leader = Identity.None;
        public static Vector3 _leaderPos = Vector3.Zero;

        public static bool Ready = true;
        private Dictionary<Identity, bool> teamReadiness = new Dictionary<Identity, bool>();
        private bool? lastSentIsReadyState = null;

        public static Window _infoWindow;

        public static Settings _settings;

        public static string PluginDir;

        public const int MontroyalCity = 5002;
        public const int SerenityIslands = 6010;
        public const int PlayadelDesierto = 5001;
        public const int ICCHQ = 655;

        public static Vector3 _iCCReclaim = new Vector3(3232.2f, 35.2f, 923.2f);
        public static Vector3 _iCCTeleportUp = new Vector3(3160.4f, 36.3f, 866.9f);
        public static Vector3 _iCCCenterofCities = new Vector3(3138.6f, 52.1f, 826.0f);

        public static Vector3 _montroyalGaurdPos = new Vector3(587.1f, 160.7f, 649.4f);
        public static Vector3 _serenityGaurdPos = new Vector3(998.1f, 5.0f, 1178.5f); //998.1, 1178.5, 5.0
        public static Vector3 _playadelGaurdPos = new Vector3(212.6f, 32.7f, 338.7f); //212.6, 338.7, 32.7

        public static Door _exitDoor;

        public static List<string> _ignores = new List<string>
        {
            "Alien Coccoon"
        };

        public static string previousErrorMessage = string.Empty;

        List<int> seenValues = new List<int>();

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("CityBuddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\CityBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");
                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);
                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.StartStop, OnStartStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Enter, EnterMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.SelectedMemberUpdate, HandleSelectedMemberUpdate);
                IPCChannel.RegisterCallback((int)IPCOpcode.ClearSelectedMember, HandleClearSelectedMember);
                IPCChannel.RegisterCallback((int)IPCOpcode.LeaderInfo, OnLeaderInfoMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.WaitAndReady, OnWaitAndReadyMessage);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                SettingsController.RegisterSettingsWindow("CityBuddy", pluginDir + "\\UI\\CityBuddySettingWindow.xml", _settings);

                Chat.WriteLine("CityBuddy Loaded!");
                Chat.WriteLine("/citybuddy for settings.");

                _stateMachine = new StateMachine(new IdleState());

                _settings.AddVariable("Enable", false);
                _settings.AddVariable("Corpses", false);

                _settings["Enable"] = false;

                Chat.RegisterCommand("buddy", BuddyCommand);

                Game.OnUpdate += OnUpdate;
                Network.ChatMessageReceived += CityAttackStatus;

                Network.N3MessageReceived += CTWindowIsOpenBool;

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
            Enable = true;

            Chat.WriteLine("CityBuddy enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Enable = false;

            Chat.WriteLine("CityBuddy disabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());

            NavMeshMovementController.Halt();
        }

        private void OnStartStopMessage(int sender, IPCMessage msg)
        {
            if (msg is StartStopIPCMessage startStopMessage)
            {
                if (startStopMessage.IsStarting)
                {
                    Leader = new Identity(IdentityType.SimpleChar, sender);
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

        private void EnterMessage(int sender, IPCMessage msg)
        {
            if (!(_stateMachine.CurrentState is EnterState))
            {
                Chat.WriteLine("enter");
                _stateMachine.SetState(new EnterState());
            }
        }

        private void HandleSelectedMemberUpdate(int sender, IPCMessage msg)
        {
            SelectedMemberUpdateMessage message = msg as SelectedMemberUpdateMessage;
            if (message != null)
            {
                WaitForShipState.selectedMember = Team.Members.FirstOrDefault(m => m.Identity == message.SelectedMemberIdentity);
            }
        }
        private void HandleClearSelectedMember(int sender, IPCMessage msg)
        {
            WaitForShipState.selectedMember = null;
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

            if (Leader == Identity.None)
            {

                IPCChannel.Broadcast(new LeaderInfoIPCMessage() { IsRequest = true });

            }

            if (DynelManager.LocalPlayer.Identity != Leader)
            {
                var localPlayer = DynelManager.LocalPlayer;
                bool currentIsReadyState = true;

                // Check if Nano or Health is below 66% and not in combat
                if (!InCombat())
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

            SitAndUseKit();

            #region UI
            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);

                if (channelInput != null)
                {
                    if (int.TryParse(channelInput.Text, out int channelValue)
                        && Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel != channelValue)
                    {
                        Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel = channelValue;
                    }
                }

                if (SettingsController.settingsWindow.FindView("CityBuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = HandleInfoViewClick;
                }

                if (!_settings["Enable"].AsBool() && Enable)
                {
                    IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
                }
                if (_settings["Enable"].AsBool() && !Enable)
                {
                    IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = true });
                }
            }
            #endregion

            _stateMachine.Tick();
        }

        #region Kitting

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
            bool canSit = !localPlayer.Cooldowns.ContainsKey(Stat.Treatment) && localPlayer.MovementState != MovementState.Sit && !InCombat();

            bool shouldStand = localPlayer.NanoPercent > 66 || localPlayer.HealthPercent > 66;
            bool onCooldown = localPlayer.Cooldowns.ContainsKey(Stat.Treatment);

            if (shouldSit && canSit)
            {
                MovementController.Instance.SetMovement(MovementAction.SwitchToSit);
            }
            else if (shouldStand || onCooldown)
            {
                MovementController.Instance.SetMovement(MovementAction.LeaveSit);
            }
        }

        private bool CanUseSitKit()
        {
            if (!DynelManager.LocalPlayer.IsAlive || DynelManager.LocalPlayer.IsMoving || Game.IsZoning || InCombat())
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

        #endregion


        private void BuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    bool currentToggle = _settings["Enable"].AsBool();
                    if (!currentToggle)
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
                }
                Config.Save();
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

        public static bool CanProceed()
        {
            return DynelManager.LocalPlayer.HealthPercent > 65
                && DynelManager.LocalPlayer.NanoPercent > 65
                && DynelManager.LocalPlayer.GetStat(Stat.TemporarySkillReduction) <= 1
                && DynelManager.LocalPlayer.MovementState != MovementState.Sit
                && Spell.List.Any(c => c.IsReady)
                && !Spell.HasPendingCast;
        }

        private void CityAttackStatus(object s, ChatMessageBody msg)
        {
            if (msg.PacketType != ChatMessageType.GroupMessage) { return; }

            var groupMsg = (GroupMsgMessage)msg;

            if (groupMsg.MessageType != GroupMessageType.Org) { return; }

            if (groupMsg.Text.Contains("Wave counter started."))
            {
                Chat.WriteLine("City is under attack!");

                CityUnderAttack = true;
            }
        }
        private void Network_N3MessageReceived(object s, SmokeLounge.AOtomation.Messaging.Messages.N3Message n3Msg)
        {
            if (n3Msg.N3MessageType != N3MessageType.AOTransportSignal)
                return;

            AOTransportSignalMessage sigMsg = (AOTransportSignalMessage)n3Msg;

            if (sigMsg.Action == AOSignalAction.CityInfo)
            {
                var cityInfo = (CityInfo)(sigMsg.TransportSignalMessage);

                if (!seenValues.Contains(cityInfo.Unknown2))
                {
                    seenValues.Add(cityInfo.Unknown2);
                    Chat.WriteLine($"Unknown2: {cityInfo.Unknown2}"); // Example output: "Unknown2: 2"
                }

                if (!seenValues.Contains(cityInfo.Unknown3))
                {
                    seenValues.Add(cityInfo.Unknown3);
                    Chat.WriteLine($"Unknown3: {cityInfo.Unknown3}"); // Example output: "Unknown3: 3"
                }
            }
        }

        private void CTWindowIsOpenBool(object s, SmokeLounge.AOtomation.Messaging.Messages.N3Message n3Msg)
        {
            if (n3Msg.N3MessageType != N3MessageType.AOTransportSignal)
                return;

            AOTransportSignalMessage sigMsg = (AOTransportSignalMessage)n3Msg;

            if (sigMsg.Action == AOSignalAction.CityInfo)
            {
                var cityInfo = (CityInfo)(sigMsg.TransportSignalMessage);

                // If Unknown1 has any number, it means the City Controller is open
                if (cityInfo.Unknown1 != 0)
                {
                    CTWindowIsOpen = true;
                }
            }
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
