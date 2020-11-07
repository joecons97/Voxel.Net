using System;
using OpenTK;
using VoxelNet.Assets;
using VoxelNet.Entities;
using VoxelNet.Misc;
using VoxelNet.Physics;
using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;

namespace VoxelNet
{
    public class ItemEntity : Entity
    {
        private Item item;
        private Rigidbody rigidbody;

        public ItemEntity(Item i)
        {
            item = i;
        }

        public override void Begin()
        {
            rigidbody = new Rigidbody(this, 5, new BoundingBox(-0.125f, 0.125f, -0.125f, 0.125f, -0.125f, 0.125f));
            rigidbody.AddImpluse(new Vector3(0, 30, 0));
            Scale = new Vector3(.25f, -.25f, .25f);
            Mesh = item.Mesh;
            Material = item.Material;
            Material.SetTexture(0, item.Icon);

            base.Begin();
        }

        public override void Update()
        {
            Rotation += new Vector3(0, 1, 0);

            var chunkPos = Position.ToChunkPosition();
            if (World.GetInstance().TryGetChunkAtPosition((int)chunkPos.X, (int)chunkPos.Z, out Chunk chunk))
            {
                var block = Position.ToChunkSpaceFloored();
                bool isInWater = chunk.GetBlockID((int)block.X, (int)block.Y, (int)block.Z) == GameBlocks.WATER.ID;
                rigidbody.Drag = isInWater ? UnderWaterDrag : 0;
            }

            var goToPos = World.GetInstance().WorldCamera.Position - Vector3.UnitY * 1;

            var dist = Vector3.Distance(Position, goToPos);
            if (dist < 2)
            {
                rigidbody.IsActive = false;
                var dir = ((goToPos) - Position).Normalized();
                var move = (dir / dist) * 5;
                Position += move * Time.DeltaTime;
                if (dist < 0.5f)
                {
                    //Add to inventory
                    World.GetInstance().GetPlayer().GetInventory().AddItem(item);
                    World.GetInstance().DestroyEntity(this);
                }
            }
            else
            {
                rigidbody.IsActive = true;
            }
        }

        public override void Destroyed()
        {
            rigidbody.ClearOwner();
            rigidbody = null;
        }
    }
}
