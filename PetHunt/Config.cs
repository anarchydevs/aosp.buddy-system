using AOSharp.Core;
using AOSharp.Core.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace PetHunt
{
    public class Config
    {
        public Dictionary<string, CharacterSettings> CharSettings { get; set; }

        protected string _path;

        [JsonIgnore]
        public int IPCChannel => CharSettings != null && CharSettings.ContainsKey(DynelManager.LocalPlayer.Name) ? CharSettings[DynelManager.LocalPlayer.Name].IPCChannel : 33;
        [JsonIgnore]
        public int HuntRange => CharSettings != null && CharSettings.ContainsKey(DynelManager.LocalPlayer.Name) ? CharSettings[DynelManager.LocalPlayer.Name].HuntRange : 60;

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
          
            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\PetHunt\\{DynelManager.LocalPlayer.Name}"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{CommonParameters.BasePath}\\{CommonParameters.AppPath}\\PetHunt\\{DynelManager.LocalPlayer.Name}");

            File.WriteAllText(_path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class CharacterSettings
    {
        public event EventHandler<int> IPCChannelChangedEvent;
        private int _ipcChannel = 33;

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

        public event EventHandler<int> HuntRangeChangedEvent;
        private int _huntRange = 60;

        public int HuntRange
        {
            get
            {
                return _huntRange;
            }
            set
            {
                if (_huntRange != value)
                {
                    _huntRange = value;
                    HuntRangeChangedEvent?.Invoke(this, value);
                }
            }
        }
    }
}
