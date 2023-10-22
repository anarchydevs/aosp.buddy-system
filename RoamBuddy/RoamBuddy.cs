using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using RoamBuddy.IPCMessages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace RoamBuddy
{
    public class RoamBuddy : AOPluginEntry
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

        public static List<Vector3> _waypoints = new List<Vector3>();

        public static List<SimpleChar> _mob = new List<SimpleChar>();
        public static List<SimpleChar> _bossMob = new List<SimpleChar>();
        public static List<SimpleChar> _switchMob = new List<SimpleChar>();

        private static double _refreshList;

        private static Window _infoWindow;
        private static Window _waypointWindow;

        private static View _waypointView;

        public static string PluginDir;

        public static Settings _settings;

        private static string PluginBasePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CommonParameters.BasePath, CommonParameters.AppPath, "RoamBuddy");
        private static string PlayerPreferencesPath = System.IO.Path.Combine(PluginBasePath, DynelManager.LocalPlayer.Name, "Config.json");
        private static string WaypointsExportPath = System.IO.Path.Combine(PluginBasePath, "Exports");

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("RoamBuddy");
                PluginDir = pluginDir;

                Config = Config.Load(PlayerPreferencesPath);
                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.StartStop, OnStartStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.RangeInfo, OnRangeInfoMessage);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;
                Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRangeChangedEvent += AttackRange_Changed;
                Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRangeChangedEvent += ScanRange_Changed;

                Chat.RegisterCommand("buddy", BuddyCommand);

                SettingsController.RegisterSettingsWindow("RoamBuddy", pluginDir + "\\UI\\RoamBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("ModeSelection", (int)ModeSelection.Taunt);

                _settings.AddVariable("Toggle", false);
                _settings.AddVariable("Looting", false);

                Chat.WriteLine("RoamBuddy Loaded!");
                Chat.WriteLine("/roambuddy for settings.");

                AttackRange = Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange;
                ScanRange = Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange;
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.ToString());
            }
        }

        public override void Teardown()
        {
            SettingsController.CleanUp();
        }

        public Window[] _windows => new Window[] { _waypointWindow };

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
            Toggle = true;

            Chat.WriteLine("RoamBuddy enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Toggle = false;

            Chat.WriteLine("RoamBuddy disabled.");

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

        private void OnRangeInfoMessage(int sender, IPCMessage msg)
        {
            if (msg is RangeInfoIPCMessage rangeInfoMessage)
            {
                Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange = rangeInfoMessage.AttackRange;
                Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange = rangeInfoMessage.ScanRange;
            }
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

        private void HandleWaypointsViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Do we need this?
                if (window.Views.Contains(_waypointView)) { return; }

                _waypointView = View.CreateFromXml(PluginDir + "\\UI\\RoamBuddyWaypointsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Waypoints", XmlViewName = "RoamBuddyWaypointsView" }, _waypointView);
            }
            else if (_waypointWindow == null || (_waypointWindow != null && !_waypointWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_waypointWindow, PluginDir, new WindowOptions() { Name = "Waypoints", XmlViewName = "RoamBuddyWaypointsView" }, _waypointView, out var container);
                _waypointWindow = container;
            }
        }

        private void HandleAddWaypointViewClick(object s, ButtonBase button)
        {
            _waypoints.Add(DynelManager.LocalPlayer.Position);
        }
        private void HandleRemoveWaypointViewClick(object s, ButtonBase button)
        {
            Vector3 waypoint = _waypoints.OrderBy(c => c.DistanceFrom(DynelManager.LocalPlayer.Position)).FirstOrDefault();

            if (waypoint != Vector3.Zero)
                _waypoints.Remove(waypoint);
        }

        private void HandleClearWaypointsViewClick(object s, ButtonBase button)
        {
            _waypoints.Clear();
        }

        private void HandleExportListViewClick(object s, ButtonBase button)
        {
            var window = SettingsController.FindValidWindow(_windows);

            if (window != null && window.IsValid)
            {
                window.FindView("ListNameBox", out TextInputView nameInput);

                if (nameInput != null && !string.IsNullOrEmpty(nameInput.Text))
                {
                    SaveWaypoints(nameInput.Text);
                }
            }
        }

        private void HandleLoadListViewClick(object s, ButtonBase button)
        {
            var window = SettingsController.FindValidWindow(_windows);

            if (window != null && window.IsValid)
            {
                window.FindView("ListNameBox", out TextInputView nameInput);

                if (nameInput != null && !string.IsNullOrEmpty(nameInput.Text))
                {
                    LoadWaypoints(nameInput.Text);
                }
            }
        }

        private void HandleInfoViewClick(object s, ButtonBase button)
        {
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\RoamBuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }

        protected void RegisterSettingsWindow(string settingsName, string xmlName)
        {
            SettingsController.RegisterSettingsWindow(settingsName, PluginDir + "\\UI\\" + xmlName, _settings);
        }

        private void LoadWaypoints(string name)
        {
            string waypointFilePath = System.IO.Path.Combine(WaypointsExportPath, $"{name}.txt");

            if (!File.Exists(waypointFilePath))
            {
                Chat.WriteLine($"No such file.");
                return;
            }

            foreach (string line in File.ReadAllLines(waypointFilePath))
            {
                string[] axis = line.Split(',');
                _waypoints.Add(new Vector3(
                    float.Parse(axis[0]),
                    float.Parse(axis[1]),
                    float.Parse(axis[2]))
                );
            }
        }

        private void SaveWaypoints(string name)
        {
            if (!Directory.Exists(WaypointsExportPath))
            {
                Directory.CreateDirectory(WaypointsExportPath);
            }

            string waypointFilePath = System.IO.Path.Combine(WaypointsExportPath, $"{name}.txt");

            if (File.Exists(waypointFilePath))
            {
                Chat.WriteLine("A waypoint list already exists with this name");
                return;
            }

            string fileContent = string.Join("\n", _waypoints.Select(waypoint => $"{waypoint.X},{waypoint.Y},{waypoint.Z}"));
            File.WriteAllText(waypointFilePath, fileContent);
        }

        private void Scanning()
        {
            _bossMob = DynelManager.NPCs
                .Where(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= ScanRange
                    && !Constants._ignores.Contains(c.Name)
                    && c.Health > 0 && c.IsInLineOfSight
                    && !c.Buffs.Contains(302745)
                    && !c.Buffs.Contains(NanoLine.ShovelBuffs)
                    && !c.Buffs.Contains(253953) && !c.Buffs.Contains(205607)
                    && c.MaxHealth >= 1000000)
                .OrderBy(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position))
                .OrderByDescending(c => c.Name == "Uklesh the Beguiling")
                .OrderByDescending(c => c.Name == "Khalum the Weaver of Flesh")
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
               .OrderByDescending(c => c.Name == "Alien Cocoon" || c.Name == "Alien Coccoon")
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
                && Toggle == true)
                Scanning();

            var window = SettingsController.FindValidWindow(_windows);

            if (window != null && window.IsValid)
            {
                if (window.FindView("RoamBuddyAddWaypoint", out Button addWaypointView))
                {
                    addWaypointView.Tag = window;
                    addWaypointView.Clicked = HandleAddWaypointViewClick;
                }

                if (window.FindView("RoamBuddyRemoveWaypoint", out Button removeWaypointView))
                {
                    removeWaypointView.Tag = window;
                    removeWaypointView.Clicked = HandleRemoveWaypointViewClick;
                }

                if (window.FindView("RoamBuddyClearWaypoints", out Button clearWaypointsView))
                {
                    clearWaypointsView.Tag = window;
                    clearWaypointsView.Clicked = HandleClearWaypointsViewClick;
                }

                if (window.FindView("RoamBuddyExportList", out Button exportListView))
                {
                    exportListView.Tag = window;
                    exportListView.Clicked = HandleExportListViewClick;
                }

                if (window.FindView("RoamBuddyLoadList", out Button loadListView))
                {
                    loadListView.Tag = window;
                    loadListView.Clicked = HandleLoadListViewClick;
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

                if (SettingsController.settingsWindow.FindView("RoamBuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = HandleInfoViewClick;
                }

                if (SettingsController.settingsWindow.FindView("RoamBuddyWaypointsView", out Button waypointView))
                {
                    waypointView.Tag = SettingsController.settingsWindow;
                    waypointView.Clicked = HandleWaypointsViewClick;
                }
                if (!_settings["Toggle"].AsBool() && Toggle)
                {
                    IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
                    Stop();
                }
                if (_settings["Toggle"].AsBool() && !Toggle)
                {
                    IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = true });
                    Start();
                }
            }

            if (_waypoints.Count >= 1)
            {
                foreach (Vector3 pos in _waypoints)
                {
                    Debug.DrawSphere(pos, 0.2f, DebuggingColor.White);
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
                            IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = true });

                        _settings["Toggle"] = true;
                        Chat.WriteLine("RoamBuddy enabled.");
                        Start();

                    }
                    else
                    {
                        _settings["Toggle"] = false;
                        Chat.WriteLine("RoamBuddy disabled.");
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
                        Stop();
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
                    case "load":
                        LoadWaypoints(param[1]);
                        break;

                    case "export":
                        SaveWaypoints(param[1]);
                        break;

                    case "addpos":
                        _waypoints.Add(DynelManager.LocalPlayer.Position);
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
