using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Assets;
using VoxelNet.Blocks;

namespace VoxelNet
{
    public class SandBlock : Block
    {
        public override string Key => "Block_Sand";

        public override void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
            World.GetInstance().AddEntity(new ItemEntity(GameItems.SAND) { Position = WorldPosition + new Vector3(.5f, 0, .5f) });
        }
    }
}
