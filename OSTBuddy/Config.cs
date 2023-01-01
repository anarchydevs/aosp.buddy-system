using System;
using System.Collections.Generic;
using System.IO;
using AOSharp.Core;
using Newtonsoft.Json;
using AOSharp.Core.UI;

namespace OSTBuddy
{
    public class Config
    {
        public Dictionary<int, CharacterSettings> CharSettings { get; set; }

        protected string _path;

        [JsonIgnore]
        public int RespawnDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].RespawnDelay : 1;

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
                    //GlobalSettings = new GlobalSettings(),
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
            if(!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\OSTBuddy"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\OSTBuddy");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\OSTBuddy\\{Game.ClientInst}"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\OSTBuddy\\{Game.ClientInst}");

            File.WriteAllText(_path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class CharacterSettings
    {
        public event EventHandler<int> RespawnDelayChangedEvent;
        private int _respawnDelay = 1;

        //Breaking out auto-property
        public int RespawnDelay
        {
            get
            {
                return _respawnDelay;
            }
            set
            {
                if (_respawnDelay != value)
                {
                    _respawnDelay = value;
                    RespawnDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
    }
}
