using AOSharp.Common.GameData;
using AOSharp.Core.UI;
using AOSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOSharp.Common.GameData.UI;

namespace AXPBuddy
{
    public static class SettingsController
    {
        private static List<Settings> settingsToSave = new List<Settings>();
        public static Dictionary<string, string> settingsWindows = new Dictionary<string, string>();
        private static bool IsCommandRegistered;

        public static Window settingsWindow;
        public static View settingsView;

        public static Config Config { get; private set; }

        public static void RegisterCharacters(Settings settings)
        {
            RegisterChatCommandIfNotRegistered();
            settingsToSave.Add(settings);
        }

        public static void RegisterSettingsWindow(string settingsName, string settingsWindowPath, Settings settings)
        {
            RegisterChatCommandIfNotRegistered();
            settingsWindows[settingsName] = settingsWindowPath;
            settingsToSave.Add(settings);
        }

        public static void RegisterSettings(Settings settings)
        {
            RegisterChatCommandIfNotRegistered();
            settingsToSave.Add(settings);
        }

        public static void CleanUp()
        {
            settingsToSave.ForEach(settings => settings.Save());
        }

        private static void RegisterChatCommandIfNotRegistered()
        {
            if (!IsCommandRegistered)
            {
                Chat.RegisterCommand("axpbuddy", (string command, string[] param, ChatWindow chatWindow) =>
                {
                    try
                    {
                        Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\AXPBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");

                        settingsWindow = Window.Create(new Rect(50, 50, 300, 300), "AXPBuddy", "Settings", WindowStyle.Default, WindowFlags.AutoScale);

                        if (settingsWindow.IsVisible) { return; }

                        foreach (string settingsName in settingsWindows.Keys)
                        {
                            AppendSettingsTab(settingsName, settingsWindow);

                            settingsWindow.FindView("ChannelBox", out TextInputView channelInput);
                            settingsWindow.FindView("LeaderBox", out TextInputView leaderInput);
                            settingsWindow.FindView("TickBox", out TextInputView tickInput);

                            if (channelInput != null)
                                channelInput.Text = $"{Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel}";
                            if (leaderInput != null)
                                leaderInput.Text = $"{Config.CharSettings[DynelManager.LocalPlayer.Name].Leader}";
                            if (tickInput != null)
                                tickInput.Text = $"{Config.CharSettings[DynelManager.LocalPlayer.Name].Tick}";
                        }
                    }
                    catch (Exception e)
                    {
                        Chat.WriteLine(e);
                    }
                });

                IsCommandRegistered = true;
            }
        }

        public static void AppendSettingsTab(String settingsName, Window testWindow)
        {
            String settingsWindowXmlPath = settingsWindows[settingsName];
            settingsView = View.CreateFromXml(settingsWindowXmlPath);
            if (settingsView != null)
            {
                testWindow.AppendTab(settingsName, settingsView);
                testWindow.Show(true);
            }
            else
            {
                Chat.WriteLine("Failed to load settings schema from " + settingsWindowXmlPath);
            }
        }
    }
}
