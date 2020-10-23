using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using VoxelNet.Rendering;

namespace VoxelNet.Containers
{
    public static class ContainerRenderer
    {
        static List<Container> toDraw = new List<Container>();

        public const float SLOT_SIZE = 48;

        public static Texture ContainerBackground { get; private set; }
        public static Texture ContainerSlot { get; private set; }

        public static ItemStack StackBlockedForSelection { get; set; }

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
            Input.Input.MouseDown += (sender, args) =>
            {
                if (args.Button == MouseButton.Left)
                {
                    if (SelectedStack != null)
                    {
                        for (int i = 0; i < toDraw.Count; i++)
                        {
                            if (toDraw[i].SelectedSlot == new Vector2(-1, -1))
                            {
                                continue;
                            }

                            if (toDraw[i].GetIsSlotFree(toDraw[i].SelectedSlot))
                            {
                                SelectedStack.LocationInContainer = toDraw[i].SelectedSlot;
                                toDraw[i].ItemsList.Add(SelectedStack);
                                toDraw[i].ItemDroppedIntoContainer(SelectedStack);
                                StackBlockedForSelection = SelectedStack;
                                SelectedStack = null;
                                return;
                            }
                            else
                            {
                                var stackAtLocation = toDraw[i].GetItemStackByLocation(toDraw[i].SelectedSlot);
                                if (stackAtLocation.ItemKey == SelectedStack.ItemKey)
                                {
                                    int possibleNewSize =
                                        (stackAtLocation.StackSize + stackAtLocation.StackSize) -
                                        stackAtLocation.Item.MaxStackSize;
                                    if (stackAtLocation.AddToStack(SelectedStack.StackSize) == ItemStackState.Full)
                                    {
                                        SelectedStack.StackSize = possibleNewSize;
                                        StackBlockedForSelection = stackAtLocation;
                                    }
                                    else
                                    {
                                        StackBlockedForSelection = SelectedStack;
                                        SelectedStack = null;
                                        return;
                                    }
                                }
                            }
                        }

                        //if (SelectedStack.PreviousParent != null)
                        //{
                        //    SelectedStack.PreviousParent.ItemsList.Add(SelectedStack);
                        //    SelectedStack = null;
                        //}
                    }
                }
                else if (args.Button == MouseButton.Right)
                {
                    if (SelectedStack != null)
                    {
                        for (int i = 0; i < toDraw.Count; i++)
                        {
                            if (toDraw[i].SelectedSlot == new Vector2(-1, -1))
                            {
                                continue;
                            }

                            if (toDraw[i].GetIsSlotFree(toDraw[i].SelectedSlot))
                            {
                                ItemStack newStack = new ItemStack(SelectedStack.ItemKey, 1, toDraw[i].SelectedSlot);
                                toDraw[i].ItemsList.Add(newStack);
                                toDraw[i].ItemDroppedIntoContainer(newStack);

                                if (SelectedStack.RemoveFromStack() == ItemStackState.Empty)
                                    SelectedStack = null;

                                return;
                            }
                            else
                            {
                                var stackAtLocation = toDraw[i].GetItemStackByLocation(toDraw[i].SelectedSlot);
                                if (stackAtLocation.ItemKey == SelectedStack.ItemKey && !stackAtLocation.IsStackFull())
                                {
                                    stackAtLocation.AddToStack();

                                    if (SelectedStack.RemoveFromStack() == ItemStackState.Empty)
                                        SelectedStack = null;
                                    return;
                                }
                            }
                        }
                    }
                }

                StackBlockedForSelection = null;
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
                if (GUI.PressButton(SelectedStack.Item.Icon, rect))
                {
                    
                }
                GUI.Label(SelectedStack.StackSize.ToString(), rect, SlotNumberStyle);
            }
        }
    }
}
