using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using VoxelNet.Blocks;
using VoxelNet.Physics;

namespace VoxelNet
{
    public class WaterBlock : Block
    {
        public override string Key => "Block_Water";
        public override bool IsTransparent => true;
        public override GetBlockColor BlockColor { get; set; } = (i, i1, i2) => Color4.Blue;
        public override Shape CollisionShape => null;
    }
}
