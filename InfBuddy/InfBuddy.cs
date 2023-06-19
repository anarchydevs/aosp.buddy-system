using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using InfBuddy.IPCMessages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InfBuddy
{

    public class InfBuddy : AOPluginEntry
    {
        private StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; set; }

        public static Config Config { get; private set; }

        private static string InfBuddyFaction;
        private static string InfBuddyDifficulty;

        public static bool Toggle = false;
        public static bool Easy = false;
        public static bool Medium = false;
        public static bool Hard = false;
        public static bool Neutral = false;
        public static bool Clan = false;
        public static bool Omni = false;
        public static bool Normal = false;
        public static bool Roam = false;

        public static bool _easyToggled = false;
        public static bool _mediumToggled = false;
        public static bool _hardToggled = false;
        public static bool _clanToggled = false;
        public static bool _omniToggled = false;
        public static bool _neutralToggled = false;
        public static bool _normalToggled = false;
        public static bool _roamToggled = false;

        public static SimpleChar _leader;
        public static Identity Leader = Identity.None;

        public static bool DoubleReward = false;
        private static bool _initSit = false;

        private static double _sitUpdateTimer;
        public static double _stateTimeOut;

        public static List<string> _namesToIgnore = new List<string>
        {
                    "One Who Obeys Precepts",
                    "Buckethead Technodealer",
                    "The Retainer Of Ergo",
                    "Guardian Spirit of Purification"
        };

        private static Window infoWindow;

        private static string PluginDir;

        public static Settings _settings;

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("InfBuddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\InfBuddy\\{Game.ClientInst}\\Config.json");
                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);
                IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[Game.ClientInst].IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);

                IPCChannel.RegisterCallback((int)IPCOpcode.Easy, EasyMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Medium, MediumMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Hard, HardMessage);

                IPCChannel.RegisterCallback((int)IPCOpcode.Neutral, NeutralMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Clan, ClanMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Omni, OmniMessage);

                IPCChannel.RegisterCallback((int)IPCOpcode.Normal, NormalMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Roam, RoamMessage);

                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;

                Chat.RegisterCommand("buddy", InfBuddyCommand);

                SettingsController.RegisterSettingsWindow("InfBuddy", pluginDir + "\\UI\\InfBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                NpcDialog.AnswerListChanged += NpcDialog_AnswerListChanged;
                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("ModeSelection", (int)ModeSelection.Normal);
                _settings.AddVariable("FactionSelection", (int)FactionSelection.Clan);
                _settings.AddVariable("DifficultySelection", (int)DifficultySelection.Hard);

                _settings.AddVariable("Toggle", false);

                _settings.AddVariable("DoubleReward", false);
                _settings.AddVariable("Merge", false);
                _settings.AddVariable("Looting", false);

                _settings["Toggle"] = false;

                _settings["ModeSelection"] = (int)ModeSelection.Normal;
                _settings["FactionSelection"] = (int)FactionSelection.Clan;
                _settings["DifficultySelection"] = (int)DifficultySelection.Hard;

                Chat.WriteLine("InfBuddy Loaded!");
                Chat.WriteLine("/infbuddy for settings.");
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
            infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\InfBuddyInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            infoWindow.Show(true);
        }
        private void Start()
        {
            Toggle = true;

            Chat.WriteLine("InfBuddy enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Toggle = false;

            Chat.WriteLine("InfBuddy disabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());

            NavMeshMovementController.Halt();
        }

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            if (!_settings["Merge"].AsBool())
                Leader = new Identity(IdentityType.SimpleChar, sender);

            _settings["Toggle"] = true;
            Start();
        }

        private void OnStopMessage(int sender, IPCMessage msg)
        {
            _settings["Toggle"] = false;
            Stop();
        }

        private void EasyMessage(int sender, IPCMessage msg)
        {
            _settings["DifficultySelection"] = (int)DifficultySelection.Easy;
        }

        private void MediumMessage(int sender, IPCMessage msg)
        {
            _settings["DifficultySelection"] = (int)DifficultySelection.Medium;
        }

        private void HardMessage(int sender, IPCMessage msg)
        {
            _settings["DifficultySelection"] = (int)DifficultySelection.Hard;

        }

        private void NeutralMessage(int sender, IPCMessage msg)
        {
            _settings["FactionSelection"] = (int)FactionSelection.Neutral;
        }

        private void ClanMessage(int sender, IPCMessage msg)
        {
            _settings["FactionSelection"] = (int)FactionSelection.Clan;
        }

        private void OmniMessage(int sender, IPCMessage msg)
        {
            _settings["FactionSelection"] = (int)FactionSelection.Omni;
        }

        private void NormalMessage(int sender, IPCMessage msg)
        {
            _settings["ModeSelection"] = (int)ModeSelection.Normal;
        }
        private void RoamMessage(int sender, IPCMessage msg)
        {
            _settings["ModeSelection"] = (int)ModeSelection.Roam;
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning) { return; }

            _stateMachine.Tick();

            Selections();

            if (Time.NormalTime > _sitUpdateTimer + 1.5)
            {
                ListenerSit();

                _sitUpdateTimer = Time.NormalTime;
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

                if (SettingsController.settingsWindow.FindView("InfBuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = InfoView;
                }

                if (!_settings["Toggle"].AsBool() && Toggle)
                {
                    IPCChannel.Broadcast(new StopMessage());
                    Stop();
                }
                if (_settings["Toggle"].AsBool() && !Toggle)
                {
                    if (!_settings["Merge"].AsBool())
                        Leader = DynelManager.LocalPlayer.Identity;

                    IPCChannel.Broadcast(new StartMessage());
                    Start();
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

                if (Hard)
                {
                    IPCChannel.Broadcast(new HardMessage());
                    Hard = false;
                }

                if (Neutral)
                {
                    IPCChannel.Broadcast(new NeutralMessage());
                    Neutral = false;
                }
                if (Clan)
                {
                    IPCChannel.Broadcast(new ClanMessage());
                    Clan = false;
                }
                if (Omni)
                {
                    IPCChannel.Broadcast(new OmniMessage());
                    Omni = false;
                }
                if (Normal)
                {
                    IPCChannel.Broadcast(new NormalMessage());
                    Normal = false;
                }
                if (Roam)
                {
                    IPCChannel.Broadcast(new RoamMessage());
                    Roam = false;
                }
            }
        }

        public static void Selections()
        {
            switch ((DifficultySelection)_settings["DifficultySelection"].AsInt32())
            {
                case DifficultySelection.Easy:
                    Easy = true;
                    Medium = false;
                    Hard = false;
                    _easyToggled = true;
                    _mediumToggled = false;
                    _hardToggled = false;
                    break;
                case DifficultySelection.Medium:
                    Easy = false;
                    Medium = true;
                    Hard = false;
                    _easyToggled = false;
                    _mediumToggled = true;
                    _hardToggled = false;
                    break;
                case DifficultySelection.Hard:
                    Easy = false;
                    Medium = false;
                    Hard = true;
                    _easyToggled = false;
                    _mediumToggled = false;
                    _hardToggled = true;
                    break;
            }
            switch ((FactionSelection)_settings["FactionSelection"].AsInt32())
            {
                case FactionSelection.Neutral:
                    Neutral = true;
                    Clan = false;
                    Omni = false;
                    _neutralToggled = true;
                    _clanToggled = false;
                    _omniToggled = false;
                    break;
                case FactionSelection.Clan:
                    Neutral = false;
                    Clan = true;
                    Omni = false;
                    _neutralToggled = false;
                    _clanToggled = true;
                    _omniToggled = false;
                    break;
                case FactionSelection.Omni:
                    Neutral = false;
                    Clan = false;
                    Omni = true;
                    _neutralToggled = false;
                    _clanToggled = false;
                    _omniToggled = true;
                    break;
            }
            switch ((ModeSelection)_settings["ModeSelection"].AsInt32())
            {
                case ModeSelection.Normal:
                    Normal = true;
                    Roam = false;
                    _normalToggled = true;
                    _roamToggled = false;
                    break;
                case ModeSelection.Roam:
                    Normal = false;
                    Roam = true;
                    _normalToggled = false;
                    _roamToggled = true;
                    break;
            }
            //if (DifficultySelection.Easy == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && !_easyToggled)
            //{
            //    Easy = true;
            //    Medium = false;
            //    Hard = false;

            //    _easyToggled = true;
            //    _mediumToggled = false;
            //    _hardToggled = false;
            //}

            //if (DifficultySelection.Medium == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && !_mediumToggled)
            //{
            //    Easy = false;
            //    Medium = true;
            //    Hard = false;

            //    _easyToggled = false;
            //    _mediumToggled = true;
            //    _hardToggled = false;
            //}

            //if (DifficultySelection.Hard == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && !_hardToggled)
            //{
            //    Easy = false;
            //    Medium = false;
            //    Hard = true;

            //    _easyToggled = false;
            //    _mediumToggled = false;
            //    _hardToggled = true;
            //}

            //if (FactionSelection.Neutral == (FactionSelection)_settings["FactionSelection"].AsInt32() && !_neutralToggled)
            //{
            //    Neutral = true;
            //    Clan = false;
            //    Omni = false;

            //    _neutralToggled = true;
            //    _clanToggled = false;
            //    _omniToggled = false;
            //}

            //if (FactionSelection.Clan == (FactionSelection)_settings["FactionSelection"].AsInt32() && !_clanToggled)
            //{
            //    Neutral = false;
            //    Clan = true;
            //    Omni = false;

            //    _neutralToggled = false;
            //    _clanToggled = true;
            //    _omniToggled = false;
            //}

            //if (FactionSelection.Omni == (FactionSelection)_settings["FactionSelection"].AsInt32() && !_omniToggled)
            //{
            //    Neutral = false;
            //    Clan = false;
            //    Omni = true;

            //    _neutralToggled = false;
            //    _clanToggled = false;
            //    _omniToggled = true;
            //}

            //if (ModeSelection.Normal == (ModeSelection)_settings["ModeSelection"].AsInt32() && !_normalToggled)
            //{
            //    Normal = true;
            //    Roam = false;

            //    _normalToggled = true;
            //    _roamToggled = false;
            //}
            //if (ModeSelection.Roam == (ModeSelection)_settings["ModeSelection"].AsInt32() && !_roamToggled)
            //{
            //    Normal = false;
            //    Roam = true;

            //    _normalToggled = false;
            //    _roamToggled = true;
            //}
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

            Item kit = Inventory.Items.Where(x => RelevantItems.Kits.Contains(x.Id)).FirstOrDefault();

            if (kit == null) { return; }

            if (_initSit == false && spell != null)
            {
                if (!DynelManager.LocalPlayer.Buffs.Contains(280488) && CanUseSitKit())
                {
                    if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment)
                        && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
                    {
                        if (DynelManager.LocalPlayer.NanoPercent < 66 || DynelManager.LocalPlayer.HealthPercent < 66)
                        {
                            Task.Factory.StartNew(
                               async () =>
                               {
                                   _initSit = true;
                                   await Task.Delay(400);
                                   NavMeshMovementController.SetMovement(MovementAction.SwitchToSit);
                                   await Task.Delay(1200);
                                   NavMeshMovementController.SetMovement(MovementAction.LeaveSit);
                                   await Task.Delay(400);
                                   _initSit = false;
                                   _sitUpdateTimer = Time.NormalTime;
                               });
                        }
                    }
                }
            }
        }

        private void NpcDialog_AnswerListChanged(object s, Dictionary<int, string> options)
        {
            SimpleChar dialogNpc = DynelManager.GetDynel((Identity)s).Cast<SimpleChar>();

            if (dialogNpc.Name == Constants.QuestGiverName)
            {
                foreach (KeyValuePair<int, string> option in options)
                {
                    if (option.Value == "Is there anything I can help you with?" ||
                        (FactionSelection.Clan == (FactionSelection)_settings["FactionSelection"].AsInt32() && option.Value == "I will defend against the Unredeemed!") ||
                        (FactionSelection.Omni == (FactionSelection)_settings["FactionSelection"].AsInt32() && option.Value == "I will defend against the Redeemed!") ||
                        (FactionSelection.Neutral == (FactionSelection)_settings["FactionSelection"].AsInt32() && option.Value == "I will defend against the creatures of the brink!") ||
                        (DifficultySelection.Easy == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && option.Value == "I will deal with only the weakest aversaries") || //Brink missions have a typo
                        (DifficultySelection.Easy == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && option.Value == "I will deal with only the weakest adversaries") ||
                        (DifficultySelection.Medium == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && option.Value == "I will challenge these invaders, as long as there aren't too many") ||
                        (DifficultySelection.Hard == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && !_settings["DoubleReward"].AsBool() && option.Value == "I will purge the temple of any and all assailants") ||
                        (DifficultySelection.Hard == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && _settings["DoubleReward"].AsBool() && !DoubleReward && option.Value == "I will challenge these invaders, as long as there aren't too many") ||
                        (DifficultySelection.Hard == (DifficultySelection)_settings["DifficultySelection"].AsInt32() && _settings["DoubleReward"].AsBool() && DoubleReward && option.Value == "I will purge the temple of any and all assailants")
                        )
                        NpcDialog.SelectAnswer(dialogNpc.Identity, option.Key);
                }
            }
            else if (dialogNpc.Name == Constants.QuestStarterName)
            {
                foreach (KeyValuePair<int, string> option in options)
                {
                    if (option.Value == "Yes, I am ready.")
                        NpcDialog.SelectAnswer(dialogNpc.Identity, option.Key);
                }
            }
        }

        private void InfBuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Toggle"].AsBool() && !Toggle)
                    {
                        Leader = DynelManager.LocalPlayer.Identity;

                        IPCChannel.Broadcast(new StartMessage());
                        Start();
                    }
                    else
                    {
                        IPCChannel.Broadcast(new StopMessage());
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
            Normal, Roam, Leech
        }
        public enum FactionSelection
        {
            Neutral, Clan, Omni
        }
        public enum DifficultySelection
        {
            Easy, Medium, Hard
        }

        public static class RelevantItems
        {
            public static readonly int[] Kits = {
                297274, 293296, 291084, 291083, 291082
            };
        }
    }
}
