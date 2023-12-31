﻿using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;

namespace AttackBuddy
{
    public class WindowOptions
    {
        public string Name { get; set; }
        public string XmlViewName { get; set; }
        public Rect WindowSize { get; set; } = new Rect(0, 0, 240, 345);
        public WindowStyle Style { get; set; } = WindowStyle.Default;
        public WindowFlags Flags { get; set; } = WindowFlags.AutoScale | WindowFlags.NoFade;
    }
    public static class SettingsController
    {
        private static List<Settings> settingsToSave = new List<Settings>();
        public static Dictionary<string, string> settingsWindows = new Dictionary<string, string>();
        private static bool IsCommandRegistered;

        public static Window settingsWindow;
        public static View settingsView;

        public static string _staticName = string.Empty;

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
                Chat.RegisterCommand("attackbuddy", (string command, string[] param, ChatWindow Chat) =>
                {
                    try
                    {
                        Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\AttackBuddy\\{DynelManager.LocalPlayer.Name}\\Config.json");

                        settingsWindow = Window.Create(new Rect(50, 50, 300, 300), "AttackBuddy", "Settings", WindowStyle.Default, WindowFlags.AutoScale);

                        foreach (string settingsName in settingsWindows.Keys)
                        {
                            AppendSettingsTab(settingsName, settingsWindow);

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
                    catch (Exception ex)
                    {
                        var errorMessage = "An error occurred on line " + AttackBuddy.GetLineNumber(ex) + ": " + ex.Message;

                        if (errorMessage != AttackBuddy.previousErrorMessage)
                        {
                            Chat.WriteLine(errorMessage);
                            Chat.WriteLine("Stack Trace: " + ex.StackTrace);
                            AttackBuddy.previousErrorMessage = errorMessage;
                        }
                    }
                });

                IsCommandRegistered = true;
            }
        }

        public static Window FindValidWindow(Window[] allWindows)
        {
            foreach (var window in allWindows)
            {
                if (window?.IsValid == true)
                    return window;
            }

            return null;
        }

        public static void AppendSettingsTab(Window windowToCreate, WindowOptions options, View view)
        {
            if (windowToCreate != null && windowToCreate.IsValid)
            {
                if (!string.IsNullOrEmpty(_staticName) && options.Name != _staticName && !windowToCreate.Views.Contains(view))
                {
                    windowToCreate.AppendTab(options.Name, view);
                }
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

        public static void CreateSettingsTab(Window windowToCreate, string PluginDir, WindowOptions options, View view, out Window container)
        {
            windowToCreate = Window.CreateFromXml(options.Name, $@"{PluginDir}\UI\{options.XmlViewName}.xml",
                windowSize: options.WindowSize,
                windowStyle: options.Style,
                windowFlags: options.Flags);

            _staticName = options.Name;

            windowToCreate.Show(true);
            container = windowToCreate;
        }
    }
}
