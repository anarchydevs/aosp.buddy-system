using System;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using AOSharp.Pathfinding;
using AOSharp.Core.IPC;
using AOSharp.Common.GameData.UI;
using AOSharp.Core.Movement;
using MitaarBuddy.IPCMessages;
using AOSharp.Core.Inventory;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SmokeLounge.AOtomation.Messaging.GameData;
using System.Reflection.Emit;
using System.Net.NetworkInformation;

namespace MitaarBuddy
{
    public class MitaarBuddy : AOPluginEntry
    {
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        protected double _lastZonedTime = Time.NormalTime;

        public static bool Toggle = false;
        public static bool Farming = false;
        public static bool Easy = false;
        public static bool _easyToggled = false;
        public static bool Medium = false;
        public static bool _mediumToggled = false;
        public static bool Hardcore = false;
        public static bool _hardcoreToggled = false;


        public static bool _init = false;
        public static bool _initCorpse = false;
        public static bool _repeat = false;
        public static bool _leader = false;

        public static bool _atEntrance = false;
        public static bool _atReclaim = false;

        private static SimpleChar _sinuh;
        private static Corpse _sinuhCorpse;
        private static SimpleChar _xanSpirits;
        private static SimpleChar _greenXanSpirit;
        private static SimpleChar _redXanSpirit;
        private static SimpleChar _blueXanSpirit;
        private static SimpleChar _yellowXanSpirit;
        private static SimpleChar _alienCoccoon;

        public static Vector3 _reclaim = new Vector3(610.0f, 309.8f, 519.8f);
        public static Vector3 _entrance = new Vector3(347.0f, 310.9f, 407.7f);

        public static Vector3 _greenPodium = new Vector3(108.6f, 12.1f, 110.3f);
        public static Vector3 _redPodium = new Vector3(91.3f, 12.1f, 110.2f);
        public static Vector3 _bluePodium = new Vector3(92.2f, 12.1f, 97.8f);
        public static Vector3 _yellowPodium = new Vector3(108.7f, 12.1f, 97.6f);

        public static Vector3 _startPosition = new Vector3(91.3f, 12.1f, 110.2f);

        public static Vector3 _sinuhCorpsePos = Vector3.Zero;

        public static double _time;

        public static string PluginDirectory;

        public static Window _infoWindow;

        public static Settings _settings;

        public static Identity Leader = Identity.None;

        public static List<Identity> _teamCache = new List<Identity>();
        public static List<Identity> _invitedList = new List<Identity>();

        public static List<Vector3> _pathToMitaar = new List<Vector3>()
        {
            new Vector3(610.0f, 309.8f, 519.8f),// 610.0, 519.8, 309.8 // reclaim
            new Vector3(606.1, 310.1f, 517.5f),//606.1, 517.5, 310.1 //602.0, 511.0, 310.9,
            new Vector3(594.6, 310.9f, 506.0f),//594.6, 506.0, 310.9,
            new Vector3(593.5f, 310.9f, 501.8f),// 593.5, 501.8, 310.9, 594.4, 505.1, 310.9,
            new Vector3(584.0f, 310.9f, 471.6f),// 584.0, 471.6, 310.9,
            new Vector3(605.4f, 309.4f, 446.2f),// 605.4, 446.2, 309.4
            new Vector3(594.7f, 309.1f, 392.2f),// 594.7, 392.2, 309.1
            new Vector3(563.8f, 310.9f, 381.9f),// 563.8, 381.9, 310.9
            new Vector3(536.6f, 310.9f, 370.0f),// 536.6, 370.0, 310.9
            new Vector3(526.3f, 310.9f, 329.3f),// 526.3, 329.6, 310.9
            new Vector3(484.1f, 308.6f, 329.3f),// 484.1, 329.3, 308.6
            new Vector3(438.9f, 312.1f, 355.6f),// 438.9, 355.6, 312.1
            new Vector3(388.2f, 309.0f, 383.1f),// 388.2, 383.1, 309.0,
            new Vector3(358.4f, 310.9f, 411.0f),// 358.4, 411.0, 310.9, // mitaar
            new Vector3(354.7f, 310.9f, 410.4f)//354.7, 410.4, 310.9
        };

