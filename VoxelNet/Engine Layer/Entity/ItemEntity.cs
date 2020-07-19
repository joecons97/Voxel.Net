using System;
using OpenTK;
using VoxelNet.Assets;
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

        public override void Render()
        {
            var material = AssetDatabase.GetAsset<Material>("Resources/Materials/Fallback.mat");
            material.SetTexture(0, item.Icon);
            var mat = Matrix4.CreateScale(new Vector3(.25f, -.25f,.25f)) * Matrix4.CreateRotationY(Time.GameTime) * Matrix4.CreateTranslation(Position);
            Renderer.DrawRequest(item.Mesh, material, mat);

            base.Render();
        }
    }
}
