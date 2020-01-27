using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using OpenTK;
using OpenTK.Input;
using VoxelNet.Assets;
using VoxelNet.Physics;
using VoxelNet.Rendering;

namespace VoxelNet.Entity
{
    public class Player : Entity
    {
        private bool hasHadInitialSet;
        private World currentWorld;
        private Rigidbody rigidbody;
        private Vector3 vel;

        private float walkSpeed = 3.5f;

        public override void Begin()
        {
            Name = "Player";
            currentWorld = World.GetInstance();
            hasHadInitialSet = false;

            Input.Input.GetSetting("Jump").KeyDown += () =>
            {
                if(Raycast.CastVoxel(currentWorld.WorldCamera.Position, new Vector3(0,-1,0), 2.1f, out RayVoxelOut output))
                    rigidbody.AddImpluse(new Vector3(0, 1, 0) * 600);
            };

            Input.Input.GetSetting("Interact").KeyDown += () =>
            {
                if (Raycast.CastVoxel(currentWorld.WorldCamera.Position, currentWorld.WorldCamera.GetForward(), 5,
                    out RayVoxelOut op))
                {
                    if (currentWorld.TryGetChunkAtPosition((int)op.PlacementChunk.X, (int)op.PlacementChunk.Y, out Chunk chunk))
                    {
                        chunk.PlaceBlock((int)op.PlacementPosition.X, (int)op.PlacementPosition.Y, (int)op.PlacementPosition.Z, 1);

                        currentWorld.RequestChunkUpdate(chunk);
                    }
                }
            };

            Input.Input.GetSetting("Destroy Block").KeyDown += () =>
            {
                if (Raycast.CastVoxel(currentWorld.WorldCamera.Position, currentWorld.WorldCamera.GetForward(), 5,
                    out RayVoxelOut op))
                {
                    if (currentWorld.TryGetChunkAtPosition((int) op.ChunkPosition.X, (int) op.ChunkPosition.Y,
                        out Chunk chunk))
                    {
                        chunk.DestroyBlock((int) op.BlockPosition.X, (int) op.BlockPosition.Y, (int) op.BlockPosition.Z);

                        currentWorld.RequestChunkUpdate(chunk);
                    }
                }
            };
        }

        void HandleInput()
        {
            vel = Vector3.Zero;
            KeyboardState kbdState = Keyboard.GetState();

            if (kbdState.IsKeyDown(Key.S))
                vel += -GetForwardVector() * walkSpeed;
            else if (kbdState.IsKeyDown(Key.W))
                vel += GetForwardVector() * walkSpeed;

            if (kbdState.IsKeyDown(Key.A))
                vel += -GetRightVector() * walkSpeed;
            else if (kbdState.IsKeyDown(Key.D))
                vel += GetRightVector() * walkSpeed;

            rigidbody.Velocity = new Vector3(vel.X, rigidbody.Velocity.Y, vel.Z);

            currentWorld.WorldCamera.Position = Position + new Vector3(0, 1.7f, 0);

            float x = Input.Input.GetMouseDelta().X / 20f;
            float y = Input.Input.GetMouseDelta().Y / 20f;

            Rotation = new Vector3(0, Rotation.Y + x, 0);
            currentWorld.WorldCamera.Rotation = new Vector3(currentWorld.WorldCamera.Rotation.X + y, Rotation.Y, 0);
        }

        public override void Update()
        {
            if (!hasHadInitialSet && currentWorld.TryGetChunkAtPosition((int)GetChunk().X, (int)GetChunk().Y, out Chunk c))
            {
                Position.Y = c.GetHeightAtBlock((int)GetPositionInChunk().X, (int)GetPositionInChunk().Z);
                hasHadInitialSet = true;

                rigidbody = new Rigidbody(this, 70, new BoundingBox(-0.25f, 0.25f, 0, 2, -0.25f, 0.25f));
            }

            if (hasHadInitialSet)
                HandleInput();
        }

        public override void GUI()
        {
            ImGui.Begin("debug", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.SetWindowPos("debug", new System.Numerics.Vector2(0, 0));
            ImGui.Text($"{1/ Time.DeltaTime}fps" + $" {Time.DeltaTime}ms");
            ImGui.Text($"Draw calls: {Renderer.DrawCalls}");

            ImGui.Text($"World Pos:  {currentWorld.WorldCamera.Position.ToString()}");
            ImGui.Text($"Chunk:  {currentWorld.WorldCamera.Position.ToChunkPosition().ToString()}");
            ImGui.Text($"Pos In Chunk:  {currentWorld.WorldCamera.Position.ToChunkSpace().ToString()}");
            ImGui.Text($"Velocity:  {rigidbody?.Velocity}");

            ImGui.End();

            ImGui.Begin("crosshair", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.AlwaysAutoResize);

            int size = 32;
            ImGui.Image((IntPtr)currentWorld.TexturePack.Crosshair.Handle, System.Numerics.Vector2.One * size);

            ImGui.SetWindowPos("crosshair", new System.Numerics.Vector2(((float)Program.Window.Width / 2f) - 16, ((float)Program.Window.Height / 2f) - 16));

            ImGui.End();
        }
    }
}
