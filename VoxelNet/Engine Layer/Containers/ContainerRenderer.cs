using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelNet.Rendering;

namespace VoxelNet.Containers
{
    public static class ContainerRenderer
    {
        static List<Container> toDraw = new List<Container>();

        public static Texture ContainerBackground { get; private set; }
        public static Texture ContainerSlot { get; private set; }

        static ContainerRenderer()
        {
            ContainerBackground = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/inventory_bg.png");
            ContainerSlot = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/inventory_slot.png");
        }

        public static void OpenContainer(Container container)
        {
            if (!toDraw.Contains(container))
            {
                toDraw.Add(container);
                container.IsOpen = true;
            }
        }

        public static void CloseContainer(Container container)
        {
            if (toDraw.Contains(container))
            {
                toDraw.Remove(container);
                container.IsOpen = false;
            }
        }

        public static void RenderGUI()
        {
            for (int i = 0; i < toDraw.Count; i++)
            {
                toDraw[i].RenderGUI();
            }
        }
    }
}
