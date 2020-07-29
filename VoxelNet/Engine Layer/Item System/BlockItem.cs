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
    public class BlockItem : Item
    {
        public Block Block { get; }

        public BlockItem(Block block)
        {
            Block = block;
            Name = block.Key;
            IconLocation = $"Resources/Textures/Items/{Name}.png";
            GenerateGraphics();
            ItemDatabase.RegisterItem(this);
        }

        public override string Key => $"Item_{Name}";

        public override void OnInteract(Vector3 position, Chunk chunk)
        {
            chunk.PlaceBlock((int)position.X, (int) position.Y,
                (int) position.Z, Block, true);

        }
    }
}
