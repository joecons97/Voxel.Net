using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelNet.Assets;

namespace VoxelNet.Blocks
{
    public class BlockState
    {
        public Block Block { get; }

        public BlockState(string blockId)
        {
            Block = BlockDatabase.GetBlock(blockId);
        }
    }
}
