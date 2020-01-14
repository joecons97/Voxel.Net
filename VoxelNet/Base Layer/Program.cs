using System;
using VoxelNet;

namespace VoxelNet
{
    public static class Program
    {
        public const string PROGRAMTITLE = "Voxel.Net";

        public static Settings Settings;

        [STAThread]
        static void Main()
        {
            LoadSettings();

            using (Window window = new Window(Settings.WindowWidth, Settings.WindowHeight, PROGRAMTITLE))
            {
                if (Settings.FPS == -1)
                    window.Run();
                else
                    window.Run(60);
            }
        }

        static void LoadSettings()
        {
            //TODO Load from file
            Settings = new Settings()
            {
                FPS = 60,
                WindowWidth = 1280,
                WindowHeight = 720
            };
        }
    }
}
