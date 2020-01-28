using System;
using ImGuiNET;

namespace VoxelNet.Menus
{
    public class TestMenu : Menu
    {
        public override void GUI()
        {
            if(ImGui.Button("Play"))
            {

            }

            if(ImGui.Button("Quit"))
            {
                Program.Window.Close();
            }
        }
    }
}
