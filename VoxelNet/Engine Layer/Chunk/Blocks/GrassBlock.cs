using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using VoxelNet.Assets;

namespace VoxelNet.Blocks
{
    public class GrassBlock : Block
    {
        public override string Key => "Block_Grass";
        public override GetBlockColor BlockColor => (x,y,z) =>
        {
            float scale = 0.25f;
            var biome = 1;//(float)(World.GetInstance().BiomeNoise.Value2D(((float)x/Chunk.WIDTH) * scale, ((float)z / Chunk.WIDTH) * scale) + 1f) / 2f;

            return new Color4(0.25f * biome, 0.75f * biome, 0.16f * biome,1);
        };
}
}
