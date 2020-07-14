using System;
using VoxelNet.Assets;
using VoxelNet.Entities;

namespace VoxelNet.Menus
{
    public class TestMenu : Menu
    {
        private GUIStyle titleStyle = (GUIStyle)GUI.LabelStyle.Clone();

        OptionsMenu options = new OptionsMenu();

        public override void Show()
        {
            Player.SetMouseVisible(true);

            titleStyle.HorizontalAlignment = HorizontalAlignment.Middle;
            titleStyle.VerticalAlignment = VerticalAlignment.Top;
            titleStyle.FontSize = 48;

            options.PreviousMenu = this;

            base.Show();
        }

        public override void OnGUI()
        {
            GUI.Label("VOXEL .NET", new Rect(0, 256f / (1280f/Program.Window.Height), Program.Window.Width, 72), titleStyle);

            if (GUI.Button("New World", new Rect(Program.Window.Width / 2f - 198, Program.Window.Height / 2f - 27, 198 * 2, 18 * 2)))
            {
                var wrld = new World("New World", new Random().Next(1,999999).ToString());
                Close();
                Player.SetMouseVisible(false);
            }

            if (GUI.Button("Options", new Rect(Program.Window.Width / 2f - 198, Program.Window.Height / 2f + 27, 198 * 2, 18 * 2)))
            {
                options.Show();
                Close();
            }

            if (GUI.Button("Quit", new Rect(Program.Window.Width / 2f - 198, Program.Window.Height / 2f + 81, 198 * 2, 18 * 2)))
            {
                Program.Window.Close();
            }
        }
    }
}
