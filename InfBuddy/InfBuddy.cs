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
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace InfBuddy
{

    public class InfBuddy : AOPluginEntry
    {
        private StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; set; }

        public static Config Config { get; private set; }

        private Stopwatch _kitTimer = new Stopwatch();
        private Stopwatch _sitTimer = new Stopwatch();

        public static bool Toggle = false;

        public static bool Ready = true;
        private Dictionary<Identity, bool> teamReadiness = new Dictionary<Identity, bool>();
        private bool? lastSentIsReadyState = null;

        ModeSelection currentMode;
        FactionSelection currentFaction;
        DifficultySelection currentDifficulty;

        public static SimpleChar _leader;
        public static Identity Leader = Identity.None;

        public static bool DoubleReward = false;

        public static double _stateTimeOut;
        private static double _uiDelay;

        private string previousErrorMessage = string.Empty;

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

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\InfBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");
                NavMeshMovementController = new NavMeshMovementController($"{pluginDir}\\NavMeshes", true);
                MovementController.Set(NavMeshMovementController);
                IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel));

                IPCChannel.RegisterCallback((int)IPCOpcode.StartStop, OnStartStopMessage);
                IPCChannel.RegisterCallback((short)IPCOpcode.ModeSelections, OnModeSelectionsMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.LeaderInfo, OnLeaderInfoMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.WaitAndReady, OnWaitAndReadyMessage);

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;

                Chat.RegisterCommand("buddy", BuddyCommand);

                SettingsController.RegisterSettingsWindow("InfBuddy", pluginDir + "\\UI\\InfBuddySettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                NpcDialog.AnswerListChanged += NpcDialog_AnswerListChanged;
                Team.TeamRequest += OnTeamRequest;
                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("ModeSelection", (int)ModeSelection.Normal);
                _settings.AddVariable("FactionSelection", (int)FactionSelection.Clan);
                _settings.AddVariable("DifficultySelection", (int)DifficultySelection.Hard);

                _settings.AddVariable("Toggle", false);

                _settings.AddVariable("DoubleReward", false);
                _settings.AddVariable("Merge", false);
                _settings.AddVariable("Looting", false);
                _settings.AddVariable("Leech", false);

                //_settings["Toggle"] = false;

                Chat.WriteLine("InfBuddy Loaded!");
                Chat.WriteLine("/infbuddy for settings.");
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

        private void OnStartStopMessage(int sender, IPCMessage msg)
        {
            if (msg is StartStopIPCMessage startStopMessage)
            {
                if (startStopMessage.IsStarting)
                {
                    // Only set the Leader if "Merge" is not checked.
                    if (!_settings["Merge"].AsBool())
                    {
                        Leader = new Identity(IdentityType.SimpleChar, sender);
                    }

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

        private void OnModeSelectionsMessage(int sender, IPCMessage msg)
        {
            if (msg is ModeSelectionsIPCMessage modeSelectionsMessage)
            {
                currentMode = modeSelectionsMessage.Mode;
                currentFaction = modeSelectionsMessage.Faction;
                currentDifficulty = modeSelectionsMessage.Difficulty;

                _settings["ModeSelection"] = (int)currentMode;
                _settings["FactionSelection"] = (int)currentFaction;
                _settings["DifficultySelection"] = (int)currentDifficulty;

                //Chat.WriteLine($"Received Mode: {currentMode}, Faction: {currentFaction}, Difficulty: {currentDifficulty}");
            }
        }
        private void OnLeaderInfoMessage(int sender, IPCMessage msg)
        {
            if (msg is LeaderInfoIPCMessage leaderInfoMessage)
            {
                if (leaderInfoMessage.IsRequest)
                {
                    if (Leader != Identity.None)
                    {
                        IPCChannel.Broadcast(new LeaderInfoIPCMessage() { LeaderIdentity = Leader, IsRequest = false });
                    }
                }
                else
                {
                    Leader = leaderInfoMessage.LeaderIdentity;
                }
            }
        }
        private void OnWaitAndReadyMessage(int sender, IPCMessage msg)
        {

            if (msg is WaitAndReadyIPCMessage waitAndReadyMessage)
            {
                Identity senderIdentity = waitAndReadyMessage.PlayerIdentity; // Get the Identity from the IPCMessage

                teamReadiness[senderIdentity] = waitAndReadyMessage.IsReady;

                //Chat.WriteLine($"IPC received. Sender: {senderIdentity}, IsReady: {waitAndReadyMessage.IsReady}"); // Debugging line added

                bool allReady = true;

                // Check team members against the readiness dictionary
                foreach (var teamMember in Team.Members)
                {
                    if (teamReadiness.ContainsKey(teamMember.Identity) && !teamReadiness[teamMember.Identity])
                    {
                        allReady = false;
                        break;
                    }
                }

                if (Leader == DynelManager.LocalPlayer.Identity)
                {
                    Ready = allReady;

                }
            }
        }
        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning) { return; }

            _stateMachine.Tick();

            Shared.Kits kitsInstance = new Shared.Kits();

            kitsInstance.SitAndUseKit();

            if (Leader == Identity.None)
            {
                if (_settings["Merge"].AsBool())
                {
                    SimpleChar teamLeader = Team.Members.FirstOrDefault(member => member.IsLeader)?.Character;

                    Leader = teamLeader?.Identity ?? Identity.None;
                }
                else
                {
                    IPCChannel.Broadcast(new LeaderInfoIPCMessage() { IsRequest = true });
                }
            }

            if (DynelManager.LocalPlayer.Identity != Leader)
            {
                var localPlayer = DynelManager.LocalPlayer;
                bool currentIsReadyState = true;

                // Check if Nano or Health is below 66% and not in combat
                if (!InCombat())
                {
                    if (Spell.HasPendingCast || localPlayer.NanoPercent < 66 || localPlayer.HealthPercent < 66
                        || !Spell.List.Any(spell => spell.IsReady))
                    {
                        currentIsReadyState = false;
                    }
                }

                // Check if Nano and Health are above 66%
                else if (!Spell.HasPendingCast && localPlayer.NanoPercent > 70
                    && localPlayer.HealthPercent > 70 && Spell.List.Any(spell => spell.IsReady))
                {
                    currentIsReadyState = true;
                }

                // Only send a message if the state has changed.
                if (currentIsReadyState != lastSentIsReadyState)
                {
                    Identity localPlayerIdentity = DynelManager.LocalPlayer.Identity;
                    //Chat.WriteLine($"Broadcasting IPC. Local player identity: {localPlayerIdentity}"); // Debugging line added

                    IPCChannel.Broadcast(new WaitAndReadyIPCMessage
                    {
                        IsReady = currentIsReadyState,
                        PlayerIdentity = localPlayerIdentity
                    });
                    lastSentIsReadyState = currentIsReadyState; // Update the last sent state
                }
            }

            #region UI

            if (Time.NormalTime > _uiDelay + 1.0)
            {
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

                    if (SettingsController.settingsWindow.FindView("InfBuddyInfoView", out Button infoView))
                    {
                        infoView.Tag = SettingsController.settingsWindow;
                        infoView.Clicked = InfoView;
                    }

                    if (!_settings["Toggle"].AsBool() && Toggle)
                    {
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
                        Stop();
                    }
                    if (_settings["Toggle"].AsBool() && !Toggle)
                    {
                        if (!_settings["Merge"].AsBool())
                            Leader = DynelManager.LocalPlayer.Identity;

                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = true });
                        Start();
                    }

                    ModeSelection newMode = (ModeSelection)_settings["ModeSelection"].AsInt32();
                    FactionSelection newFaction = (FactionSelection)_settings["FactionSelection"].AsInt32();
                    DifficultySelection newDifficulty = (DifficultySelection)_settings["DifficultySelection"].AsInt32();

                    bool modeChanged = newMode != currentMode;
                    bool factionChanged = newFaction != currentFaction;
                    bool difficultyChanged = newDifficulty != currentDifficulty;

                    if (modeChanged || factionChanged || difficultyChanged)
                    {
                        // Populate a ModeSelectionsIPCMessage
                        ModeSelectionsIPCMessage modeSelectionsMessage = new ModeSelectionsIPCMessage
                        {
                            Mode = newMode,
                            Faction = newFaction,
                            Difficulty = newDifficulty
                        };

                        // Broadcast the message
                        IPCChannel.Broadcast(modeSelectionsMessage);

                        // Update the current settings
                        if (modeChanged)
                        {
                            currentMode = newMode;
                        }

                        if (factionChanged)
                        {
                            currentFaction = newFaction;
                        }

                        if (difficultyChanged)
                        {
                            currentDifficulty = newDifficulty;
                        }
                    }
                    _uiDelay = Time.NormalTime;
                }

                #endregion
            }
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

        private void OnTeamRequest(object sender, TeamRequestEventArgs e)
        {
            // Set the leader to the sender of the team request
            Leader = e.Requester;
        }

        private void BuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    bool currentToggle = _settings["Toggle"].AsBool();
                    if (!currentToggle)
                    {
                        Leader = DynelManager.LocalPlayer.Identity;
                        _settings["Toggle"] = true;
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = true });
                        Start();
                    }
                    else
                    {
                        _settings["Toggle"] = false;
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
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
            Normal,
            Roam
        }
        public enum FactionSelection
        {
            Neutral, 
            Clan, 
            Omni
        }
        public enum DifficultySelection
        {
            Easy, 
            Medium, 
            Hard
        }

        public static class RelevantItems
        {
            public static readonly int[] Kits = {
                297274, 293296, 291084, 291083, 291082
            };
        }

        public int GetLineNumber(Exception ex)
        {
            var lineNumber = 0;

            var lineMatch = Regex.Match(ex.StackTrace ?? "", @":line (\d+)$", RegexOptions.Multiline);

            if (lineMatch.Success)
                lineNumber = int.Parse(lineMatch.Groups[1].Value);

            return lineNumber;
        }
    }
}
