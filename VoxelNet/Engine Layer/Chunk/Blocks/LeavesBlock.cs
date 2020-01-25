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
        public override GetBlockColor BlockColor => (x, y, z) => new Color4(117, 192, 93, 255);
    }
}
