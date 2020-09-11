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
        public override sbyte Opacity => 6;

        public override GetBlockColor BlockColor => (x, y, z) =>
        {
            var biome = 1;

            return new Color4(0.25f * biome, 0.75f * biome, 0.16f * biome, 1);
        };
    }
}
