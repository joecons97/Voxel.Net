using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelNet.Blocks;

namespace VoxelNet.Assets
{
    public static class BlockDatabase
    {
        static Dictionary<string, Block> blocks = new Dictionary<string, Block>();

        public static void RegisterBlock(Block block)
        {
            block.ID = 1 + blocks.Count;
            blocks.Add(block.Key, block);
        }

        public static Block GetBlock(string key)
        {
            if (blocks.TryGetValue(key, out Block block))
            {
                return block;
            }

            return null;
        }

        public static Block GetBlock(int id)
        {
            var val = blocks.Values.FirstOrDefault(x => x.ID == id);
            return val;
        }

        public static void SetBlock(string key, Block block)
        {
            if (blocks.ContainsKey(key))
            {
                blocks[key] = block;
            }
        }
    }
}
