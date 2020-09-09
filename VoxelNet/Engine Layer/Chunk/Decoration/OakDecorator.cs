using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Assets;
using VoxelNet.Misc;

namespace VoxelNet.Decoration
{
    public class OakDecorator : Decorator
    {
        private int maxTrees = 8;
        private int treeDists = 3;
        private float treeChance = .0125f;
        List<Vector2> treePoses = new List<Vector2>();

        public override void Dispose()
        {

        }

        void PlaceLeaf(int x, int y, int z, Chunk chunk)
        {
            int blockid = chunk.GetBlockID(x, y, z);
            if (blockid == 0)
                chunk.PlaceBlock(x, y, z, GameBlocks.LEAVES_OAK, false);
        }

        public override void DecorateAtBlock(Chunk chunk, int x, int y, int z)
        {
            if (chunk.GetBlockID(x, y, z) != (short)GameBlocks.GRASS.ID)
                return;

            bool chance = Maths.Chance(treeChance);
            if (!chance) return;

            if (treePoses.Count >= maxTrees)
                return;

            var otherX = Chunk.WIDTH - x;
            var otherZ = Chunk.WIDTH - z;
            if (x >= treeDists && z >= treeDists && otherX >= treeDists && otherZ >= treeDists)
            {
                if (treePoses.Count == 0 || treePoses.Any(v => Vector2.Distance(v, new Vector2(x, z)) > treeDists))
                {
                    chunk.PlaceBlock(x, y, z, GameBlocks.DIRT, false);
                    var height = World.GetInstance().Randomizer.Next(5, 8);
                    int leaves = 2;
                    for (int i = 0; i < height; i++)
                    {
                        chunk.PlaceBlock(x, y + i + 1, z, (short)GameBlocks.LOG_OAK.ID, false);
                    }

                    var leavesHeight = height - leaves;
                    for (int lx = -leaves; lx <= leaves; lx++)
                    {
                        for (int lz = -leaves; lz <= leaves; lz++)
                        {
                            for (int ly = 0; ly < leaves; ly++)
                            {
                                PlaceLeaf(x + lx, y + leavesHeight + ly, z + lz, chunk);
                            }
                        }
                    }

                    leaves--;

                    for (int lx = -leaves; lx <= leaves; lx++)
                    {
                        for (int lz = -leaves; lz <= leaves; lz++)
                        {
                            PlaceLeaf(x + lx, y + leavesHeight + 2, z + lz, chunk);
                        }
                    }

                    PlaceLeaf(x, y + leavesHeight + 2 + leaves, z, chunk);

                    PlaceLeaf(x + 1, y + leavesHeight + 2 + leaves, z, chunk);
                    PlaceLeaf(x - 1, y + leavesHeight + 2 + leaves, z, chunk);
                    PlaceLeaf(x, y + leavesHeight + 2 + leaves, z + 1, chunk);
                    PlaceLeaf(x, y + leavesHeight + 2 + leaves, z - 1, chunk);
                    
                    treePoses.Add(new Vector2(x, z));
                }
            }
        }
    }
}
