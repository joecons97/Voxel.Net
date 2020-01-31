using System;
using ImGuiNET;
using VoxelNet.Assets;

namespace VoxelNet.Menus
{
    public class TestMenu : Menu
    {
        public override void GUI()
        {
            if(ImGui.Button("Play"))
            {
                var wrld = new World("New World", "New World");
                //Instantiate world...?
            }

            if(ImGui.Button("Quit"))
            {
                //Does this actually end the program process?
                Program.Window.Close();
            }
        }
    }
}
