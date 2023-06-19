using System;
using System.Collections.Generic;
using System.IO;
using AOSharp.Core;
using Newtonsoft.Json;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;

namespace AXPBuddy
{
    public class Config
    {
        public Dictionary<int, CharacterSettings> CharSettings { get; set; }

        protected string _path;

        [JsonIgnore]
        public int IPCChannel => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].IPCChannel : 0;
        [JsonIgnore]
        public float Tick => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].Tick : 0.7f;
        [JsonIgnore]
        public string Leader => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].Leader : string.Empty;

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
                    CharSettings = new Dictionary<int, CharacterSettings>()
                    {
                        { Game.ClientInst, new CharacterSettings() }
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

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\AXPBuddy"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\AXPBuddy");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\AXPBuddy\\{Game.ClientInst}"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\KnowsMods\\AXPBuddy\\{Game.ClientInst}");

            File.WriteAllText(_path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class CharacterSettings
    {
        public event EventHandler<int> IPCChannelChangedEvent;
        private int _ipcChannel = 0;

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
        public event EventHandler<float> TickChangedEvent;
        private float _tick = 0.7f;

        public float Tick
        {
            get
            {
                return _tick;
            }
            set
            {
                if (_tick != value)
                {
                    _tick = value;
                    TickChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<string> LeaderChangedEvent;
        private string _leader = string.Empty;

        //Breaking out auto-property
        public string Leader
        {
            get
            {
                return _leader;
            }
            set
            {
                if (_leader != value)
                {
                    _leader = value;
                    LeaderChangedEvent?.Invoke(this, value);
                }
            }
        }
    }
}
