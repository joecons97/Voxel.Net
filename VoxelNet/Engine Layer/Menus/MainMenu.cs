using System;
using VoxelNet.Assets;
using VoxelNet.Entities;
using VoxelNet.Properties;
using VoxelNet.Rendering;

namespace VoxelNet.Menus
{
    public class MainMenu : Menu
    {
        private GUIStyle titleStyle = (GUIStyle)GUI.LabelStyle.Clone();

        OptionsMenu options = new OptionsMenu();

        private Texture tex;

        public override void Show()
        {
            Player.SetMouseVisible(true);

            titleStyle.HorizontalAlignment = HorizontalAlignment.Middle;
            titleStyle.VerticalAlignment = VerticalAlignment.Middle;
            titleStyle.FontSize = 92;

            options.PreviousMenu = this;

            tex = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/inventory_bg.png");

            base.Show();
        }

        public override void OnGUI()
        {
            float winWidth = Program.Window.Width;
            float winHeight = Program.Window.Height;

            GUI.Label("VOXEL .NET", new Rect(0, 256f / (1280f/winHeight), winWidth, 72), titleStyle);

            if (GUI.Button("New World", new Rect(winWidth / 2f - 198, winHeight / 2f - 81, 198 * 2, 18 * 2)))
            {
                var newWorld = new NewWorldMenu();
                newWorld.PreviousMenu = this;
                newWorld.Show();
                Close();
            }

            if (GUI.Button("Load World", new Rect(winWidth / 2f - 198, winHeight / 2f - 27, 198 * 2, 18 * 2)))
            {
                var wrld = new World("New World", new Random().Next(1,999999).ToString());
                Close();
                Player.SetMouseVisible(false);
            }

            if (GUI.Button("Options", new Rect(winWidth / 2f - 198, winHeight / 2f + 27, 198 * 2, 18 * 2)))
            {
                options.Show();
                Close();
            }

            if (GUI.Button("Quit", new Rect(winWidth / 2f - 198, winHeight / 2f + 81, 198 * 2, 18 * 2)))
            {
                Program.Window.Close();
            }
        }
    }
}
