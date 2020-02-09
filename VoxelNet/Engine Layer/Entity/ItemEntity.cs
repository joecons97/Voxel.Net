using OpenTK;
using VoxelNet.Assets;
using VoxelNet.Physics;
using VoxelNet.Rendering;

namespace VoxelNet
{
    public class ItemEntity : Entity
    {
        private Item item;
        private Rigidbody rigidbody;

        public ItemEntity(Item i)
        {
            item = i;
            World.GetInstance().AddEntity(this);

        }

        public override void Begin()
        {
            rigidbody = new Rigidbody(this, 5, new BoundingBox(-0.125f, 0.125f, -0.125f, 0.125f, -0.125f, 0.125f));
            rigidbody.AddImpluse(new Vector3(0, 30, 0));
            base.Begin();
        }

        public override void Update()
        {
            var chunkPos = Position.ToChunkPosition();
            if (World.GetInstance().TryGetChunkAtPosition((int)chunkPos.X, (int)chunkPos.Z, out Chunk chunk))
            {
                var block = Position.ToChunkSpaceFloored();
                bool isInWater = chunk.GetBlockID((int)block.X, (int)block.Y, (int)block.Z) == GameBlocks.WATER.ID;
                rigidbody.Drag = isInWater ? UnderWaterDrag : 0;
            }
        }

        public override void Render()
        {
            Renderer.DrawRequest(item.Mesh, null, Matrix4.CreateScale(.25f) * Matrix4.CreateRotationY(Time.GameTime) * Matrix4.CreateTranslation(Position));

            base.Render();
        }
    }
}
