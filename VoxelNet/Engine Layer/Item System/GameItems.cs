using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelNet.Blocks;

namespace VoxelNet
{
    public static class GameItems
    {
        public static void Init()
        {
            DIRT = new BlockItem(GameBlocks.DIRT);
            GRASS = new BlockItem(GameBlocks.GRASS);
            STONE = new BlockItem(GameBlocks.STONE);
            SAND = new BlockItem(GameBlocks.SAND);
            LOG_OAK = new BlockItem(GameBlocks.LOG_OAK);
            PLANKS_OAK = new BlockItem(GameBlocks.PLANKS_OAK);
        }

        public static BlockItem DIRT { get; private set; }

        public static BlockItem GRASS { get; private set; }

        public static BlockItem STONE { get; private set; }

        public static BlockItem SAND { get; private set; }

        public static BlockItem LOG_OAK { get; private set; }
        public static BlockItem PLANKS_OAK { get; private set; }
    }
}
