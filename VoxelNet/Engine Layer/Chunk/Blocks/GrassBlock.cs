using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;

namespace VoxelNet.Blocks
{
    public class GrassBlock : Block
    {
        public override int ID => 2;
        public override string Key => "Block_Grass";
        public override GetBlockColor BlockColor => (x, y, z) => new Color4(117, 192, 93, 255);
    }
}
