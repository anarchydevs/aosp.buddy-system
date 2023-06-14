using AOSharp.Core;
using AOSharp.Core.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Db1Buddy
{
    public class Config
    {
        public Dictionary<int, CharacterSettings> CharSettings { get; set; }

        protected string _path;

        [JsonIgnore]
<<<<<<< HEAD
        public int IPCChannel => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].IPCChannel : 48;
=======
        public int IPCChannel => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].IPCChannel : 45;
>>>>>>> aab7ee3ccaa03c6ad6b10dee74da529f4148bb84

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

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\Db1Buddy"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\Db1Buddy");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\Db1Buddy\\{Game.ClientInst}"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\Db1Buddy\\{Game.ClientInst}");

            File.WriteAllText(_path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class CharacterSettings
    {
        public event EventHandler<int> IPCChannelChangedEvent;
<<<<<<< HEAD
        private int _ipcChannel = 48;
=======
        private int _ipcChannel = 45;
>>>>>>> aab7ee3ccaa03c6ad6b10dee74da529f4148bb84

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
    }
}
