using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Assets;

namespace VoxelNet.Blocks
{
    public class StoneBlock : Block
    {
        public override string Key => "Block_Stone";

        public override void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
            World.GetInstance().AddEntity(new ItemEntity(GameItems.STONE) { Position = WorldPosition + new Vector3(.5f, 0, .5f) });
        }
    }
}
