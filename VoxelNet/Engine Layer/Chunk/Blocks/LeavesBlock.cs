using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;

namespace VoxelNet.Blocks
{
    public class LeavesBlock : Block
    {
        public override bool IsTransparent => true;
        public override sbyte Opacity => 5;

        public override GetBlockColor BlockColor => (x, y, z) =>
        {
            float scale = 0.25f;
            var biome = 1;//(float)(World.GetInstance().BiomeNoise.Value2D(((float)x/Chunk.WIDTH) * scale, ((float)z / Chunk.WIDTH) * scale) + 1f) / 2f;

            return new Color4(0.25f * biome, 0.75f * biome, 0.16f * biome, 1);
        };
    }
}
