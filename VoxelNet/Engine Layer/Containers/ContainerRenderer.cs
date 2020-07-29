using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using VoxelNet.Rendering;

namespace VoxelNet.Containers
{
    public static class ContainerRenderer
    {
        static List<Container> toDraw = new List<Container>();

        public const float SLOT_SIZE = 48;

        public static Texture ContainerBackground { get; private set; }
        public static Texture ContainerSlot { get; private set; }

        public static ItemStack SelectedStack { get; set; }

        public static GUIStyle SlotNumberStyle { get; private set; }

        static ContainerRenderer()
        {
            ContainerBackground = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/inventory_bg.png");
            ContainerSlot = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/inventory_slot.png");
            SlotNumberStyle = new GUIStyle()
            {
                Font = GUI.ButtonStyle.Font,
                FontSize = 40,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                AlignmentOffset = new Vector2(8,0),
                Normal = new GUIStyleOption()
                {
                    TextColor = Color4.White
                }
            };
            Input.Input.GetSetting("Destroy Block").KeyUp += () =>
            {
                if (SelectedStack != null)
                {
                    for (int i = 0; i < toDraw.Count; i++)
                    {
                        if (toDraw[i].SelectedSlot != new Vector2(-1, -1) && toDraw[i].GetIsSlotFree(toDraw[i].SelectedSlot))
                        {
                            SelectedStack.LocationInContainer = toDraw[i].SelectedSlot;
                            toDraw[i].ItemsList.Add(SelectedStack);
                            SelectedStack = null;
                            return;
                        }
                    }

                    if (SelectedStack.PreviousParent != null)
                    {
                        SelectedStack.PreviousParent.ItemsList.Add(SelectedStack);
                        SelectedStack = null;
                    }
                }
            };
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

            if (SelectedStack != null)
            {
                var rect = new Rect(GUI.MousePosition.X, GUI.MousePosition.Y, SLOT_SIZE-8, SLOT_SIZE-8);
                if (GUI.HoldButton(SelectedStack.Item.Icon, rect))
                {
                    
                }
                GUI.Label(SelectedStack.StackSize.ToString(), rect, SlotNumberStyle);
            }
        }
    }
}
