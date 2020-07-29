using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Input;
using VoxelNet.Assets;
using VoxelNet.Containers;
using VoxelNet.Physics;
using VoxelNet.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace VoxelNet.Entities
{
    public class Player : Entity
    {
        private bool hasHadInitialSet;
        private World currentWorld;
        private Rigidbody rigidbody;
        private Vector3 vel;
        private bool isInWater = false;
        private bool isSprinting;

        private float walkSpeed = 3.5f;
        private float runSpeed = 6f;

        private static bool controlsEnabled = true;
        private static bool mouseHidden = true;

        PlayerInventory inventory = new PlayerInventory();

        public override void Begin()
        {
            Name = "Player";
            currentWorld = World.GetInstance();
            hasHadInitialSet = false;

            Input.Input.GetSetting("Jump").KeyDown += () =>
            {
                if (!controlsEnabled || isInWater)
                    return;

                if (Raycast.CastVoxel(currentWorld.WorldCamera.Position, new Vector3(0,-1,0), 2.1f, out RayVoxelOut output))
                    rigidbody.AddImpluse(new Vector3(0, 1, 0) * 600);
            };

            Input.Input.GetSetting("Interact").KeyDown += () =>
            {
                if (!controlsEnabled)
                    return;

                if (Raycast.CastVoxel(currentWorld.WorldCamera.Position, currentWorld.WorldCamera.GetForward(), 5,
                    out RayVoxelOut op))
                {
                    if (currentWorld.TryGetChunkAtPosition((int)op.PlacementChunk.X, (int)op.PlacementChunk.Y, out Chunk chunk))
                    {
                        var stack = inventory.GetItemStackByLocation(inventory.SelectedItemIndex, 0);
                        if (stack != null)
                        {
                            stack.Item.OnInteract(op.PlacementPosition, chunk);
                            inventory.RemoveItem(stack.Item);
                            currentWorld.RequestChunkUpdate(chunk);
                        }
                    }
                }
            };

            Input.Input.GetSetting("Destroy Block").KeyDown += () =>
            {
                if (!controlsEnabled)
                    return;

                if (Raycast.CastVoxel(currentWorld.WorldCamera.Position, currentWorld.WorldCamera.GetForward(), 5,
                    out RayVoxelOut op))
                {
                    if (currentWorld.TryGetChunkAtPosition((int) op.ChunkPosition.X, (int) op.ChunkPosition.Y,
                        out Chunk chunk))
                    {
                        chunk.DestroyBlock((int) op.BlockPosition.X, (int) op.BlockPosition.Y, (int) op.BlockPosition.Z);

                        var chunkWp = (op.ChunkPosition * Chunk.WIDTH);
                        var wp = new Vector3(chunkWp.X, 0, chunkWp.Y) + op.BlockPosition;
                        BlockDatabase.GetBlock(op.BlockID).OnBreak(wp, op.ChunkPosition);

                        currentWorld.RequestChunkUpdate(chunk);
                    }
                }
            };

            Input.Input.GetSetting("Inventory").KeyDown += () =>
            {
                if(inventory.IsOpen)
                    inventory.Close();
                else
                    inventory.Open();

                SetMouseVisible(inventory.IsOpen);
                SetControlsActive(!inventory.IsOpen);
            };


            Program.Window.MouseWheel += (sender, args) =>
            {
                if(args.Delta > 0)
                    inventory.SelectedItemIndex--;
                else
                    inventory.SelectedItemIndex++;

                if (inventory.SelectedItemIndex > inventory.ContainerSize.X - 1)
                    inventory.SelectedItemIndex = 0;
                else if (inventory.SelectedItemIndex < 0) 
                    inventory.SelectedItemIndex = (int)inventory.ContainerSize.X - 1;
            };

            Input.Input.GetSetting("Sprint").KeyDown += () => isSprinting = true;
            Input.Input.GetSetting("Sprint").KeyUp += () => isSprinting = false;
        }

        void HandleInput()
        {
            vel = Vector3.Zero;

            if (controlsEnabled)
            {
                KeyboardState kbdState = Keyboard.GetState();

                var finalSpeed = walkSpeed;

                if (isSprinting)
                    finalSpeed = runSpeed;

                if (kbdState.IsKeyDown(Key.S))
                    vel += -GetForwardVector() * finalSpeed;
                else if (kbdState.IsKeyDown(Key.W))
                    vel += GetForwardVector() * finalSpeed;

                if (kbdState.IsKeyDown(Key.A))
                    vel += -GetRightVector() * finalSpeed;
                else if (kbdState.IsKeyDown(Key.D))
                    vel += GetRightVector() * finalSpeed;

                if (isInWater)
                {
                    if (kbdState.IsKeyDown(Key.Space))
                        rigidbody.AddForce(new Vector3(0, 1, 0) * 4000);
                }

                float x = Input.Input.GetMouseDelta().X / 20f;
                float y = Input.Input.GetMouseDelta().Y / 20f;

                Rotation = new Vector3(0, Rotation.Y + x, 0);
                currentWorld.WorldCamera.Rotation = new Vector3(currentWorld.WorldCamera.Rotation.X + y, Rotation.Y, 0);
            }

            rigidbody.Velocity = new Vector3(vel.X, rigidbody.Velocity.Y, vel.Z);
        }

        public override void Update()
        {
            if (!hasHadInitialSet)
            {
                Position.Y = Chunk.HEIGHT;
                if (Raycast.CastVoxel(new Vector3(Position),
                    new Vector3(0, -1, 0), Chunk.HEIGHT, out RayVoxelOut hit))
                {
                    var chunkWp = (hit.BlockPosition);
                    Position = chunkWp + new Vector3(0.5f, Chunk.HEIGHT, 0.5f);
                    Debug.Log("Hit block for y pos " + Position.Y);
                    rigidbody = new Rigidbody(this, 70, new BoundingBox(-0.25f, 0.25f, 0, 2, -0.25f, 0.25f));
                    hasHadInitialSet = true;
                }
            }

            currentWorld.WorldCamera.Position = Position + new Vector3(0, 1.7f, 0);

            if (hasHadInitialSet)
            {
                HandleInput();

                var chunkPos = Position.ToChunkPosition();
                if (World.GetInstance().TryGetChunkAtPosition((int) chunkPos.X, (int) chunkPos.Z, out Chunk chunk))
                {
                    var block = Position.ToChunkSpaceFloored();
                    isInWater = chunk.GetBlockID((int) block.X, (int) block.Y, (int) block.Z) == GameBlocks.WATER.ID;
                    rigidbody.Drag = isInWater ? UnderWaterDrag : 0;
                }
            }
        }

        public override void RenderGUI()
        {
            var rect = new Rect(8, 8, 1024, 32);
            GUI.Label($"{(int)Time.FramesPerSecond}fps", rect);
            rect.Y += 32f;
            GUI.Label($"Clipped: {Renderer.ClippedCount}", rect);
            rect.Y += 32f;
            GUI.Label($"Chunks loaded: {World.GetInstance().GetLoadedChunks().Length}", rect);
            rect.Y += 32f;
            GUI.Label($"Loc in chunk: {GetPositionInChunk()}", rect);

            GUI.Image(currentWorld.TexturePack.Crosshair, new Rect((Program.Window.Width / 2) - 16, (Program.Window.Height / 2) - 16, 32, 32));
            inventory.RenderToolBar();
        }

        public static void SetControlsActive(bool active)
        {
            controlsEnabled = active;
        }

        public static void SetMouseVisible(bool visible)
        {
            if (mouseHidden != visible)
                return;

            mouseHidden = !visible;
            Program.Window.CursorVisible = visible;
            Program.Window.CursorGrabbed = !visible;

            Mouse.SetPosition(Program.Window.X + Program.Window.Width/2f, Program.Window.Y + Program.Window.Height/2f);
        }

        public Container GetInventory()
        {
            return inventory;
        }
    }
}
