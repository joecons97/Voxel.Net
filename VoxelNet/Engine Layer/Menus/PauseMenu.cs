using System;
using VoxelNet.Assets;
using VoxelNet.Entities;
using VoxelNet.Properties;
using VoxelNet.Rendering;

namespace VoxelNet.Menus
{
    public class PauseMenu : Menu
    {
        private GUIStyle titleStyle = (GUIStyle)GUI.LabelStyle.Clone();

        OptionsMenu options = new OptionsMenu();

        public override void Show()
        {
            options.PreviousMenu = this;
            options.Close();

            Player.SetMouseVisible(true);
            Player.SetControlsActive(false);

            titleStyle.HorizontalAlignment = HorizontalAlignment.Middle;
            titleStyle.VerticalAlignment = VerticalAlignment.Middle;
            titleStyle.FontSize = 92;

            base.Show();
        }

        public override void OnGUI()
        {
            float winWidth = Program.Window.Width;
            float winHeight = Program.Window.Height;

            if (GUI.Button("Resume", new Rect(winWidth / 2f - 198, winHeight / 2f - 72, 198 * 2, 18 * 2)))
            {
                Close();
                Player.SetMouseVisible(false);
                Player.SetControlsActive(true);

            }

            if (GUI.Button("Options", new Rect(winWidth / 2f - 198, winHeight / 2f - 18, 198 * 2, 36)))
            {
                this.Close();
                options.Show();
            }

            if (GUI.Button("Quit", new Rect(winWidth / 2f - 198, winHeight / 2f + 36, 198 * 2, 18 * 2)))
            {
                Close();
                World.GetInstance().Dispose();
                Program.Window.MainMenu.Show();
            }
        }
    }
}