        public static string PluginDir;


        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("MitaarBuddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\MitaarBuddy\\{Game.ClientInst}\\Config.json");

                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);

                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);

                IPCChannel.RegisterCallback((int)IPCOpcode.Farming, FarmingMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.NoFarming, NoFarmingMessage);

                IPCChannel.RegisterCallback((int)IPCOpcode.EasyMode, EasyMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.MediumMode, MediumMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.HardcoreMode, HardcoreMessage);


                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;

                SettingsController.RegisterSettingsWindow("MitaarBuddy", pluginDir + "\\UI\\MitaarBuddySettingWindow.xml", _settings);

                Chat.WriteLine("MitaarBuddy Loaded!");
                Chat.WriteLine("/mitaarbuddy for settings.");

                _settings.AddVariable("Toggle", false);
                _settings.AddVariable("Farming", false);

                _settings.AddVariable("DifficultySelection", (int)DifficultySelection.Easy);

                _settings["Toggle"] = false;
                _settings["Farming"] = false;

                _settings["DifficultySelection"] = (int)DifficultySelection.Easy;

                Chat.RegisterCommand("buddy", MitaarCommand);

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

        public static void Difficulty_Changed(object s, int e)
        {
            IPCChannel.SetChannelId(Convert.ToByte(e));
            Config.Save();
        }

        public static void Start()
        {
            Toggle = true;

        }

        private void Stop()
        {
            Toggle = false;

            NavMeshMovementController.Halt();
        }

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            if (Leader == Identity.None
                && DynelManager.LocalPlayer.Identity.Instance != sender)
                Leader = new Identity(IdentityType.SimpleChar, sender);

            
            _settings["Toggle"] = true;
            Start();
        }

        private void OnStopMessage(int sender, IPCMessage msg)
        {
            _settings["Toggle"] = false;
            Stop();
        }

        private void FarmingMessage(int sender, IPCMessage msg)
        {
            _settings["Farming"] = true;
            Farming = false;
            
        }

        private void NoFarmingMessage(int sender, IPCMessage msg)
        {
            _settings["Farming"] = false;
            Farming = true;
        }

        private void EasyMessage(int sender, IPCMessage msg)
        {
            _settings["DifficultySelection"] = (int)DifficultySelection.Easy;
        }

        private void MediumMessage(int sender, IPCMessage msg)
        {
            _settings["DifficultySelection"] = (int)DifficultySelection.Medium;
        }

        private void HardcoreMessage(int sender, IPCMessage msg)
        {
            _settings["DifficultySelection"] = (int)DifficultySelection.Hardcore;
        }


        private void HandleInfoViewClick(object s, ButtonBase button)
        {
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\MitaarBuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning) //|| Time.NormalTime < _lastZonedTime + 6.0
                return;

            Difficulty();

            if (Toggle == true && _settings["Toggle"].AsBool())
            {

                //Outside
                if (Playfield.ModelIdentity.Instance == 6013)
                {

                    //Outside mitaar not in team
                    if (!Team.IsInTeam && Time.NormalTime > _time + 5f)
                    {
                            //Inviting to team
                                foreach (SimpleChar player in DynelManager.Players.Where(c => c.IsInPlay && !_invitedList.Contains(c.Identity) && _teamCache.Contains(c.Identity)))
                                {
                                    if (_invitedList.Contains(player.Identity)) { continue; }

                                    _invitedList.Add(player.Identity);

                                    if (player.Identity == Leader) { continue; }

                                    Team.Invite(player.Identity);
                                    Chat.WriteLine($"Inviting {player.Name}");

                                }


                        _initCorpse = false;
                        _init = false;
                        _initCorpse = false;
                        _init = false;
                        _time = Time.NormalTime;

                    }


                    //Outside mitaar in team
                    if (Team.IsInTeam && Time.NormalTime > _time + 5f)
                    {

                        if (DynelManager.LocalPlayer.Position.DistanceFrom(_entrance) < 20.0f)
                        {
                            _atEntrance = true;
                            _atEntrance = true;

                            MovementController.Instance.SetDestination(new Vector3(347.0f, 310.9f, 407.7f).Randomize(2f));

                        }

                        //Pathing from reclaim
                        if (DynelManager.LocalPlayer.Position.DistanceFrom(_reclaim) < 20.0f && !_atEntrance)
                        {
                            MovementController.Instance.SetPath(_pathToMitaar);
                        }

                        _initCorpse = false;
                        _init = false;
                        _initCorpse = false;
                        _init = false;


                        _time = Time.NormalTime;
                    }
                }
               

                //Inside Mitaar
                if (Playfield.ModelIdentity.Instance == 6017)
                {
                    Mobs();

                    //Starting on red
                    if (!_init )
                    {
                        MovementController.Instance.SetDestination(_startPosition);

                        if (DynelManager.LocalPlayer.Position.DistanceFrom(_startPosition) < 1.2F 
                            && Team.Members.FirstOrDefault(c => c.Character == null) == null)
                        {
                            _init = true;
                            _atEntrance = false;
                            _repeat = false;
                            _init = true;
                            _atEntrance = false;
                            _repeat = false;

                            _invitedList.Clear();
                            Leader = DynelManager.LocalPlayer.Identity;
                        }
                    }


                    if (DifficultySelection.Easy == (DifficultySelection)_settings["DifficultySelection"].AsInt32())
                    {
                        //Attack and initial start
                        if (_sinuh != null && _alienCoccoon == null && _init && _xanSpirits == null)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                                DynelManager.LocalPlayer.Attack(_sinuh);
                            _repeat = false;
                            _repeat = false;
                        }

                        if (_sinuh != null && _alienCoccoon != null || _xanSpirits != null)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget != null
                                && DynelManager.LocalPlayer.FightingTarget.Name == _sinuh.Name)
                            {
                                DynelManager.LocalPlayer.StopAttack();
                            }
                        }

                        if (_alienCoccoon != null)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                            {
                                DynelManager.LocalPlayer.Attack(_alienCoccoon);
                            }
                        }

                        //Pathing to spirits
                        if (_xanSpirits != null && !MovementController.Instance.IsNavigating)
                        {
                            if (_redXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(_redPodium) > 0.9f
                                && !DynelManager.LocalPlayer.Buffs.Contains(SpiritNanos.BlessingofTheBlood))
                                MovementController.Instance.SetDestination(_redPodium);

                            if (_blueXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(_bluePodium) > 0.9f
                                && !DynelManager.LocalPlayer.Buffs.Contains(SpiritNanos.BlessingofTheSource))
                                MovementController.Instance.SetDestination(_bluePodium);

                            if (_greenXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(_greenPodium) > 0.9f
                                && !DynelManager.LocalPlayer.Buffs.Contains(SpiritNanos.BlessingofTheOutsider))
                                MovementController.Instance.SetDestination(_greenPodium);

                            if (_yellowXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(_yellowPodium) > 0.9f
                                && !DynelManager.LocalPlayer.Buffs.Contains(SpiritNanos.BlessingofTheLight))
                                MovementController.Instance.SetDestination(_yellowPodium);
                        }
                    }

                    if (DifficultySelection.Medium == (DifficultySelection)_settings["DifficultySelection"].AsInt32())
                    {
                        //Attack and initial start
                        if (_sinuh != null && _alienCoccoon == null && _init)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                                DynelManager.LocalPlayer.Attack(_sinuh);
                            _repeat = false;
                            _repeat = false;
                        }

                        if (_sinuh != null && _alienCoccoon != null)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget != null
                                && DynelManager.LocalPlayer.FightingTarget.Name == _sinuh.Name)
                            {
                                DynelManager.LocalPlayer.StopAttack();
                            }
                        }

                        if (_alienCoccoon != null)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                            {
                                DynelManager.LocalPlayer.Attack(_alienCoccoon);
                            }
                        }

                        //Pathing to spirits
                        if (_xanSpirits != null && !MovementController.Instance.IsNavigating)
                        {
                            if (_redXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(_redPodium) > 0.9f
                                && !DynelManager.LocalPlayer.Buffs.Contains(SpiritNanos.BlessingofTheBlood))
                                MovementController.Instance.SetDestination(_redPodium);

                        }
                    }

                    if (DifficultySelection.Hardcore == (DifficultySelection)_settings["DifficultySelection"].AsInt32())
                    {
                        //Attack and initial start
                        if (_sinuh != null && _alienCoccoon == null && _init )
                        {
                            if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                                DynelManager.LocalPlayer.Attack(_sinuh);
                            _repeat = false;
                            _repeat = false;
                        }

                        if (_sinuh != null && _alienCoccoon != null )
                        {
                            if (DynelManager.LocalPlayer.FightingTarget != null
                                && DynelManager.LocalPlayer.FightingTarget.Name == _sinuh.Name)
                            {
                                DynelManager.LocalPlayer.StopAttack();
                            }
                        }

                        if (_alienCoccoon != null)
                        {
                            if (DynelManager.LocalPlayer.FightingTarget == null && !DynelManager.LocalPlayer.IsAttackPending)
                            {
                                DynelManager.LocalPlayer.Attack(_alienCoccoon);
                            }
                        }

                        //Pathing to spirits
                        if (_xanSpirits != null && !MovementController.Instance.IsNavigating)
                        {
                            if (_redXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(_redPodium) > 0.9f
                                && !DynelManager.LocalPlayer.Buffs.Contains(SpiritNanos.BlessingofTheBlood))
                                MovementController.Instance.SetDestination(_redPodium);

                            if (_blueXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(_bluePodium) > 0.9f
                                && !DynelManager.LocalPlayer.Buffs.Contains(SpiritNanos.BlessingofTheSource))
                                MovementController.Instance.SetDestination(_bluePodium);

                            if (_greenXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(_greenPodium) > 0.9f
                                && !DynelManager.LocalPlayer.Buffs.Contains(SpiritNanos.BlessingofTheOutsider))
                                MovementController.Instance.SetDestination(_greenPodium);

                            if (_yellowXanSpirit != null && DynelManager.LocalPlayer.Position.DistanceFrom(_yellowPodium) > 0.9f
                                && !DynelManager.LocalPlayer.Buffs.Contains(SpiritNanos.BlessingofTheLight))
                                MovementController.Instance.SetDestination(_yellowPodium);
                        }
                    }

                    if (IsSettingEnabled("Farming"))
                    {
                        if (_sinuhCorpse != null && _xanSpirits == null && _alienCoccoon == null)
                        {
                            _sinuhCorpsePos = (Vector3)_sinuhCorpse?.Position;

                            //Path to corpse
                            if (DynelManager.LocalPlayer.Position.DistanceFrom(_sinuhCorpsePos) > 3.0f)
                                MovementController.Instance.SetDestination(_sinuhCorpsePos);


                            if (!_initCorpse && Team.IsInTeam)
                            {
                                if (Team.IsLeader && !_leader)
                                {

                                    Leader = DynelManager.LocalPlayer.Identity;

                                    _leader = true;
                                }

                                if (Team.IsInTeam && _leader)
                                {
                                    Task.Factory.StartNew(
                                        async () =>
                                        {
                                            //Team save and disbanding
                                            foreach (SimpleChar player in DynelManager.Players.Where(c => c.IsInPlay && !_teamCache.Contains(c.Identity)))
                                            {
                                                if (!_teamCache.Contains(player.Identity))
                                                    _teamCache.Add(player.Identity);
                                                //Chat.WriteLine($"Player {player.Identity} added");

                                                _invitedList.Clear();
                                                _invitedList.Clear();
                                            }

                                            await Task.Delay(15000);
                                            Team.Disband();

                                            _invitedList.Clear();
                                            _leader = false;
                                            _repeat = true;
                                            _initCorpse = true;
                                            _initCorpse = true;

                                        });
                                }

                                _invitedList.Clear();
                                _leader = false;
                                _repeat = true;

                            }
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

                    if (SettingsController.settingsWindow.FindView("MitaarBuddyInfoView", out Button infoView))
                    {
                        infoView.Tag = SettingsController.settingsWindow;
                        infoView.Clicked = HandleInfoViewClick;
                    }

                    if (!_settings["Toggle"].AsBool() && Toggle)
                    {
                        IPCChannel.Broadcast(new StopMessage());
                        Toggle = false;

                        Chat.WriteLine("MitaarBuddy disabled.");
                    }
                    if (_settings["Toggle"].AsBool() && !Toggle)
                    {
                        IPCChannel.Broadcast(new StartMessage());
                        if (Team.IsLeader)
                        {
                            Leader = DynelManager.LocalPlayer.Identity;
                        }
                        Toggle = true;
                        Chat.WriteLine("MitaarBuddy enabled.");
                    }

                    if (!_settings["Farming"].AsBool() && Farming)
                    {
                        IPCChannel.Broadcast(new NoFarmingMessage());
                        Farming = false;
                        Chat.WriteLine("Farming disabled");
                    }

                    if (_settings["Farming"].AsBool() && !Farming)
                    {
                        IPCChannel.Broadcast(new FarmingMessage());
                        Farming = true;
                        Chat.WriteLine("Farming enabled.");
                }


                    if (Easy)
                    {
                        IPCChannel.Broadcast(new EasyMessage());
                        Easy = false;
                    }

                    if (Medium)
                    {
                        IPCChannel.Broadcast(new MediumMessage());
                        Medium = false;
                    }

                    if (Hardcore)
                    {
                        IPCChannel.Broadcast(new HardecoreMessage());
                        Hardcore = false;
                    }

            }
        }


        private void MitaarCommand(string command, string[] param, ChatWindow chatWindow)
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
                        Leader = DynelManager.LocalPlayer.Identity;
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

        public static void Difficulty()
        {
            if (DifficultySelection.Easy == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && !_easyToggled)
            {
                Easy = true;
                Medium = false;
                Hardcore = false;

                _easyToggled = true;
                _mediumToggled = false;
                _hardcoreToggled = false;

                Chat.WriteLine("Stop being a Primitive Screwhead.");
            }

            if (DifficultySelection.Medium == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && !_mediumToggled)
            {
                Easy = false;
                Medium = true;
                Hardcore = false;

                _easyToggled = false;
                _mediumToggled = true;
                _hardcoreToggled = false;

                Chat.WriteLine("Okay, a little better. Groovy.");
            }

            if (DifficultySelection.Hardcore == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && !_hardcoreToggled)
            {
                Easy = false;
                Medium = false;
                Hardcore = true;

                _easyToggled = false;
                _mediumToggled = false;
                _hardcoreToggled = true;

                Chat.WriteLine("Destroyer! Hail to the king, baby.");
            }
        }

        public static void Mobs()
        {

            _sinuh = DynelManager.NPCs
                 .Where(c => c.Health > 0
                  && c.Name.Contains("Technomaster Sinuh")
                  && !c.Name.Contains("Remains of"))
                  .FirstOrDefault();

            _alienCoccoon = DynelManager.NPCs
               .Where(c => c.Health > 0
                       && c.Name.Contains("Alien Coccoon"))
                   .FirstOrDefault();

            _xanSpirits = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.Name.Contains("Xan Spirit"))
                    .FirstOrDefault();

            _redXanSpirit = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.Name.Contains("Xan Spirit")
                    && c.Buffs.Contains(SpiritNanos.BlessingofTheBlood))
                    .FirstOrDefault();

            _blueXanSpirit = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.Name.Contains("Xan Spirit")
                    && c.Buffs.Contains(SpiritNanos.BlessingofTheSource))
                    .FirstOrDefault();

            _greenXanSpirit = DynelManager.NPCs
               .Where(c => c.Health > 0
                   && c.Name.Contains("Xan Spirit")
                   && c.Buffs.Contains(SpiritNanos.BlessingofTheOutsider))
                   .FirstOrDefault();

            _yellowXanSpirit = DynelManager.NPCs
                .Where(c => c.Health > 0
                    && c.Name.Contains("Xan Spirit")
                    && c.Buffs.Contains(SpiritNanos.BlessingofTheLight))
                    .FirstOrDefault();

            _sinuhCorpse = DynelManager.Corpses
              .Where(c => c.Name.Contains("Remains of Technomaster Sinuh"))
                  .FirstOrDefault();

        }

        protected bool IsSettingEnabled(string settingName)
        {
            return _settings[settingName].AsBool();
        }

        private static class SpiritNanos
        {
            public const int BlessingofTheBlood = 280472; //Red
            public const int BlessingofTheSource = 280521; //Blue
            public const int BlessingofTheOutsider = 280493; //Green
            public const int BlessingofTheLight = 280496;  //Yellow
        }

        public enum DifficultySelection
        {
            Easy, Medium, Hardcore
        }
    }
}
