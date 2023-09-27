using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Pathfinding;
using MitaarBuddy.IPCMessages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MitaarBuddy
{
    public class MitaarBuddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        public static Identity Leader = Identity.None;
        public static SimpleChar _leader;

        public static Vector3 _sinuhCorpsePos = Vector3.Zero;

        private Stopwatch _kitTimer = new Stopwatch();

        public static bool Toggle = false;
        public static bool Farming = false;

        //public static bool _initCorpse = false;

        public static bool Easy = false;
        public static bool _easyToggled = false;
        public static bool Medium = false;
        public static bool _mediumToggled = false;
        public static bool Hardcore = false;
        public static bool _hardcoreToggled = false;

        public static bool SinuhCorpse = false;

        public static bool _died = false;

        public static double _stateTimeOut;

        public static Window _infoWindow;

        public static Settings _settings;

        public static string PluginDir;

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("MitaarBuddy");
                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\MitaarBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");
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

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                Chat.RegisterCommand("buddy", BuddyCommand);

                SettingsController.RegisterSettingsWindow("MitaarBuddy", pluginDir + "\\UI\\MitaarBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Team.TeamRequest += OnTeamRequest;
                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("DifficultySelection", (int)DifficultySelection.Easy);

                _settings.AddVariable("Toggle", false);
                _settings.AddVariable("Farming", false);
                _settings.AddVariable("Leader", false);

                _settings["Toggle"] = false;
                _settings["Farming"] = false;
                

                _settings["DifficultySelection"] = (int)DifficultySelection.Easy;

                Chat.WriteLine("MitaarBuddy Loaded!");
                Chat.WriteLine("/mitaarbuddy for settings.");
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

        public static void Start()
        {
            if (_settings["Leader"].AsBool())
            {
                Leader = DynelManager.LocalPlayer.Identity;
            }

            Toggle = true;

            Chat.WriteLine("MitaarBuddy enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Toggle = false;

            Chat.WriteLine("MitaarBuddy disabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());

            NavMeshMovementController.Halt();
        }

        private void farmingEnabled()
        {
            Farming = true;
        }
        private void farmingDisabled()
        {
            Farming = false;
        }

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            //if (Leader == Identity.None
            //    && DynelManager.LocalPlayer.Identity.Instance != sender)
            //    Leader = new Identity(IdentityType.SimpleChar, sender);

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
            farmingEnabled();
        }

        private void NoFarmingMessage(int sender, IPCMessage msg)
        {
            _settings["Farming"] = false;
            farmingDisabled();
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
            if (Game.IsZoning)
                return;

            Difficulty();

                SitAndUseKit();

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

                if (SettingsController.settingsWindow.FindView("MitaarBuddyInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = HandleInfoViewClick;
                }

                if (!_settings["Toggle"].AsBool() && Toggle)
                {
                    IPCChannel.Broadcast(new StopMessage());
                    Stop();
                }
                if (_settings["Toggle"].AsBool() && !Toggle)
                {
                    //Leader = DynelManager.LocalPlayer.Identity;
                    IPCChannel.Broadcast(new StartMessage());
                    Start();
                }

                if (!_settings["Farming"].AsBool() && Farming) // Farming off
                {
                    IPCChannel.Broadcast(new NoFarmingMessage());
                    Chat.WriteLine("Farming disabled");
                    farmingDisabled();
                }

                if (_settings["Farming"].AsBool() && !Farming) // farming on
                {
                    IPCChannel.Broadcast(new FarmingMessage());
                    Chat.WriteLine("Farming enabled.");
                    farmingEnabled();
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
            if (_settings["Toggle"].AsBool())
            {
                _stateMachine.Tick();
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

        private void OnTeamRequest(object s, TeamRequestEventArgs e)
        {
            if (e.Requester != Leader)
            {
                if (Toggle)
                    e.Ignore();

                return;
            }

            e.Accept();
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
            bool canSit = !localPlayer.Cooldowns.ContainsKey(Stat.Treatment) && localPlayer.MovementState != MovementState.Sit;

            bool shouldStand = localPlayer.NanoPercent > 66 || localPlayer.HealthPercent > 66;
            bool onCooldown = localPlayer.Cooldowns.ContainsKey(Stat.Treatment);

            if (shouldSit && canSit)
            {
                MovementController.Instance.SetMovement(MovementAction.SwitchToSit);
            }
            else if (shouldStand && onCooldown)
            {
                MovementController.Instance.SetMovement(MovementAction.LeaveSit);
            }
        }

        private bool CanUseSitKit()
        {
            if (!DynelManager.LocalPlayer.IsAlive || DynelManager.LocalPlayer.IsMoving || Game.IsZoning)
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
                    if (!_settings["Toggle"].AsBool())
                    {
                        _settings["Toggle"] = true;
                        IPCChannel.Broadcast(new StartMessage());
                        Start();
                    }
                    else
                    {
                        _settings["Toggle"] = false;
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

        public enum DifficultySelection
        {
            Easy, Medium, Hardcore
        }

        public static class RelevantItems
        {
            public static readonly int[] Kits = {
                297274, 293296, 291084, 291083, 291082
            };
        }

        public static class SpiritNanos
        {
            public const int BlessingofTheBlood = 280472; //Red
            public const int BlessingofTheSource = 280521; //Blue
            public const int BlessingofTheOutsider = 280493; //Green
            public const int BlessingofTheLight = 280496;  //Yellow
        }
    }
}