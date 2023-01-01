using System;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using AOSharp.Pathfinding;
using AOSharp.Core.IPC;
using CityBuddy.IPCMessages;
using AOSharp.Common.GameData.UI;

namespace CityBuddy
{
    public class CityBuddy : AOPluginEntry
    {
        public static StateMachine _stateMachine;
        public static NavMeshMovementController NavMeshMovementController { get; private set; }
        public static IPCChannel IPCChannel { get; private set; }

        public static Config Config { get; private set; }

        public static DateTime gameTime;
        public static DateTime cloakTime;
        public static DateTime endWave1;

        public static bool Running = false;

        public static bool UsedCru = false;

        public static Identity Leader = Identity.None;
        public static bool IsLeader = false;

        public static Vector3 DefendPos;
        public static Vector3 CityControllerPos;

        public static string PluginDirectory;

        public static Settings CityBuddySettings = new Settings("CityBuddy");

        public override void Run(string pluginDir)
        {
            try
            {
                PluginDirectory = pluginDir;

                Chat.WriteLine("CityBuddy Loaded!");
                Chat.WriteLine("/citybuddy for settings.");

                Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\CityBuddy\\{Game.ClientInst}\\Config.json");

                IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[Game.ClientInst].IPCChannel));

                _stateMachine = new StateMachine(new IdleState());

                CityBuddySettings.AddVariable("Running", false);

                CityBuddySettings["Running"] = false;

                SettingsController.RegisterSettingsWindow("CityBuddy", pluginDir + "\\UI\\CityBuddySettingWindow.xml", CityBuddySettings);

                Chat.RegisterCommand("buddy", CityBuddyCommand);

                IPCChannel.RegisterCallback((int)IPCOpcode.Start, OnStartMessage);
                IPCChannel.RegisterCallback((int)IPCOpcode.Stop, OnStopMessage);

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

        private void Start()
        {
            Running = true;

            if (!(_stateMachine.CurrentState is IdleState))
                _stateMachine.SetState(new IdleState());
        }

        private void Stop()
        {
            Running = false;

            _stateMachine.SetState(new IdleState());
        }

        private void OnStartMessage(int sender, IPCMessage msg)
        {
            StartMessage startMsgCity = (StartMessage)msg;

            CityBuddySettings["Running"] = true;

            Leader = new Identity(IdentityType.SimpleChar, sender);

            Start();
        }

        private void OnStopMessage(int sender, IPCMessage msg)
        {
            StopMessage stopMsgCity = (StopMessage)msg;

            CityBuddySettings["Running"] = false;

            Stop();
        }

        private void HelpBox(object s, ButtonBase button)
        {
            Window helpWindow = Window.CreateFromXml("Help", PluginDirectory + "\\UI\\CityBuddyHelpBox.xml",
            windowSize: new Rect(0, 0, 455, 345),
            windowStyle: WindowStyle.Default,
            windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);
            helpWindow.Show(true);
        }

        private void CcSetBox(object s, ButtonBase button)
        {
            CityControllerPos = DynelManager.LocalPlayer.Position;
            Chat.WriteLine($"City controller position set.");
        }


        private void DefendSetBox(object s, ButtonBase button)
        {
            DefendPos = DynelManager.LocalPlayer.Position;
            Chat.WriteLine($"Defend position set.");
        }


        private void OnUpdate(object s, float deltaTime)
        {
            if (Game.IsZoning)
                return;

            if (!CityBuddySettings["Running"].AsBool() && Running == true)
            {
                Stop();
                IPCChannel.Broadcast(new StopMessage());
                return;
            }
            if (CityBuddySettings["Running"].AsBool() && Running == false)
            {
                if (DefendPos == Vector3.Zero && CityControllerPos == Vector3.Zero) 
                {
                    Chat.WriteLine("Set your positions.");
                    CityBuddySettings["Running"] = false;
                    return;
                }
                if (CityControllerPos == Vector3.Zero)
                {
                    Chat.WriteLine("Set the city controller position.");
                    CityBuddySettings["Running"] = false;
                    return;
                }
                if (DefendPos == Vector3.Zero)
                {
                    Chat.WriteLine("Set your defend position.");
                    CityBuddySettings["Running"] = false;
                    return;
                }

                IsLeader = true;
                Leader = DynelManager.LocalPlayer.Identity;

                if (DynelManager.LocalPlayer.Identity == Leader)
                {
                    IPCChannel.Broadcast(new StartMessage());
                }
                Start();

                return;
            }

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelBox);

                if (channelBox != null && !string.IsNullOrEmpty(channelBox.Text))
                {
                    if (int.TryParse(channelBox.Text, out int channelValue))
                    {
                        if (Config.CharSettings[Game.ClientInst].IPCChannel != channelValue)
                        {
                            IPCChannel.SetChannelId(Convert.ToByte(channelValue));
                            Config.CharSettings[Game.ClientInst].IPCChannel = Convert.ToByte(channelValue);
                            Config.Save();
                        }
                    }
                }

                if (SettingsController.settingsView != null)
                {
                    if (SettingsController.settingsView.FindChild("CityBuddyHelpBox", out Button helpBox))
                    {
                        helpBox.Tag = SettingsController.settingsView;
                        helpBox.Clicked = HelpBox;
                    }

                    if (SettingsController.settingsView.FindChild("CityControllerSetBox", out Button ccBox))
                    {
                        ccBox.Tag = SettingsController.settingsView;
                        ccBox.Clicked = CcSetBox;
                    }

                    if (SettingsController.settingsView.FindChild("DefendPosSetBox", out Button defendBox))
                    {
                        defendBox.Tag = SettingsController.settingsView;
                        defendBox.Clicked = DefendSetBox;
                    }
                }
            }

            _stateMachine.Tick();
        }

        private void CityBuddyCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    if (!CityBuddySettings["Running"].AsBool() && !Running)
                    {
                        IsLeader = true;
                        Leader = DynelManager.LocalPlayer.Identity;

                        CityBuddySettings["Running"] = true;
                        Start();
                        Chat.WriteLine("Bot enabled.");
                        if (DynelManager.LocalPlayer.Identity == Leader)
                        {
                            IPCChannel.Broadcast(new StartMessage());
                        }
                    }
                    else if (CityBuddySettings["Running"].AsBool() && Running)
                    {
                        Stop();
                        Chat.WriteLine("Bot disabled.");
                        if (DynelManager.LocalPlayer.Identity == Leader)
                        {
                            IPCChannel.Broadcast(new StopMessage());
                        }
                    }
                    return;
                }
                Config.Save();
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
