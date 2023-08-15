using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;

namespace DefendBuddy
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
                Chat.RegisterCommand("defendbuddy", (string command, string[] param, ChatWindow chatWindow) =>
                {
                    try
                    {
                        Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\DefendBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");

                        settingsWindow = Window.Create(new Rect(50, 50, 300, 300), "DefendBuddy", "Settings", WindowStyle.Default, WindowFlags.AutoScale);

                        if (settingsWindow != null && !settingsWindow.IsVisible)
                        {
                            AppendSettingsTab("DefendBuddy", settingsWindow);

                            settingsWindow.FindView("ChannelBox", out TextInputView channelInput);
                            settingsWindow.FindView("AttackRangeBox", out TextInputView attackRangeInput);
                            settingsWindow.FindView("ScanRangeBox", out TextInputView scanRangeInput);

                            if (channelInput != null)
                                channelInput.Text = $"{Config.CharSettings[DynelManager.LocalPlayer.Name].IPCChannel}";
                            if (attackRangeInput != null)
                                attackRangeInput.Text = $"{Config.CharSettings[DynelManager.LocalPlayer.Name].AttackRange}";
                            if (scanRangeInput != null)
                                scanRangeInput.Text = $"{Config.CharSettings[DynelManager.LocalPlayer.Name].ScanRange}";
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
