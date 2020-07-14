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
        //public override string Name => "Block";
        //public override string ID => "Item_Block";
        public override int Key => -1;

        public Block Block { get; }

        public BlockItem(Block block)
        {
            Block = block;
            Name = block.Key;
            ID = $"Item_{Name}";
            IconLocation = $"Resources/Textures/Items/{Name}.png";
            GenerateGraphics();
        }
    }
}
