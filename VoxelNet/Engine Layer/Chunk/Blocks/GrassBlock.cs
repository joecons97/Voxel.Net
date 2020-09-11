using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using VoxelNet.Assets;

namespace VoxelNet.Blocks
{
    public class GrassBlock : Block
    {
        public override string Key => "Block_Grass";
        public override GetBlockColor BlockColor => (x,y,z) =>
        {
            var biome = 1;

            return new Color4(0.25f * biome, 0.75f * biome, 0.16f * biome,1);
        };

        public override void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
            World.GetInstance().AddEntity(new ItemEntity(GameItems.DIRT) { Position = WorldPosition + new Vector3(.5f, 0, .5f) });
        }
    }
}
