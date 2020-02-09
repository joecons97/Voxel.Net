using System;
using System.Collections.Generic;
using ImGuiNET;
using OpenTK;
using VoxelNet.Inventory_System;
using VoxelNet.Rendering;
using SysVector2 = System.Numerics.Vector2;

namespace VoxelNet
{
    public class Inventory
    {
        static List<Inventory> openInventories = new List<Inventory>();

        public static void GUIAll()
        {
            for (int i = 0; i < openInventories.Count; i++)
            {
                if (openInventories[i] != null)
                    openInventories[i].GUI();
            }
        }

        public bool IsOpen { get; private set; }

        protected IntPtr SlotImgHandle;
        protected virtual Vector2 InventorySize { get; } = new Vector2(9, 3);
        private InventorySlot[] slots;

        int dragFrom = -1, dragTo = -1;
        private int inventoryId;


        public Inventory()
        {
            SlotImgHandle = (IntPtr)AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/Inventory_Slot.png").Handle;

            slots = new InventorySlot[(int)(InventorySize.X * InventorySize.Y)];
            int i = 0;
            for (var index = 0; index < slots.Length; index++)
            {
                slots[index] = new InventorySlot((short)(i + 1));
                i++;
            }
        }

        public void SetIsOpen(bool isOpen)
        {
            IsOpen = isOpen;

            if (isOpen)
            {
                inventoryId = openInventories.Count;
                openInventories.Add(this);
            }
            else
            {
                inventoryId = -1;
                openInventories.Remove(this);
            }
        }

        public virtual void GUI()
        {
            if (ImGui.IsMouseReleased(0))
            {
                if (dragFrom != -1 && dragTo != -1)
                {
                    var temp = slots[dragTo];
                    slots[dragTo] = slots[dragFrom];
                    slots[dragFrom] = temp;
                    dragFrom = -1;
                    dragTo = -1;
                }
            }

            ImGui.Begin("Inventory", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.NoMove);
            ImGui.SetWindowPos(new SysVector2(Program.Window.Width / 2f - (ImGui.GetWindowWidth() / 2), Program.Window.Height / 2f - (ImGui.GetWindowHeight() / 2)));
            for (int y = 0; y < InventorySize.Y; y++)
            {
                for (int x = 0; x < InventorySize.X; x++)
                {
                    DrawSlot(x, y);
                }
                ImGui.NewLine();
            }
            ImGui.End();


            void DrawSlot(int x, int y)
            {
                int index = x * (int)InventorySize.Y + y;
                ImGuiDragDropFlags src_flags = ImGuiDragDropFlags.SourceNoDisableHover;
                src_flags |= ImGuiDragDropFlags.SourceNoHoldToOpenOthers;

                ImGui.PushID($"{inventoryId} - {x},{y}");
                ImGui.Button(slots[index].ItemID.ToString(), SysVector2.One * 32);
                //ImGui.ImageButton(SlotImgHandle, SysVector2.One * 32, SysVector2.Zero, SysVector2.One, 0);
                ImGui.PopID();

                if (ImGui.BeginDragDropSource(src_flags))
                {
                    string str = $"{x},{y}";
                    unsafe
                    {
                        IntPtr parameter = new IntPtr(&index);
                        ImGui.SetDragDropPayload("ITEM", parameter, sizeof(int), ImGuiCond.Once);
                    }
                    ImGui.Button(slots[index].ItemID.ToString(), SysVector2.One * 32);
                    //ImGui.ImageButton(SlotImgHandle, SysVector2.One * 32, SysVector2.Zero, SysVector2.One, 0);
                    ImGui.EndDragDropSource();
                }
                if (ImGui.BeginDragDropTarget())
                {
                    ImGuiDragDropFlags target_flags = ImGuiDragDropFlags.AcceptBeforeDelivery;

                    var payload = ImGui.AcceptDragDropPayload("ITEM", target_flags);
                    if (payload.IsDelivery())
                        return;

                    int i = 0;
                    unsafe
                    {
                        i = *(int*)payload.NativePtr->Data;
                    }

                    dragFrom = i;
                    dragTo = index;

                    ImGui.EndDragDropTarget();
                }
                ImGui.SameLine();
            }
        }
    }
}
