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

        public static SimpleChar _leader;
        public static Identity Leader = Identity.None;

        public static bool DoubleReward = false;
        private static bool Sitting = false;

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

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\InfBuddy\\{Game.ClientInst}\\Config.json");
                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);
                IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[Game.ClientInst].IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);

                Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;

                Chat.RegisterCommand("buddy", InfBuddyCommand);

                SettingsController.RegisterSettingsWindow("InfBuddy", pluginDir + "\\UI\\InfBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                NpcDialog.AnswerListChanged += NpcDialog_AnswerListChanged;
                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("ModeSelection", (int)ModeSelection.Roam);
                _settings.AddVariable("FactionSelection", (int)FactionSelection.Omni);
                _settings.AddVariable("DifficultySelection", (int)DifficultySelection.Hard);

                _settings.AddVariable("Toggle", false);

                _settings.AddVariable("DoubleReward", false);
                _settings.AddVariable("Merge", false);

                _settings["Toggle"] = false;

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


        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning) { return; }

            _stateMachine.Tick();

            if (Time.NormalTime > _sitUpdateTimer + 1)
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
            }
        }

        private bool CanUseSitKit()
        {
            if (Inventory.Find(297274, out Item premSitKit))
                if (DynelManager.LocalPlayer.Health > 0 && !Extensions.InCombat()
                                    && !DynelManager.LocalPlayer.IsMoving && !Game.IsZoning) { return true; }

            if (DynelManager.LocalPlayer.IsAlive && !Extensions.InCombat()
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

            if (spell != null)
            {
                if (!DynelManager.LocalPlayer.Buffs.Contains(280488) && CanUseSitKit())
                {
                    if (spell != null && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment) && Sitting == false
                        && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
                    {
                        if (DynelManager.LocalPlayer.NanoPercent < 66 || DynelManager.LocalPlayer.HealthPercent < 66)
                        {
                            Task.Factory.StartNew(
                               async () =>
                               {
                                   Sitting = true;
                                   await Task.Delay(400);
                                   NavMeshMovementController.SetMovement(MovementAction.SwitchToSit);
                                   await Task.Delay(800);
                                   NavMeshMovementController.SetMovement(MovementAction.LeaveSit);
                                   await Task.Delay(200);
                                   Sitting = false;
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
