using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using DefendBuddy.IPCMessages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DefendBuddy
{
    public class DefendBuddy : AOPluginEntry
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

        private static double _refreshList;

        public static Window _infoWindow;

        public static string PluginDir;

        public static Settings _settings;


        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("DefendBuddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\DefendBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");
                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.SetPos, OnSetPosMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.SetResetPos, OnSetResetPosMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.AttackRange, OnAttackRangeMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.ScanRange, OnScanRangeMessage);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;
                Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRangeChangedEvent += AttackRange_Changed;
                Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRangeChangedEvent += ScanRange_Changed;

                Chat.RegisterCommand("buddy", BuddyCommand);

                SettingsController.RegisterSettingsWindow("DefendBuddy", pluginDir + "\\UI\\DefendBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("ModeSelection", (int)ModeSelection.Taunt);

                _settings.AddVariable("Toggle", false);
                _settings.AddVariable("Looting", false);

                _settings["Toggle"] = false;

                Chat.WriteLine("DefendBuddy Loaded!");
                Chat.WriteLine("/defendbuddy for settings.");

                AttackRange = Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange;
                ScanRange = Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange;
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
            //TODO: Change in config so it saves when needed to - interface name -> INotifyPropertyChanged
            Config.Save();
        }
        public static void AttackRange_Changed(object s, int e)
        {
            Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange = e;
            AttackRange = e;
            //TODO: Change in config so it saves when needed to - interface name -> INotifyPropertyChanged
            Config.Save();
        }
        public static void ScanRange_Changed(object s, int e)
        {
            Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange = e;
            ScanRange = e;
            //TODO: Change in config so it saves when needed to - interface name -> INotifyPropertyChanged
            Config.Save();
        }

        private void OnSetPosMessage(int sender, IPCMessage msg)
        {
            SetPosMessage setposMsg = (SetPosMessage)msg;

            Chat.WriteLine("Position set.");

            Constants._posToDefend = DynelManager.LocalPlayer.Position;
        }

        private void OnSetResetPosMessage(int sender, IPCMessage msg)
        {
            SetResetPosMessage setresetposMsg = (SetResetPosMessage)msg;

            Chat.WriteLine("Position reset.");

            Constants._posToDefend = Vector3.Zero;
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

            Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange = rangeMsg.Range;
        }

        private void OnScanRangeMessage(int sender, IPCMessage msg)
        {
            ScanRangeMessage rangeMsg = (ScanRangeMessage)msg;

            Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange = rangeMsg.Range;
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
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\DefendBuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }

        protected void RegisterSettingsWindow(string settingsName, string xmlName)
        {
            SettingsController.RegisterSettingsWindow(settingsName, PluginDir + "\\UI\\" + xmlName, _settings);
        }

        private void Scanning()
        {
            _bossMob = DynelManager.NPCs
                .Where(c => c.Position.DistanceFrom(Constants._posToDefend) <= ScanRange
                    && !Constants._ignores.Contains(c.Name)
                    && c.IsAlive && c.IsInLineOfSight
                    && !c.Buffs.Contains(253953) && !c.Buffs.Contains(205607)
                    && !c.Buffs.Contains(302745)
                    && !c.Buffs.Contains(NanoLine.ShovelBuffs)
                    && c.MaxHealth >= 1000000)
                .OrderBy(c => c.Position.DistanceFrom(Constants._posToDefend))
                .OrderByDescending(c => c.Name == "Uklesh the Beguiling")
                .OrderByDescending(c => c.Name == "Khalum the Weaver of Flesh")
                .ToList();

            _switchMob = DynelManager.NPCs
               .Where(c => c.DistanceFrom(Extensions.GetLeader(DefendBuddy.Leader)) <= ScanRange
                   && !Constants._ignores.Contains(c.Name)
                   && c.Name != "Zix" && !c.Name.Contains("sapling")
                   && c.IsAlive && c.IsInLineOfSight && c.MaxHealth < 1000000
                   && Extensions.IsFightingAny(c) && (c.Name == "Devoted Fanatic" || c.Name == "Hallowed Acolyte" || c.Name == "Hand of the Colonel"
                || c.Name == "Hacker'Uri" || c.Name == "The Sacrifice" || c.Name == "Corrupted Xan-Len"
                 || c.Name == "Blue Tower" || c.Name == "Green Tower" || c.Name == "Drone Harvester - Jaax'Sinuh"
                  || c.Name == "Support Sentry - Ilari'Uri" || c.Name == "Fanatic" || c.Name == "Alien Coccoon" || c.Name == "Alien Cocoon" || c.Name == "Stasis Containment Field"))
               .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(DefendBuddy.Leader).Position))
               .OrderBy(c => c.HealthPercent)
               .OrderByDescending(c => c.Name == "Drone Harvester - Jaax'Sinuh")
               .OrderByDescending(c => c.Name == "Lost Thought")
               .OrderByDescending(c => c.Name == "Support Sentry - Ilari'Uri")
               .OrderByDescending(c => c.Name == "Alien Cocoon" || c.Name == "Alien Coccoon")
               .ToList();

            _mob = DynelManager.Characters
                .Where(c => !c.IsPlayer && c.DistanceFrom(Extensions.GetLeader(DefendBuddy.Leader)) <= ScanRange
                    && !Constants._ignores.Contains(c.Name)
                    && c.Name != "Zix" && !c.Name.Contains("sapling") && c.IsAlive
                    && c.IsInLineOfSight && c.MaxHealth < 1000000 && Extensions.IsFightingAny(c)
                    && (!c.IsPet || c.Name == "Drop Trooper - Ilari'Ra"))
                .OrderBy(c => c.Position.DistanceFrom(Extensions.GetLeader(DefendBuddy.Leader).Position))
                .OrderBy(c => c.HealthPercent)
                .OrderByDescending(c => c.Name == "Corrupted Hiisi Berserker")
                .OrderByDescending(c => c.Name == "Corrupted Xan-Cur")
                .OrderByDescending(c => c.Name == "Corrupted Xan-Kuir")
                .OrderByDescending(c => c.Name == "Drone Harvester - Jaax'Sinuh")
                .OrderByDescending(c => c.Name == "Support Sentry - Ilari'Uri")
                .OrderByDescending(c => c.Name == "Alien Cocoon")
                .OrderByDescending(c => c.Name == "Alien Coccoon")
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
                .OrderByDescending(c => c.Name == "Hallowed Acolyte")
                .OrderByDescending(c => c.Name == "Devoted Fanatic")
                .ToList();

            _refreshList = Time.NormalTime;
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning)
                return;

            if (Time.NormalTime - _refreshList >= 0.5
                && Toggle == true && Constants._posToDefend != Vector3.Zero)
                Scanning();

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
                if (attackRangeInput != null && !string.IsNullOrEmpty(attackRangeInput.Text))
                {
                    if (int.TryParse(attackRangeInput.Text, out int attackRangeInputValue)
                        && Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange != attackRangeInputValue)
                    {
                        Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange = attackRangeInputValue;
                        IPCChannel.Broadcast(new AttackRangeMessage()
                        {
                            Range = attackRangeInputValue
                        });
                    }
                }
                if (scanRangeInput != null && !string.IsNullOrEmpty(scanRangeInput.Text))
                {
                    if (int.TryParse(scanRangeInput.Text, out int scanRangeInputValue)
                        && Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange != scanRangeInputValue)
                    {
                        Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange = scanRangeInputValue;
                        IPCChannel.Broadcast(new ScanRangeMessage()
                        {
                            Range = scanRangeInputValue
                        });
                    }
                }

                if (SettingsController.settingsWindow.FindView("DefendBuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = HandleInfoViewClick;
                }

                if (_settings["Toggle"].AsBool()
                    && DynelManager.LocalPlayer.Identity == Leader
                    && Constants._posToDefend == Vector3.Zero)
                {
                    Constants._posToDefend = DynelManager.LocalPlayer.Position;
                    IPCChannel.Broadcast(new SetPosMessage());

                    Chat.WriteLine("Position set.");
                    return;
                }

                if (!_settings["Toggle"].AsBool()
                    && DynelManager.LocalPlayer.Identity == Leader
                    && Constants._posToDefend != Vector3.Zero)
                {
                    Constants._posToDefend = Vector3.Zero;
                    IPCChannel.Broadcast(new SetResetPosMessage());

                    Chat.WriteLine("Position reset.");
                    return;
                }

                if (_settings["Toggle"].AsBool() && !Toggle)
                {
                    IsLeader = true;
                    Leader = DynelManager.LocalPlayer.Identity;

                    if (DynelManager.LocalPlayer.Identity == Leader)
                        IPCChannel.Broadcast(new StartMessage());

                    Chat.WriteLine("DefendBuddy enabled.");
                    Start();
                }
                if (!_settings["Toggle"].AsBool() && Toggle)
                {
                    Stop();
                    Chat.WriteLine("DefendBuddy disabled.");
                    IPCChannel.Broadcast(new StopMessage());
                }
            }

            _stateMachine.Tick();

        }

        private void BuddyCommand(string command, string[] param, ChatWindow chatWindow)
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
                        Chat.WriteLine("DefendBuddy enabled.");
                        Start();

                    }
                    else
                    {
                        Stop();
                        Chat.WriteLine("DefendBuddy disabled.");
                        IPCChannel.Broadcast(new StopMessage());
                    }
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
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        public enum ModeSelection
        {
            Taunt, Path
        }
    }
}
