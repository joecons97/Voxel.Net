using System;
using System.Windows.Forms;
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

            try
            {
                LoadSettings();
                using (Window = new Window(Settings.WindowWidth, Settings.WindowHeight, PROGRAMTITLE))
                {
                    Window.Run();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace, "Fatal Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void LoadSettings()
        {
            //TODO Load from file
            Settings = Settings.Load();
        }
    }
}
