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
        public static void Init()
        {
            DIRT = new DirtBlock();
            GRASS = new GrassBlock();
            STONE = new StoneBlock();
            SAND = new SandBlock();
            WATER = new WaterBlock();
            LOG_OAK = new OakLogBlock();
            LEAVES_OAK = new LeavesOakBlock();
            PLANKS_OAK = new OakWoodPlanks();
        }

        public static DirtBlock DIRT { get; private set; }

        public static GrassBlock GRASS { get; private set; }

        public static StoneBlock STONE { get; private set; }

        public static SandBlock SAND { get; private set; }

        public static WaterBlock WATER { get; private set; }

        public static OakLogBlock LOG_OAK { get; private set; }

        public static OakWoodPlanks PLANKS_OAK { get; private set; }

        public static LeavesOakBlock LEAVES_OAK { get; private set; }
    }
}
