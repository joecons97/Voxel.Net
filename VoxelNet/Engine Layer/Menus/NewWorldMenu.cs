using System;
using System.Collections.Generic;
using OpenTK;
using VoxelNet.Assets;
using VoxelNet.Entities;

namespace VoxelNet.Menus
{
    public class NewWorldMenu : Menu
    {
        private GUIStyle titleStyle = (GUIStyle)GUI.LabelStyle.Clone();

        public Menu PreviousMenu { get; set; }

        private string seed = "";

        public override void Show()
        {
            titleStyle.HorizontalAlignment = HorizontalAlignment.Middle;
            titleStyle.VerticalAlignment = VerticalAlignment.Middle;
            titleStyle.FontSize = 92;

            base.Show();
        }

        public override void OnGUI()
        {
            GUI.Label("NEW WORLD", new Rect(0, 256f / (1280f / Program.Window.Height), Program.Window.Width, 72), titleStyle);

            GUI.Textbox(ref seed,"World Seed", 100,
                new Rect(Program.Window.Width / 2f - 198, Program.Window.Height / 2f - 27, 198 * 2, 18 * 2));

            if (GUI.Button("Back", new Rect(Program.Window.Width / 2f - 198, Program.Window.Height / 2f + 81, 190, 18 * 2)))
            {
                if (PreviousMenu != null)
                    PreviousMenu.Show();

                Close();
            }

            if (GUI.Button("Create", new Rect(Program.Window.Width / 2f + 8, Program.Window.Height / 2f + 81, 190, 18 * 2)))
            {
                Debug.Log(seed.GetSeedNum().ToString());
                var wrld = new World("New World", seed);

                Player.SetMouseVisible(false);

                Close();
            }
        }
    }
}
