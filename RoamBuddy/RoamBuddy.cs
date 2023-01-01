using System;
using System.Collections.Generic;
using System.Linq;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Core.Movement;
using AOSharp.Common.GameData;
using AOSharp.Pathfinding;
using System.Data;
using AOSharp.Core.IPC;
using RoamBuddy.IPCMessages;
using AOSharp.Common.GameData.UI;
using System.IO;
using System.Globalization;
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


        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("RoamBuddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy\\{Game.ClientInst}\\Config.json");
                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.AttackRange, OnAttackRangeMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.ScanRange, OnScanRangeMessage);

                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;
                Config.CharSettings[Game.ClientInst].AttackRangeChangedEvent += AttackRange_Changed;
                Config.CharSettings[Game.ClientInst].ScanRangeChangedEvent += ScanRange_Changed;

                Chat.RegisterCommand("buddy", RoamBuddyCommand);

                SettingsController.RegisterSettingsWindow("RoamBuddy", pluginDir + "\\UI\\RoamBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("ModeSelection", (int)ModeSelection.Taunt);

                _settings.AddVariable("Toggle", false);
                _settings.AddVariable("Looting", false);

                _settings["Toggle"] = false;

                Chat.WriteLine("RoamBuddy Loaded!");
                Chat.WriteLine("/roambuddy for settings.");

                AttackRange = Config.CharSettings[Game.ClientInst].AttackRange;
                ScanRange = Config.CharSettings[Game.ClientInst].ScanRange;
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

        public Window[] _windows => new Window[] { _waypointWindow };

        public static void IPCChannel_Changed(object s, int e)
        {
            IPCChannel.SetChannelId(Convert.ToByte(e));
            Config.Save();
        }
        public static void AttackRange_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].AttackRange = e;
            AttackRange = e;
            Config.Save();
        }
        public static void ScanRange_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].ScanRange = e;
            ScanRange = e;
            Config.Save();
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

            Config.CharSettings[Game.ClientInst].AttackRange = rangeMsg.Range;
        }

        private void OnScanRangeMessage(int sender, IPCMessage msg)
        {
            ScanRangeMessage rangeMsg = (ScanRangeMessage)msg;

            Config.CharSettings[Game.ClientInst].ScanRange = rangeMsg.Range;
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
                    if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp"))
                        Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp");

                    if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy"))
                        Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy");

                    if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy\\Exports"))
                        Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy\\Exports");

                    string _exportPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\AOSharp\RoamBuddy\Exports\{nameInput.Text}.txt";

                    if (!File.Exists(_exportPath))
                    {
                        // Create the file.
                        using (FileStream fs = File.Create(_exportPath))
                        {
                            foreach (Vector3 vector in _waypoints)
                            {
                                string vectorstring = vector.ToString();
                                string vectorstring2 = vectorstring.Substring(1, vectorstring.Length - 2);

                                Byte[] info =
                                        new UTF8Encoding(true).GetBytes($"{vectorstring2}-");

                                fs.Write(info, 0, info.Length);
                            }
                        }
                    }
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
                    if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp"))
                        Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp");

                    if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy"))
                        Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy");

                    if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy\\Exports"))
                        Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy\\Exports");

                    string _loadPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\AOSharp\RoamBuddy\Exports\{nameInput.Text}.txt";

                    if (!File.Exists(_loadPath))
                    {
                        Chat.WriteLine($"No such file.");
                        return;
                    }

                    using (StreamReader sr = File.OpenText(_loadPath))
                    {
                        string[] _stringAsArray = sr.ReadLine().Split('-');

                        foreach (string _string in _stringAsArray)
                        {
                            if (_string.Length > 1)
                            {
                                float x, y, z;

                                string[] _stringAsArraySplit = _string.Split(',');
                                if (_string.Contains('.'))
                                {
                                    x = float.Parse(_stringAsArraySplit[0], CultureInfo.InvariantCulture.NumberFormat);
                                    y = float.Parse(_stringAsArraySplit[1], CultureInfo.InvariantCulture.NumberFormat);
                                    z = float.Parse(_stringAsArraySplit[2], CultureInfo.InvariantCulture.NumberFormat);
                                }
                                else
                                {
                                    x = float.Parse($"{_stringAsArraySplit[0]}.{_stringAsArraySplit[1]}", CultureInfo.InvariantCulture.NumberFormat);
                                    y = float.Parse($"{_stringAsArraySplit[2]}.{_stringAsArraySplit[3]}", CultureInfo.InvariantCulture.NumberFormat);
                                    z = float.Parse($"{_stringAsArraySplit[4]}.{_stringAsArraySplit[5]}", CultureInfo.InvariantCulture.NumberFormat);
                                }

                                _waypoints.Add(new Vector3(x, y, z));
                            }
                        }
                    }
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

        private void Scanning()
        {
            _bossMob = DynelManager.NPCs
                .Where(c => c.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= ScanRange
                    && !Constants._ignores.Contains(c.Name)
                    && c.Health > 0 && c.IsInLineOfSight
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
                        && Config.CharSettings[Game.ClientInst].IPCChannel != channelValue)
                    {
                        Config.CharSettings[Game.ClientInst].IPCChannel = channelValue;
                    }
                }
                if (attackRangeInput != null && !string.IsNullOrEmpty(attackRangeInput.Text))
                {
                    if (int.TryParse(attackRangeInput.Text, out int attackRangeInputValue)
                        && Config.CharSettings[Game.ClientInst].AttackRange != attackRangeInputValue)
                    {
                        Config.CharSettings[Game.ClientInst].AttackRange = attackRangeInputValue;
                        IPCChannel.Broadcast(new AttackRangeMessage()
                        {
                            Range = attackRangeInputValue
                        });
                    }
                }
                if (scanRangeInput != null && !string.IsNullOrEmpty(scanRangeInput.Text))
                {
                    if (int.TryParse(scanRangeInput.Text, out int scanRangeInputValue)
                        && Config.CharSettings[Game.ClientInst].ScanRange != scanRangeInputValue)
                    {
                        Config.CharSettings[Game.ClientInst].ScanRange = scanRangeInputValue;
                        IPCChannel.Broadcast(new ScanRangeMessage()
                        {
                            Range = scanRangeInputValue
                        });
                    }
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

                if (_settings["Toggle"].AsBool() && !Toggle)
                {
                    IsLeader = true;
                    Leader = DynelManager.LocalPlayer.Identity;

                    if (DynelManager.LocalPlayer.Identity == Leader)
                        IPCChannel.Broadcast(new StartMessage());

                    Chat.WriteLine("RoamBuddy enabled.");
                    Start();
                }
                if (!_settings["Toggle"].AsBool() && Toggle)
                {
                    Stop();
                    Chat.WriteLine("RoamBuddy disabled.");
                    IPCChannel.Broadcast(new StopMessage());
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

        private void RoamBuddyCommand(string command, string[] param, ChatWindow chatWindow)
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
                        Chat.WriteLine("RoamBuddy enabled.");
                        Start();

                    }
                    else
                    {
                        Stop();
                        Chat.WriteLine("RoamBuddy disabled.");
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
                    case "load":
                        if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp"))
                            Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp");

                        if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy"))
                            Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy");

                        if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy\\Exports"))
                            Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy\\Exports");

                        string _loadPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\AOSharp\RoamBuddy\Exports\{param[1]}.txt";

                        if (!File.Exists(_loadPath))
                        {
                            Chat.WriteLine($"No such file.");
                            return;
                        }

                        using (StreamReader sr = File.OpenText(_loadPath))
                        {
                            string[] _stringAsArray = sr.ReadLine().Split('-');

                            foreach (string _string in _stringAsArray)
                            {
                                if (_string.Length > 1)
                                {
                                    float x, y, z;

                                    string[] _stringAsArraySplit = _string.Split(',');
                                    if (_string.Contains('.'))
                                    {
                                        x = float.Parse(_stringAsArraySplit[0], CultureInfo.InvariantCulture.NumberFormat);
                                        y = float.Parse(_stringAsArraySplit[1], CultureInfo.InvariantCulture.NumberFormat);
                                        z = float.Parse(_stringAsArraySplit[2], CultureInfo.InvariantCulture.NumberFormat);
                                    }
                                    else
                                    {
                                        x = float.Parse($"{_stringAsArraySplit[0]}.{_stringAsArraySplit[1]}", CultureInfo.InvariantCulture.NumberFormat);
                                        y = float.Parse($"{_stringAsArraySplit[2]}.{_stringAsArraySplit[3]}", CultureInfo.InvariantCulture.NumberFormat);
                                        z = float.Parse($"{_stringAsArraySplit[4]}.{_stringAsArraySplit[5]}", CultureInfo.InvariantCulture.NumberFormat);
                                    }

                                    _waypoints.Add(new Vector3(x, y, z));
                                }
                            }
                        }

                        break;

                    case "export":
                        if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp"))
                            Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp");

                        if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy"))
                            Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy");

                        if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy\\Exports"))
                            Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\RoamBuddy\\Exports");

                        string _exportPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\AOSharp\RoamBuddy\Exports\{param[1]}.txt";

                        if (!File.Exists(_exportPath))
                        {
                            // Create the file.
                            using (FileStream fs = File.Create(_exportPath))
                            {
                                foreach (Vector3 vector in _waypoints)
                                {
                                    string vectorstring = vector.ToString();
                                    string vectorstring2 = vectorstring.Substring(1, vectorstring.Length - 2);

                                    Byte[] info =
                                            new UTF8Encoding(true).GetBytes($"{vectorstring2}-");

                                    fs.Write(info, 0, info.Length);
                                }
                            }
                        }
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
