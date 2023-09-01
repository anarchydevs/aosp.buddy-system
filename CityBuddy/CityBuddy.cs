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
using System.Threading.Tasks;
using SmokeLounge.AOtomation.Messaging.GameData;

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

        public static SimpleChar _leader;
        public static Identity Leader = Identity.None;
        public static Vector3 _leaderPos = Vector3.Zero;

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

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Enter, EnterMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.SelectedMemberUpdate, HandleSelectedMemberUpdate);
                IPCChannel.RegisterCallback((int)IPCOpcode.ClearSelectedMember, HandleClearSelectedMember);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                SettingsController.RegisterSettingsWindow("CityBuddy", pluginDir + "\\UI\\CityBuddySettingWindow.xml", _settings);

                Chat.WriteLine("CityBuddy Loaded!");
                Chat.WriteLine("/citybuddy for settings.");

                _stateMachine = new StateMachine(new IdleState());

                _settings.AddVariable("Enable", false);
                _settings.AddVariable("Leader", false);
                _settings.AddVariable("Corpses", false);

                _settings["Enable"] = false;

                Chat.RegisterCommand("buddy", CityBuddyCommand);

                Game.OnUpdate += OnUpdate;
                Network.ChatMessageReceived += CityAttackStatus;

                Network.N3MessageReceived += Network_N3MessageReceived;
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

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            if (Leader == Identity.None && DynelManager.LocalPlayer.Identity.Instance != sender)
            {
                Leader = _settings["Leader"].AsBool() ? DynelManager.LocalPlayer.Identity : new Identity(IdentityType.SimpleChar, sender);
            }
            Enable = true;
            _settings["Enable"] = true;

            Chat.WriteLine("CityBuddy enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void OnStopMessage(int sender, IPCMessage msg)
        {
            _settings["Enable"] = false;
            Enable = false;

            Chat.WriteLine("CityBuddy disabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());

            NavMeshMovementController.Halt();
            MovementController.Instance.Halt();

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
                SimpleChar teamLeader = Team.Members.FirstOrDefault(member => member.IsLeader)?.Character;

                Leader = teamLeader?.Identity ?? Identity.None;
            }

            ListenerSit();

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
                    IPCChannel.Broadcast(new StopMessage());
                    Enable = false;
                }
                if (_settings["Enable"].AsBool() && !Enable)
                {
                    IPCChannel.Broadcast(new StartMessage());
                    Enable = true;
                }
            }
            #endregion

            _stateMachine.Tick();
        }

        private void ListenerSit()
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
                        NavMeshMovementController.SetMovement(MovementAction.SwitchToSit);
                    }
                }
            }
            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment))
            {
                if (DynelManager.LocalPlayer.NanoPercent < 66 || DynelManager.LocalPlayer.HealthPercent < 66)
                {
                    kit.Use();
                }
            }
            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit && DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment))
            {
                NavMeshMovementController.SetMovement(MovementAction.LeaveSit);

            }
        }

        private bool CanUseSitKit()
        {
            if (Inventory.Find(297274, out Item premSitKit))
                if (DynelManager.LocalPlayer.Health > 0 && !CityBuddy.InCombat()
                                    && !DynelManager.LocalPlayer.IsMoving && !Game.IsZoning) { return true; }

            if (DynelManager.LocalPlayer.Health > 0 && !CityBuddy.InCombat()
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

        private void CityBuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Enable"].AsBool())
                    {
                        _settings["Enable"] = true;
                        Enable = true;
                        IPCChannel.Broadcast(new StartMessage());
                        Chat.WriteLine("CityBuddy enabled.");

                    }
                    else
                    {
                        _settings["Enable"] = false;
                        Enable = false;
                        IPCChannel.Broadcast(new StopMessage());
                        Chat.WriteLine("CityBuddy disabled.");
                        NavMeshMovementController.Halt();
                        MovementController.Instance.Halt();
                    }
                }
                Config.Save();
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

        public static bool InCombat()
        {
            if (Team.IsInTeam)
            {
                return DynelManager.Characters
                    .Any(c => c.FightingTarget != null && Team.Members.Select(m => m.Name).Contains(c.FightingTarget?.Name));
            }

            return DynelManager.Characters
                    .Any(c => c.FightingTarget != null && c.FightingTarget?.Name == DynelManager.LocalPlayer.Name);
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

                if (!seenValues.Contains(cityInfo.Unknown1))
                {
                    seenValues.Add(cityInfo.Unknown1);
                    Chat.WriteLine($"Unknown1: {cityInfo.Unknown1}"); // Example output: "Unknown1: 1763334"
                }

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
