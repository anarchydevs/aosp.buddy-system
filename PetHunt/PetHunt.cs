using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.UI;
using System;
using PetHunt.IPCMessages;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace PetHunt
{
    public class PetHunt : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static IPCChannel IPCChannel { get; private set; }
        public static Config Config { get; private set; }

        private static Window _infoWindow;

        public static Settings _settings;

        public static string PluginDir;

        public static string previousErrorMessage = string.Empty;

        public static int HuntRange;

        public static bool Enable = false;

        public static List<SimpleChar> _mob = new List<SimpleChar>();
        public static List<SimpleChar> _bossMob = new List<SimpleChar>();
        public static List<SimpleChar> _switchMob = new List<SimpleChar>();

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("PetHunt");

                PluginDir = pluginDir;

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\PetHunt\\{DynelManager.LocalPlayer.Name}\\Config.json");

                IPCChannel = new IPCChannel(Convert.ToByte(Config.IPCChannel));

                Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannelChangedEvent += IPCChannel_Changed;
                Config.CharSettings[DynelManager.LocalPlayer.Name].HuntRangeChangedEvent += HuntRange_Changed;

                IPCChannel.RegisterCallback((int)IPCOpcode.StartStop, OnStartStopMessage);

                Chat.RegisterCommand("buddy", BuddyCommand);

                SettingsController.RegisterSettingsWindow("PetHunt", pluginDir + "\\UI\\PetHuntSettingWindow.xml", _settings);

                _stateMachine = new StateMachine(new IdleState());

                Game.OnUpdate += OnUpdate;

                _settings.AddVariable("Enable", false);
                _settings["Enable"] = false;

                Chat.WriteLine("PetHunt Loaded!");
                Chat.WriteLine("/pethunt for settings.");

                HuntRange = Config.CharSettings[DynelManager.LocalPlayer.Name].HuntRange;

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

        public Window[] _windows => new Window[] {};

        public static void IPCChannel_Changed(object s, int e)
        {
            IPCChannel.SetChannelId(Convert.ToByte(e));
            Config.Save();
        }
        public static void HuntRange_Changed(object s, int e)
        {
            Config.CharSettings[DynelManager.LocalPlayer.Name].HuntRange = e;
            HuntRange = e;
            Config.Save();
        }
        private void Start()
        {
            Enable = true;

            Chat.WriteLine("PetHunt enabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Enable = false;

            Chat.WriteLine("PetHunt disabled.");

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());

        }

        private void OnStartStopMessage(int sender, IPCMessage msg)
        {
            if (msg is StartStopIPCMessage startStopMessage)
            {
                if (startStopMessage.IsStarting)
                {
                    _settings["Enable"] = true;
                    Start();
                }
                else
                {
                    _settings["Enable"] = false;
                    Stop();
                }
            }
        }

        private void HandleInfoViewClick(object s, ButtonBase button)
        {
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\PetHuntInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }

        private void OnUpdate(object s, float deltaTime)
        {
            try
            {
                if (Game.IsZoning)
                {
                    Enable = false;
                    _settings["Enable"] = false;

                    return;
                }

                if (_settings["Enable"].AsBool()) 
                {
                    ScanningDefault();
                    _stateMachine.Tick();
                }

                #region UI

                var window = SettingsController.FindValidWindow(_windows);

                    if (window != null && window.IsValid)
                    {
                        
                    }

                if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
                {
                    SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);
                    SettingsController.settingsWindow.FindView("HuntRangeBox", out TextInputView huntRangeInput);

                    if (channelInput != null)
                    {
                        if (int.TryParse(channelInput.Text, out int channelValue)
                            && Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel != channelValue)
                        {
                            Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel = channelValue;
                        }
                    }

                    //bool huntRangeChanged = false;

                    if (channelInput != null)
                    {
                        if (int.TryParse(huntRangeInput.Text, out int huntRangeInputValue)
                            && Config.CharSettings[DynelManager.LocalPlayer.Name].HuntRange != huntRangeInputValue)
                        {
                            Config.CharSettings[DynelManager.LocalPlayer.Name].HuntRange = huntRangeInputValue;
                            //  huntRangeChanged = true;
                        }
                    }
                    //if (huntRangeChanged)
                    //{
                        //IPCChannel.Broadcast(new RangeInfoIPCMessage()
                        //{
                        //    HuntRange = Config.CharSettings[DynelManager.LocalPlayer.Name].huntRange,
                        //});
                    //}

                    if (SettingsController.settingsWindow.FindView("PetHuntInfoView", out Button infoView))
                        {
                            infoView.Tag = SettingsController.settingsWindow;
                            infoView.Clicked = HandleInfoViewClick;
                        }

                        if (!_settings["Enable"].AsBool() && Enable)
                        {
                            IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
                            Stop();
                        }
                        if (_settings["Enable"].AsBool() && !Enable)
                        {
                            IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = true });
                            Start();
                        }
                    }

                    #endregion

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

        private void ScanningDefault()
        {
            var localPlayer = DynelManager.LocalPlayer;
            var player = DynelManager.Players;

            var pets = DynelManager.LocalPlayer.Pets;

            _bossMob = DynelManager.NPCs
                       .Where(c => c.DistanceFrom(localPlayer) <= HuntRange
                           && !Ignores._ignores.Contains(c.Name) && !c.IsPlayer
                           && c.Health > 0 && !c.IsPet
                           && !c.Buffs.Contains(253953) && !c.Buffs.Contains(205607)
                           && c.MaxHealth >= 1000000)
                       .OrderBy(c => c.Position.DistanceFrom(localPlayer.Position))
                       .OrderByDescending(c => c.Name == "Field Support  - Cha'Khaz")
                       .OrderByDescending(c => c.Name == "Ground Chief Aune")


                       .ToList();

            _switchMob = DynelManager.NPCs
               .Where(c => c.DistanceFrom(localPlayer) <= HuntRange
                   && !Ignores._ignores.Contains(c.Name) && !c.IsPlayer
                   && c.Name != "Zix" && !c.Name.Contains("sapling")
                   && c.Health > 0 && c.MaxHealth < 1000000 && !c.IsPet
                   && (c.Name == "Hand of the Colonel"
                  || c.Name == "Hacker'Uri"
                  || c.Name == "The Sacrifice"
                  || c.Name == "Drone Harvester - Jaax'Sinuh"
                  || c.Name == "Support Sentry - Ilari'Uri"
                  || c.Name == "Alien Coccoon"
                  || c.Name == "Alien Cocoon"
                  || c.Name == "Stasis Containment Field"))
               .OrderBy(c => c.Position.DistanceFrom(localPlayer.Position))
               .OrderBy(c => c.HealthPercent)
               .OrderByDescending(c => c.Name == "Drone Harvester - Jaax'Sinuh")
               .OrderByDescending(c => c.Name == "Lost Thought")
               .OrderByDescending(c => c.Name == "Support Sentry - Ilari'Uri")
               .OrderByDescending(c => c.Name == "Alien Cocoon")
               .OrderByDescending(c => c.Name == "Alien Coccoon" && c.MaxHealth < 40001)
               .ToList();

            _mob = DynelManager.Characters
                .Where(c => c.DistanceFrom(localPlayer) <= HuntRange
                    && !Ignores._ignores.Contains(c.Name) && !c.IsPlayer
                    && c.Name != "Zix" && !c.Name.Contains("sapling") && c.Health > 0
                    && c.MaxHealth < 1000000 && !c.IsPet
                    && (!c.IsPet || c.Name == "Drop Trooper - Ilari'Ra"))
                .OrderBy(c => c.Position.DistanceFrom(localPlayer.Position))
                .OrderBy(c => c.HealthPercent)
                .OrderByDescending(c => c.Name == "Drone Harvester - Jaax'Sinuh")
                .OrderByDescending(c => c.Name == "Support Sentry - Ilari'Uri")
                .OrderByDescending(c => c.Name == "Alien Cocoon")
                .OrderByDescending(c => c.Name == "Alien Coccoon" && c.MaxHealth < 40001)
                .OrderByDescending(c => c.Name == "Masked Operator")
                .OrderByDescending(c => c.Name == "Masked Technician")
                .OrderByDescending(c => c.Name == "Masked Engineer")
                .OrderByDescending(c => c.Name == "Masked Superior Commando")
                .OrderByDescending(c => c.Name == "The Sacrifice")
                .OrderByDescending(c => c.Name == "Hacker'Uri")
                .OrderByDescending(c => c.Name == "Hand of the Colonel")
                .OrderByDescending(c => c.Name == "Ground Chief Aune")
                .ToList();
        }

        private void BuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!_settings["Enable"].AsBool())
                    {
                        _settings["Enable"] = true;
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = true });
                        Start();
                    }
                    else
                    {
                        _settings["Enable"] = false;
                        IPCChannel.Broadcast(new StartStopIPCMessage() { IsStarting = false });
                        Stop();
                    }
                    return;
                }

                switch (param[0].ToLower())
                {
                    case "ignore":
                        if (param.Length > 1)
                        {
                            string name = string.Join(" ", param.Skip(1));

                            if (!Ignores._ignores.Contains(name))
                            {
                                Ignores._ignores.Add(name);
                                chatWindow.WriteLine($"Added \"{name}\" to ignored mob list");
                            }
                            else if (Ignores._ignores.Contains(name))
                            {
                                Ignores._ignores.Remove(name);
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
            catch (Exception ex)
            {
                var errorMessage = "An error occurred on line " + GetLineNumber(ex) + ": " + ex.Message;

                if (errorMessage != previousErrorMessage)
                {
                    chatWindow.WriteLine(errorMessage);
                    chatWindow.WriteLine("Stack Trace: " + ex.StackTrace);
                    previousErrorMessage = errorMessage;
                }
            }
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
