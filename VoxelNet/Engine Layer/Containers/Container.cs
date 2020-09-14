using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet.Containers
{
    public class Container
    {
        public virtual Vector2 ContainerSize { get; } = new Vector2(9, 5);
        public List<ItemStack> ItemsList { get; private set; } = new List<ItemStack>();

        public bool IsOpen { get; set; }

        public Vector2 SelectedSlot { get; set; }

        public void AddItem(Item item)
        {
            if (GetIsFull()) return;

            var stack = GetFirstEmptyStackByItem(item);
            if (stack != null && stack.WillStackBeFull(1))
            {
                stack.AddToStack();
            }
            else
            {
                stack = new ItemStack(item, GetFirstEmptyLocationInContainer());
                ItemsList.Add(stack);
            }
        }

        public void RemoveItem(Item item)
        {
            RemoveItem(item.Key);
        }

        public void RemoveItem(string itemKey)
        {
            var stack = GetFirstEmptyStackByItem(itemKey);
            if (stack != null)
            {
                if (stack.RemoveFromStack() == ItemStackState.Empty)
                    ItemsList.Remove(stack);
            }
        }
        public void RemoveItemFromStack(Item item, ItemStack stack)
        {
            RemoveItemFromStack(item.Key, stack);
        }

        public void RemoveItemFromStack(string itemKey, ItemStack stack)
        {
            if (stack != null)
            {
                if (stack.RemoveFromStack() == ItemStackState.Empty)
                    ItemsList.Remove(stack);
            }
        }

        public bool GetIsFull()
        {
            return GetFirstEmptyLocationInContainer() == new Vector2(-1, -1);
        }

        public bool GetIsSlotFree(Vector2 slot)
        {
            var sl = ItemsList.FirstOrDefault(x => x.LocationInContainer == slot);
            return sl == null;
        }

        Vector2 GetFirstEmptyLocationInContainer()
        {
            for (int y = 0; y < ContainerSize.Y; y++)
            {
                for (int x = 0; x < ContainerSize.X; x++)
                {
                    var stack = ItemsList.FirstOrDefault(v =>
                        v.LocationInContainer.X == x && v.LocationInContainer.Y == y);

                    if (stack == null)
                        return new Vector2(x, y);
                }
            }

            return new Vector2(-1,-1);
        }

        ItemStack GetFirstEmptyStackByItem(Item item)
        {
            var stack = ItemsList.FirstOrDefault(x => x.ItemKey == item.Key && !x.IsStackFull());
            if (stack != null) return stack;

            return null;
        }
        ItemStack GetFirstEmptyStackByItem(string key)
        {
            var stack = ItemsList.FirstOrDefault(x => x.ItemKey == key && !x.IsStackFull());
            if (stack != null) return stack;

            return null;
        }

        public ItemStack GetItemStackByLocation(int x, int y)
        {
            return GetItemStackByLocation(new Vector2(x, y));
        }
        public ItemStack GetItemStackByLocation(Vector2 loc)
        {
            return ItemsList.FirstOrDefault(x => x.LocationInContainer == loc);
        }

        public void Open()
        {
            ContainerRenderer.OpenContainer(this);
        }

        public void Close()
        {
            ContainerRenderer.CloseContainer(this);
        }

        protected void RenderCell(int x, int y, Rect rect)
        {
            var rectIcon = new Rect(rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8);

            GUI.Image(ContainerRenderer.ContainerSlot, rect);

            var stack = GetItemStackByLocation(x, y);
            if (stack != null)
            {
                if (GUI.HoldButton(stack.Item.Icon, rectIcon))
                {
                    if (ContainerRenderer.SelectedStack == null)
                    {
                        stack.PreviousParent = this;
                        ContainerRenderer.SelectedStack = stack;
                        ItemsList.Remove(stack);
                    }
                }

                GUI.Label(stack.StackSize.ToString(), rect, ContainerRenderer.SlotNumberStyle);
            }
        }

        public virtual void RenderGUI()
        {
            float winWidth = Program.Settings.WindowWidth;
            float winHeight = Program.Settings.WindowHeight;
            float slotSize = ContainerRenderer.SLOT_SIZE;
            Vector2 size = ContainerSize * slotSize * 2;

            Rect parentRect = new Rect((winWidth / 2f) - (size.X / 4f) - slotSize/2f, (winHeight / 2f) - (size.Y / 4f) - slotSize/2f,
                (size.X / 2f + 35), (size.Y/2f + 30));

            GUI.Image(ContainerRenderer.ContainerBackground, parentRect, 5);

            bool anySlotSelected = false;
            for (int x = 0; x < ContainerSize.X; x++)
            {
                for (int y = 0; y < ContainerSize.Y; y++)
                {
                    var rect = new Rect(x * (slotSize + 2) + parentRect.X + 10, y * (slotSize + 2) + parentRect.Y + 10,
                        slotSize, slotSize);

                    if (rect.IsPointInside(GUI.MousePosition))
                    {
                        SelectedSlot = new Vector2(x, y);
                        anySlotSelected = true;
                    }

                    RenderCell(x, y, rect);
                }
            }
            if(anySlotSelected == false) 
                SelectedSlot = new Vector2(-1, -1);
        }
    }
}
