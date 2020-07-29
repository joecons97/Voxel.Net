using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelNet.Rendering
{
    public struct DrawElementsCmd
    {
        public uint count;
        public uint instanceCount;
        public uint firstIndex;
        public uint baseVertex;
        public uint baseInstance;
    }
}
