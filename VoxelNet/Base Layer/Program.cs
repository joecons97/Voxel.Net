using System;
using VoxelNet;
using VoxelNet.Input;

namespace VoxelNet
{
    public static class Program
    {
        public const string PROGRAMTITLE = "Voxel.Net";

        public static Settings Settings;
        public static Window Window { get; private set; }

        [STAThread]
        static void Main()
        {
            LoadSettings();

            using (Window = new Window(Settings.WindowWidth, Settings.WindowHeight, PROGRAMTITLE))
            {
                if (Settings.FPS == -1)
                    Window.Run();
                else
                    Window.Run(60);
            }
        }

        static void LoadSettings()
        {
            //TODO Load from file
            Settings = new Settings()
            {
                FPS = 60,
                WindowWidth = 1280,
                WindowHeight = 720,
                FieldOfView = 65,
                Input = new InputSettings()
            };
        }
    }
}
