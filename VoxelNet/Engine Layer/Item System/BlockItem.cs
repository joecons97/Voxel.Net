using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelNet
{
    public class BlockItem : Item
    {
        public override string Name => "Block";
        public override string ID => "Item_Block";
        public override int Key => -1;
    }
}
