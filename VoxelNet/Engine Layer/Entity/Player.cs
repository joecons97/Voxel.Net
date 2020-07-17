using System;
using System.Runtime.InteropServices;
using ImGuiNET;
using OpenTK;
using OpenTK.Input;
using VoxelNet.Assets;
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

        /*RenderGUI*/
        private IntPtr inventorySlotHandle;

        private static bool controlsEnabled = true;
        private static bool mouseHidden = true;

        private Inventory inventory;

        public override void Begin()
        {
            Name = "Player";
            currentWorld = World.GetInstance();
            hasHadInitialSet = false;

            inventory = new Inventory();

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
                        chunk.PlaceBlock((int)op.PlacementPosition.X, (int)op.PlacementPosition.Y, (int)op.PlacementPosition.Z, 1);

                        currentWorld.RequestChunkUpdate(chunk);
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

                        currentWorld.RequestChunkUpdate(chunk);
                        var chunkWp = (op.ChunkPosition * Chunk.WIDTH);
                        var wp = new Vector3(chunkWp.X, 0, chunkWp.Y) + op.BlockPosition;
                        BlockDatabase.GetBlock(op.BlockID).OnBreak(wp, op.ChunkPosition);
                    }
                }
            };

            Input.Input.GetSetting("Inventory").KeyDown += () =>
            {
                inventory.SetIsOpen(!inventory.IsOpen);
                SetControlsActive(!inventory.IsOpen);
                SetMouseVisible(inventory.IsOpen);
            };

            Input.Input.GetSetting("Sprint").KeyDown += () => isSprinting = true;
            Input.Input.GetSetting("Sprint").KeyUp += () => isSprinting = false;

            inventorySlotHandle = (IntPtr)AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/Inventory_Slot.png").Handle;
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

                currentWorld.WorldCamera.Position = Position + new Vector3(0, 1.7f, 0);

                float x = Input.Input.GetMouseDelta().X / 20f;
                float y = Input.Input.GetMouseDelta().Y / 20f;

                Rotation = new Vector3(0, Rotation.Y + x, 0);
                currentWorld.WorldCamera.Rotation = new Vector3(currentWorld.WorldCamera.Rotation.X + y, Rotation.Y, 0);
            }

            rigidbody.Velocity = new Vector3(vel.X, rigidbody.Velocity.Y, vel.Z);
        }

        public override void Update()
        {
            if (!hasHadInitialSet && currentWorld.TryGetChunkAtPosition((int)GetChunk().X, (int)GetChunk().Y, out Chunk c))
            {
                Position.Y = c.GetHeightAtBlock((int)GetPositionInChunk().X, (int)GetPositionInChunk().Z) + 5;
                if(Raycast.CastVoxel(currentWorld.WorldCamera.Position + new Vector3(0, 2000,0), new Vector3(0,-1,0), 5000, out RayVoxelOut hit))
                {
                    var chunkWp = (hit.ChunkPosition * Chunk.WIDTH);
                    Position.Y = chunkWp.Y + 1;
                    Debug.Log("Hit block for y pos " + Position.Y);
                }
                hasHadInitialSet = true;

                rigidbody = new Rigidbody(this, 70, new BoundingBox(-0.25f, 0.25f, 0, 2, -0.25f, 0.25f));
            }

            if (hasHadInitialSet)
                HandleInput();

            var chunkPos = Position.ToChunkPosition();
            if (World.GetInstance().TryGetChunkAtPosition((int)chunkPos.X, (int)chunkPos.Z, out Chunk chunk))
            {
                var block = Position.ToChunkSpaceFloored();
                isInWater = chunk.GetBlockID((int) block.X, (int) block.Y, (int) block.Z) == GameBlocks.WATER.ID;
                rigidbody.Drag = isInWater ? UnderWaterDrag : 0;
            }

        }

        public override void RenderGUI()
        {
            var rect = new Rect(8, 8, 256, 32);
            GUI.Label($"{(int)Time.FramesPerSecond}fps\n", rect);

            GUI.Image(currentWorld.TexturePack.Crosshair, new Rect((Program.Window.Width / 2) - 16, (Program.Window.Height / 2) - 16, 32, 32));
               
            /*Toolbar*/
            {
                ImGui.Begin("Toolbar", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.NoMove);
                ImGui.SetWindowPos(new Vector2(Program.Window.Width/2 - (ImGui.GetWindowWidth()/2), Program.Window.Height - 64));
                for (int i = 0; i < 8; i++)
                {
                    ImGui.ImageButton(inventorySlotHandle, Vector2.One*32, Vector2.Zero, Vector2.One, 0);
                    //ImGui.Button((i + 1).ToString(), Vector2.One * 32);
                   
                    ImGui.SameLine();
                }

                ImGui.End();
            }
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
    }
}
