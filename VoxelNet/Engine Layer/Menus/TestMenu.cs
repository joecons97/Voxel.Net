using System;
using System.Numerics;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using VoxelNet.Assets;
using VoxelNet.Entities;

namespace VoxelNet.Menus
{
    public class TestMenu : Menu
    {
        public override void Show()
        {
            Player.SetMouseVisible(true);
            GL.ClearColor(0,0,0,1);
            base.Show();
        }

        public override void GUI()
        {
            ImGui.Begin("menu", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysAutoResize);
            ImGui.SetWindowPos("menu", new Vector2(Program.Window.Width/2f - 128f, (Program.Window.Height / 2f) - 64f));
            if(ImGui.Button("Play", new Vector2(256,64)))
            {
                var wrld = new World("New World", "Poo Poo");
                Close();
                Player.SetMouseVisible(false);
                //Instantiate world...?
            }

            if(ImGui.Button("Quit", new Vector2(256, 64)))
            {
                //Does this actually end the program process?
                Program.Window.Close();
            }
            ImGui.End();
        }
    }
}
