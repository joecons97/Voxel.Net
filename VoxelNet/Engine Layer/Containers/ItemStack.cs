using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Assets;

namespace VoxelNet.Containers
{
    public enum ItemStackState
    {
        Normal,
        Empty,
        Full
    }

    public class ItemStack
    {
        public string ItemKey { get; }

        public Item Item
        {
            get { return ItemDatabase.GetBlock(ItemKey); }
        }

        private int _stackSize;
        public int StackSize
        {
            get { return _stackSize; }
            set { _stackSize = MathHelper.Clamp(value, 0, Item.MaxStackSize); }
        }

        public Vector2 LocationInContainer { get; set; }

        public ItemStack() { }
        public ItemStack(Item item, Vector2 location)
        {
            ItemKey = item.Key;
            StackSize = 1;
            LocationInContainer = location;
        }
        public ItemStack(string itemKey, int stackSize, Vector2 location)
        {
            ItemKey = itemKey;
            StackSize = stackSize;
            LocationInContainer = location;
        }
        public ItemStack(string itemKey, Vector2 location)
        {
            ItemKey = itemKey;
            StackSize = 1;
            LocationInContainer = location;
        }
        public ItemStack(Item item, int stackSize, Vector2 location)
        {
            ItemKey = item.Key;
            StackSize = stackSize;
            LocationInContainer = location;
        }

        public bool IsStackFull()
        {
            if (Item == null) return false;

            return Item.MaxStackSize == StackSize;
        }

        public bool WillStackBeFull(int num)
        {
            if (Item == null) return false;

            return Item.MaxStackSize >= StackSize + num;
        }

        public ItemStackState AddToStack(int num = 1)
        {
            StackSize += num;
            if (IsStackFull())
                return ItemStackState.Full;

            return ItemStackState.Normal;
        }

        public ItemStackState RemoveFromStack(int num = 1)
        {
            StackSize -= num;

            if (StackSize <= 0)
                return ItemStackState.Empty;

            return ItemStackState.Normal;
        }
    }
}
