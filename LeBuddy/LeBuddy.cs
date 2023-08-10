using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using LeBuddy.IPCMessages;
using org.critterai.nav;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LeBuddy.NavGenState;

namespace LeBuddy
{

    public class LeBuddy : AOPluginEntry
    {
        private StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; set; }
        public static IPCChannel IPCChannel { get; set; }

        public static Config Config { get; private set; }

        public static bool Enable = false;

        public static SimpleChar _leader;
        public static Identity Leader = Identity.None;
        public static Vector3 _leaderPos = Vector3.Zero;

        public static Door _exitDoor;

        public static double _stateTimeOut;

        private static Window infoWindow;

        public static string PluginDir;

        public static Settings _settings;

        public static string previousErrorMessage = string.Empty;

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("LeBuddy");

                PluginDir = pluginDir;
                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\LeBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");

                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);

                IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Enter, EnterMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.SelectedMemberUpdate, HandleSelectedMemberUpdate);
                IPCChannel.RegisterCallback((int)IPCOpcode.ClearSelectedMember, HandleClearSelectedMember);


                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                SettingsController.RegisterSettingsWindow("LeBuddy", pluginDir + "\\UI\\LeBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Chat.RegisterCommand("buddy", LeCommand);

                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("Enable", false);

                _settings.AddVariable("Leader", false);

                _settings.AddVariable("DifficultySelection", (int)DifficultySelection.Hard);

                _settings["Enable"] = false;

                Chat.WriteLine("LeBuddy Loaded!");
                Chat.WriteLine("/le for settings.");
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

        private void InfoView(object s, ButtonBase button)
        {
            infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\LeBuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            infoWindow.Show(true);
        }
        private void Start()
        {
            Enable = true;

            if (_settings["Leader"].AsBool())
            {
                Leader = DynelManager.LocalPlayer.Identity;
            }

            Chat.WriteLine("LeBuddy enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Enable = false;

            Chat.WriteLine("LeBuddy disabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());

            NavMeshMovementController.Halt();
        }

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            if (Leader == Identity.None && DynelManager.LocalPlayer.Identity.Instance != sender)
            {
                Leader = _settings["Leader"].AsBool() ? DynelManager.LocalPlayer.Identity : new Identity(IdentityType.SimpleChar, sender);
            }

            _settings["Enable"] = true;
            Start();
        }


        private void OnStopMessage(int sender, IPCMessage msg)
        {
            _settings["Enable"] = false;
            Stop();
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
                // Find the team member with the received identity and set as selectedMember
                IdleState.selectedMember = Team.Members.FirstOrDefault(m => m.Identity == message.SelectedMemberIdentity);
            }
        }
        private void HandleClearSelectedMember(int sender, IPCMessage msg)
        {
            IdleState.selectedMember = null;
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning) { return; }

            _stateMachine.Tick();

                ListenerSit();

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

                if (SettingsController.settingsWindow.FindView("LeBuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = InfoView;
                }

                if (!_settings["Enable"].AsBool() && Enable)
                {
                    IPCChannel.Broadcast(new StopMessage());
                    Stop();
                }
                if (_settings["Enable"].AsBool() && !Enable)
                {
                    IPCChannel.Broadcast(new StartMessage());
                    Start();
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
            if (DynelManager.LocalPlayer.MovementState == MovementState.Sit && DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment))
            {
                NavMeshMovementController.SetMovement(MovementAction.LeaveSit);
                
            }
        }

        public void LeCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Enable"].AsBool())
                    {
                        _settings["Enable"] = true;
                        IPCChannel.Broadcast(new StartMessage());
                        Start();
                        Chat.WriteLine("Enabled.");
                    }
                    else
                    {
                        _settings["Enable"] = false;
                        IPCChannel.Broadcast(new StopMessage());
                        Stop();
                        Chat.WriteLine("Disabled.");
                    }
                }
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        public enum DifficultySelection
        {
            Easy, Medium, Hard
        }

        public static class RelevantNanos
        {
        
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
