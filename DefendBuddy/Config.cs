using AOSharp.Core;
using AOSharp.Core.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DefendBuddy
{
    public class Config
    {
        public Dictionary<string, CharacterSettings> CharSettings { get; set; }

        protected string _path;

        [JsonIgnore]
        public int IPCChannel => CharSettings != null && CharSettings.ContainsKey(DynelManager.LocalPlayer.Name) ? CharSettings[DynelManager.LocalPlayer.Name].IPCChannel : 43;
        [JsonIgnore]
        public int AttackRange => CharSettings != null && CharSettings.ContainsKey(DynelManager.LocalPlayer.Name) ? CharSettings[DynelManager.LocalPlayer.Name].AttackRange : 10;
        [JsonIgnore]
        public int AggroToolRange => CharSettings != null && CharSettings.ContainsKey(DynelManager.LocalPlayer.Name) ? CharSettings[DynelManager.LocalPlayer.Name].ScanRange : 11;
        public static Config Load(string path)
        {
            Config config;
            try
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));

                config._path = path;
            }
            catch
            {
                Chat.WriteLine($"No config file found.");
                Chat.WriteLine($"Using default settings");

                config = new Config
                {
                    CharSettings = new Dictionary<string, CharacterSettings>()
                    {
                        { DynelManager.LocalPlayer.Name, new CharacterSettings() }
                    }
                };

                config._path = path;

                config.Save();
            }

            return config;
        }

        public void Save()
        {
            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\DefendBuddy"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\DefendBuddy");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\DefendBuddy\\{DynelManager.LocalPlayer.Name}"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\DefendBuddy\\{DynelManager.LocalPlayer.Name}");

            File.WriteAllText(_path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class CharacterSettings
    {
        public event EventHandler<int> IPCChannelChangedEvent;
        private int _ipcChannel = 43;

        //Breaking out auto-property
        public int IPCChannel
        {
            get
            {
                return _ipcChannel;
            }
            set
            {
                if (_ipcChannel != value)
                {
                    _ipcChannel = value;
                    IPCChannelChangedEvent?.Invoke(this, value);
                }
            }
        }

        public event EventHandler<int> AttackRangeChangedEvent;
        private int _attackRange = 10;

        //Breaking out auto-property
        public int AttackRange
        {
            get
            {
                return _attackRange;
            }
            set
            {
                if (_attackRange != value)
                {
                    _attackRange = value;
                    AttackRangeChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> ScanRangeChangedEvent;
        private int _aggroToolRange = 11;

        //Breaking out auto-property
        public int ScanRange
        {
            get
            {
                return _aggroToolRange;
            }
            set
            {
                if (_aggroToolRange != value)
                {
                    _aggroToolRange = value;
                    ScanRangeChangedEvent?.Invoke(this, value);
                }
            }
        }
    }
}
