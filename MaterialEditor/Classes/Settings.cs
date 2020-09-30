using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MaterialEditor
{
    public class Settings
    {
        private static string SaveLocation =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Blue Eyes", "Voxel.Net",
                "Material Editor", "Prefs.json");

        private static Settings _instance;
        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                    Load();

                return _instance;
            }
        }

        public string ProjectLocation { get; set; }

        static void Load()
        {
            if (!File.Exists(SaveLocation))
            {
                _instance = new Settings();
                return;
            }

            var json = File.ReadAllText(SaveLocation);
            _instance = JsonConvert.DeserializeObject<Settings>(json);
        }

        public static void Save()
        {
            if (!File.Exists(SaveLocation))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SaveLocation));
            }

            var json = JsonConvert.SerializeObject(_instance);
            File.WriteAllText(SaveLocation, json);
        }
    }
}
