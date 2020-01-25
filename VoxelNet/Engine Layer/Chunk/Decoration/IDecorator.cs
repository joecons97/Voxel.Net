using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelNet.Decoration
{
    public interface IDecorator
    {
        void DecorateAtBlock(Chunk chunk, int x, int y, int z);
    }
}
