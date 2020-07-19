using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelNet.Blocks;

namespace VoxelNet.Assets
{
    public static class ItemDatabase
    {
        static Dictionary<string, Item> items = new Dictionary<string, Item>();

        public static void RegisterItem(Item item)
        {
            if (items.ContainsKey(item.Key))
            {
                Debug.Log("Item with key: " + item.Key + " already exists! Cancelling this addition", DebugLevel.Warning);
                return;
            }
            item.ID = 1 + items.Count;
            items.Add(item.Key, item);
        }

        public static Item GetBlock(string key)
        {
            if (items.TryGetValue(key, out Item item))
            {
                return item;
            }

            return null;
        }

        public static Item GetBlock(int id)
        {
            var val = items.Values.FirstOrDefault(x => x.ID == id);
            return val;
        }

        public static void SetBlock(string key, Item item)
        {
            if (items.ContainsKey(key))
            {
                items[key] = item;
            }
        }
    }
}
