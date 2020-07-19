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
        public Vector2 ContainerSize { get; } = new Vector2(9, 5);
        public List<ItemStack> ItemsList { get; private set; }

        public bool IsOpen { get; set; }

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

        public bool GetIsFull()
        {
            return GetFirstEmptyLocationInContainer() == new Vector2(-1, -1);
        }

        Vector2 GetFirstEmptyLocationInContainer()
        {
            for (int x = 0; x < ContainerSize.X; x++)
            {
                for (int y = 0; y < ContainerSize.Y; y++)
                {
                    var stack = ItemsList.FirstOrDefault(v =>
                        v.LocationInContainer.X == x && v.LocationInContainer.Y == y);

                    if(stack == null)
                        return new Vector2(x,y);
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

        public void Open()
        {
            ContainerRenderer.OpenContainer(this);
        }

        public void Close()
        {
            ContainerRenderer.CloseContainer(this);
        }

        public virtual void RenderGUI()
        {
            float winWidth = Program.Settings.WindowWidth;
            float winHeight = Program.Settings.WindowHeight;

            GUI.Image(ContainerRenderer.ContainerBackground, new Rect(winWidth/2f - ContainerSize.X/2f - 10f, winHeight / 2f - ContainerSize.Y / 2f - 10f,
                winWidth / 2f - ContainerSize.X / 2f - 10f, winHeight / 2f - ContainerSize.Y / 2f - 10f), 5);
        }
    }
}
