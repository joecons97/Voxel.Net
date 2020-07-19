using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet.Blocks
{
    public class LeavesOakBlock : LeavesBlock
    {
        public override string Key => "Block_Leaves_Oak";

        public override void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {

        }
    }
}
