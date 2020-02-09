using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelNet.Blocks;

namespace VoxelNet
{
    public static class GameBlocks
    {
        public static void Init() { }

        public static readonly DirtBlock DIRT = new DirtBlock();

        public static readonly GrassBlock GRASS = new GrassBlock();

        public static readonly StoneBlock STONE = new StoneBlock();

        public static readonly SandBlock SAND = new SandBlock();

        public static readonly WaterBlock WATER = new WaterBlock();

        public static readonly OakLogBlock LOG_OAK = new OakLogBlock();

        public static readonly LeavesOakBlock LEAVES_OAK = new LeavesOakBlock();
    }
}
