using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Assets;

namespace VoxelNet.Blocks
{
    public class OakLogBlock : Block
    {
        public override string Key => "Block_Log_Oak";

        public override void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
            World.GetInstance().AddEntity(new ItemEntity(GameItems.LOG_OAK) { Position = WorldPosition + new Vector3(.5f, 0, .5f) });
        }
    }
}
