using System;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using AOSharp.Pathfinding;
using AOSharp.Core.IPC;
using AOSharp.Common.GameData.UI;
using AOSharp.Core.Movement;
using DB2Buddy.IPCMessages;
using AOSharp.Core.Inventory;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DB2Buddy
{
    public class DB2Buddy : AOPluginEntry
    {
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static bool Toggle = false;
        public static bool _init = false;
        public static bool _initLol = false;
        public static bool _initStart = false;
        public static bool _initTower = false;
        public static bool _initCorpse = false;
        public static bool IsLeader = false;
        public static bool _repeat = false;

        public static double _time;

        public static string PluginDirectory;

        public static Window _infoWindow;

        public static Settings _settings;

        public static List<Identity> _teamCache = new List<Identity>();

        public static List<Vector3> _pathToAune = new List<Vector3>()
        {
            new Vector3(280.1f, 135.3f, 143.7f),
            new Vector3(293.7f, 135.3f, 149.0f),
            new Vector3(301.6f, 135.3f, 159.3f),
            new Vector3(294.1f, 135.4f, 197.1f),
            new Vector3(273.9f, 135.4f, 197.9f),
            new Vector3(269.5f, 135.4f, 201.2f),
            new Vector3(271.3f, 134.5f, 204.9f),
            new Vector3(267.6f, 133.4f, 208.3f),
            new Vector3(265.8f, 133.4f, 217.7f),
            new Vector3(274.1f, 133.4f, 224.0f),
            new Vector3(279.2f, 133.4f, 223.9f),
            new Vector3(286.4f, 133.4f, 230.8f)
        };

        public static string PluginDir;


        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("DB2Buddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\DB2Buddy\\{Game.ClientInst}\\Config.json");

                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);

                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);

                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;

                SettingsController.RegisterSettingsWindow("DB2Buddy", pluginDir + "\\UI\\DB2BuddySettingWindow.xml", _settings);

                Chat.WriteLine("DB2Buddy Loaded!");
                Chat.WriteLine("/db2buddy for settings.");

                _settings.AddVariable("Toggle", false);

                _settings["Toggle"] = false;

                Chat.RegisterCommand("buddy", DB2BuddyCommand);

                Game.OnUpdate += OnUpdate;
            }
            catch(Exception e)
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

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            Toggle = true;
            _settings["Toggle"] = true;
        }

        private void OnStopMessage(int sender, IPCMessage msg)
        {
            Toggle = false;
            _settings["Toggle"] = false;
        }

        private void HandleInfoViewClick(object s, ButtonBase button)
        {
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\DB2BuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning)
                return;

            if (_settings["Toggle"].AsBool())
            {
                SimpleChar aune = DynelManager.NPCs
                   .Where(c => c.Health > 0
                       && c.Name.Contains("Ground Chief Aune"))
                   .FirstOrDefault();

                Dynel tower = DynelManager.AllDynels
                    .Where(c => c.Identity.Type != IdentityType.Corpse 
                        && c.Name.Contains("Strange Xan Artifact"))
                    .FirstOrDefault();

                SimpleChar towerChar = DynelManager.NPCs
                    .Where(c => c.Health == 0
                        && c.Name.Contains("Strange Xan Artifact"))
                    .FirstOrDefault();

                Dynel mist = DynelManager.AllDynels
                    .Where(c => c.Name.Contains("Notum Irregularity"))
                    .FirstOrDefault();

                Corpse _aune = DynelManager.Corpses.FirstOrDefault(c => c.Name.Contains("Aune"));

                if (_aune != null)
                {
                    DynelManager.LocalPlayer.Position = (Vector3)_aune?.Position;
                    MovementController.Instance.SetMovement(MovementAction.Update);

                    if (!_initCorpse && Team.IsInTeam)
                    {
                        _initCorpse = true;

                        Task.Factory.StartNew(
                            async () =>
                            {
                                foreach (Identity identity in Team.Members.Select(c => c.Identity))
                                {
                                    if (!_teamCache.Contains(identity))
                                        _teamCache.Add(identity);
                                }

                                await Task.Delay(10000);
                                Team.Leave();
                                _repeat = true;
                            });
                    }
                }

                //Outside db2
                if (Playfield.ModelIdentity.Instance == 570 && Time.NormalTime > _time + 5f)
                {
                    _time = Time.NormalTime;

                    if (_repeat)
                    {
                        _initCorpse = false;
                        _initStart = false;
                        _initLol = false;
                        _init = false;

                        if (IsLeader)
                        {
                            foreach (Identity identity in _teamCache)
                            {
                                Team.Invite(identity);
                            }
                        }

                        _repeat = false;
                    }

                    MovementController.Instance.SetDestination(new Vector3(2121.8f, 0.4f, 2769.1f).Randomize(2f));
                    
                }

                //Inside db2
                if (Playfield.ModelIdentity.Instance == 6055
                    && !_init
                    && !_initLol)
                {
                    _init = true;
                }

                if (Playfield.ModelIdentity.Instance == 6055
                    && _init && Time.NormalTime > _time + 2f && !_initLol)
                {
                    _time = Time.NormalTime;

                    if (Team.Members.FirstOrDefault(c => c.Character == null) == null)
                    {
                        _init = false;
                        _initLol = true;
                        MovementController.Instance.SetPath(_pathToAune);
                    }
                }

                if (mist != null && tower == null && _aune == null)
                {
                    DynelManager.LocalPlayer.Position = (Vector3)mist?.Position;
                    MovementController.Instance.SetMovement(MovementAction.Update);
                }

                if (aune != null && DynelManager.LocalPlayer.Position.DistanceFrom(aune.Position) >= 3f
                    && mist == null
                    && (tower == null
                        || towerChar != null)
                    && _initStart)
                {
                    DynelManager.LocalPlayer.Position = (Vector3)aune?.Position;
                    MovementController.Instance.SetMovement(MovementAction.Update);
                }


                //Attack and initial start
                if (aune != null && DynelManager.LocalPlayer.Position.DistanceFrom(aune.Position) < 30f
                    && !DynelManager.Players.Any(c => c.Buffs.Contains(274101))
                    && aune.Buffs.Contains(273220) == false)
                {
                    if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                        DynelManager.LocalPlayer.Attack(aune);

                    if (DynelManager.LocalPlayer.Position.DistanceFrom(aune.Position) < 3f)
                        _initStart = true;
                }


                //Has buff stop and move to tower
                if (aune != null && aune.Buffs.Contains(273220) == true
                    || DynelManager.Players.Any(c => c.Buffs.Contains(274101)))
                {
                    if (DynelManager.LocalPlayer.FightingTarget != null
                        && DynelManager.LocalPlayer.FightingTarget.Name == aune.Name)
                    {
                        DynelManager.LocalPlayer.StopAttack();
                    }

                    if (tower == null)
                    {
                        DynelManager.LocalPlayer.Position = new Vector3(285.7f, 133.3f, 232.9f);
                        MovementController.Instance.SetMovement(MovementAction.Update);
                    }

                    if (tower != null)
                    {
                        DynelManager.LocalPlayer.Position = tower.Position;
                        MovementController.Instance.SetMovement(MovementAction.Update);

                        if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                        {
                            DynelManager.LocalPlayer.Attack(tower);
                        }
                    }
                }
            }

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);

                if (channelInput != null)
                {
                    if (int.TryParse(channelInput.Text, out int channelValue)
                        && Config.CharSettings[Game.ClientInst].IPCChannel != channelValue)
                    {
                        Config.CharSettings[Game.ClientInst].IPCChannel = channelValue;
                    }
                }

                if (SettingsController.settingsWindow.FindView("DB2BuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = HandleInfoViewClick;
                }

                if (!_settings["Toggle"].AsBool() && Toggle)
                {
                    IPCChannel.Broadcast(new StopMessage());
                    Toggle = false;
                }
                if (_settings["Toggle"].AsBool() && !Toggle)
                {
                    IPCChannel.Broadcast(new StartMessage());
                    Toggle = true;
                }
            }
        }

        private void DB2BuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Toggle"].AsBool())
                    {
                        _settings["Toggle"] = true;
                        IPCChannel.Broadcast(new StartMessage());
                        Toggle = true;
                        IsLeader = true;
                        Chat.WriteLine("Bot enabled.");
                    }
                    else if (_settings["Toggle"].AsBool())
                    {
                        _settings["Toggle"] = false;
                        IPCChannel.Broadcast(new StopMessage());
                        Toggle = false;
                        Chat.WriteLine("Bot disabled.");
                    }
                }
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
