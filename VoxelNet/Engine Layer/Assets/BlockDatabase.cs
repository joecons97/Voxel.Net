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

        public static void Init()
        {
            DirtBlock dirt = new DirtBlock();
            blocks.Add(dirt.Key, dirt);

            GrassBlock grass = new GrassBlock();
            blocks.Add(grass.Key, grass);

            StoneBlock stone = new StoneBlock();
            blocks.Add(stone.Key, stone);
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
