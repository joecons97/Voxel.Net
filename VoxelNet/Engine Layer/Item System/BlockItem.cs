using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelNet.Blocks;

namespace VoxelNet
{
    public class BlockItem : Item
    {
        public Block Block { get; }

        public BlockItem(Block block)
        {
            Block = block;
            Name = block.Key;
            IconLocation = $"Resources/Textures/Items/{Name}.png";
            GenerateGraphics();
        }

        public override string Key => $"Item_{Name}";
    }
}
