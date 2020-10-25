using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Input;
using VoxelNet.Assets;
using VoxelNet.Containers;
using VoxelNet.Entities.Interfaces;
using VoxelNet.Menus;
using VoxelNet.Physics;
using VoxelNet.Rendering;
using Vector2 = System.Numerics.Vector2;

namespace VoxelNet.Entities
{
    public class Player : Entity, IDamageable
    {
        public const int MAX_HEALTH = 20;
        public const int MAX_HUNGER = 20;
        private float currentHealth = MAX_HEALTH;

        private float currentHunger = MAX_HUNGER;

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

        CraftingContainer craftingInventory = new CraftingContainer();
        PlayerInventory inventory = new PlayerInventory();
        PauseMenu pauseMenu = new PauseMenu();
        private Texture heartIcon;
        private Texture heartHalfIcon;
        private Texture heartEmptyIcon;

        private Texture hungerIcon;
        private Texture hungerHalfIcon;
        private Texture hungerEmptyIcon;

        private float hungerLossTickRate = .5f;
        private float hungerLossAmount = 0.01f;
        private float lastHungerLossTick;
        private float healthIncreaseTickRate = 4;
        private float lastHealthIncreaseTick;

        public override void Begin()
        {
            Name = "Player";
            currentWorld = World.GetInstance();
            hasHadInitialSet = false;

            heartIcon = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/heart.png");
            heartHalfIcon = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/heart_half.png");
            heartEmptyIcon = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/heart_empty.png");

            hungerIcon = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/hunger.png");
            hungerHalfIcon = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/hunger_half.png");
            hungerEmptyIcon = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/hunger_empty.png");

            Input.Input.GetSetting("Pause").KeyDown += InputPause;

            Input.Input.GetSetting("Jump").KeyDown += InputJump;

            Input.Input.GetSetting("Interact").KeyDown += InputInteract;

            Input.Input.GetSetting("Destroy Block").KeyDown += InputDestroyBlock;

            Input.Input.GetSetting("Inventory").KeyDown += InputInventory;

            Program.Window.MouseWheel += InputMouseWheel;

            Input.Input.GetSetting("Sprint").KeyDown += InputSprintDown;
            Input.Input.GetSetting("Sprint").KeyUp += InputSprintUp;
        }

        public override void Destroyed()
        {
            Input.Input.GetSetting("Pause").KeyDown -= InputPause;

            Input.Input.GetSetting("Jump").KeyDown -= InputJump;

            Input.Input.GetSetting("Interact").KeyDown -= InputInteract;

            Input.Input.GetSetting("Destroy Block").KeyDown -= InputDestroyBlock;

            Input.Input.GetSetting("Inventory").KeyDown -= InputInventory;

            Program.Window.MouseWheel -= InputMouseWheel;

            Input.Input.GetSetting("Sprint").KeyDown -= InputSprintDown;
            Input.Input.GetSetting("Sprint").KeyUp -= InputSprintUp;

            heartEmptyIcon.Dispose();
            heartHalfIcon.Dispose();
            heartIcon.Dispose();

            hungerHalfIcon.Dispose();
            hungerEmptyIcon.Dispose();
            hungerIcon.Dispose();

            base.Destroyed();
        }

        private void InputMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                inventory.SelectedItemIndex--;
            else
                inventory.SelectedItemIndex++;

            if (inventory.SelectedItemIndex > inventory.ContainerSize.X - 1)
                inventory.SelectedItemIndex = 0;
            else if (inventory.SelectedItemIndex < 0)
                inventory.SelectedItemIndex = (int)inventory.ContainerSize.X - 1;
        }

        void InputSprintDown()
        {
            isSprinting = true;
        }

        void InputSprintUp()
        {
            isSprinting = false;
        }

        void InputInventory()
        {
            if (inventory.IsOpen)
            {
                inventory.Close();
                craftingInventory.Close();
            }
            else
            {
                inventory.Open();
                craftingInventory.Open();
            }

            SetMouseVisible(inventory.IsOpen);
            SetControlsActive(!inventory.IsOpen);
        }

        void InputPause()
        {
            if (!currentWorld.HasFinishedInitialLoading)
                return;

            if (inventory.IsOpen)
            {
                inventory.Close();
                SetControlsActive(true);
                SetMouseVisible(false);
            }
            else if (pauseMenu.IsOpen)
            {
                pauseMenu.Close();
                SetControlsActive(true);
                SetMouseVisible(false);
            }
            else
                pauseMenu.Show();
        }

        void InputJump()
        {
            if (!controlsEnabled || isInWater)
                return;

            if (Raycast.CastVoxel(currentWorld.WorldCamera.Position, new Vector3(0, -1, 0), 2.1f, out RayVoxelOut output))
                rigidbody.AddImpluse(new Vector3(0, 1, 0) * 600);
        }

        void InputInteract()
        {
            if (!controlsEnabled)
                return;

            if (Raycast.CastVoxel(currentWorld.WorldCamera.Position, currentWorld.WorldCamera.GetForward(), 5,
                out RayVoxelOut op))
            {
                int x = (int)Math.Floor(GetPositionInChunk().X);
                int z = (int)Math.Floor(GetPositionInChunk().Z);
                bool isPlayerAtPos = (int) op.PlacementPosition.X == x && (int) op.PlacementPosition.Z == z;
                if (currentWorld.TryGetChunkAtPosition((int)op.PlacementChunk.X, (int)op.PlacementChunk.Y, out Chunk chunk) &&
                    !isPlayerAtPos)
                {
                    var stack = inventory.GetItemStackByLocation(inventory.SelectedItemIndex, 0);
                    if (stack != null)
                    {
                        stack.Item.OnInteract(op.PlacementPosition, chunk);
                        inventory.RemoveItemFromStack(stack.Item, stack);
                        currentWorld.RequestChunkUpdate(chunk, true, (int)op.BlockPosition.X, (int)op.BlockPosition.Z);
                    }
                }
            }
        }

        void InputDestroyBlock()
        {
            if (!controlsEnabled)
                return;

            if (Raycast.CastVoxel(currentWorld.WorldCamera.Position, currentWorld.WorldCamera.GetForward(), 5,
                out RayVoxelOut op))
            {
                if (currentWorld.TryGetChunkAtPosition((int)op.ChunkPosition.X, (int)op.ChunkPosition.Y,
                    out Chunk chunk))
                {
                    chunk.DestroyBlock((int)op.BlockPosition.X, (int)op.BlockPosition.Y, (int)op.BlockPosition.Z);

                    var chunkWp = (op.ChunkPosition * Chunk.WIDTH);
                    var wp = new Vector3(chunkWp.X, 0, chunkWp.Y) + op.BlockPosition;
                    BlockDatabase.GetBlock(op.BlockID).OnBreak(wp, op.ChunkPosition);

                    currentWorld.RequestChunkUpdate(chunk, true, (int)op.BlockPosition.X, (int)op.BlockPosition.Z);
                }
            }
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

                if (y > 0 && currentWorld.WorldCamera.Rotation.X >= 90)
                    y = 0;
                else if (y < 0 && currentWorld.WorldCamera.Rotation.X <= -85)
                    y = 0;

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

            if (currentWorld.HasFinishedInitialLoading)
            {
                if (lastHungerLossTick + hungerLossTickRate <= Time.GameTime)
                {
                    hungerLossAmount = isSprinting ? 0.25f : 0.01f;

                    if(currentHunger > 0)
                        currentHunger -= hungerLossAmount;
                    else
                    {
                        currentHealth -= 0.0625f;
                        TakeDamage(0);
                    }
                    lastHungerLossTick = Time.GameTime;
                }

                if (lastHealthIncreaseTick + healthIncreaseTickRate <= Time.GameTime)
                {
                    if (currentHealth < MAX_HEALTH && currentHunger == MAX_HUNGER)
                        SetHealth((int)currentHealth + 1);

                    lastHealthIncreaseTick = Time.GameTime;
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

            float winWidth = Window.WindowWidth;
            float winHeight = Window.WindowHeight;
            int size = 23;
            for (int i = 0; i < MAX_HEALTH; i++)
            {
                int forX = size * (i / 2);
                int curHealth = (int)Math.Ceiling(currentHealth);

                if (i > curHealth)
                    GUI.Image(heartEmptyIcon, new Rect((winWidth / 2) - 240 + forX, winHeight - 110, size, size));

                if (i % 2 != 0)
                {
                    if (i == curHealth)
                    {
                        GUI.Image(heartHalfIcon, new Rect((winWidth / 2) - 240 + forX, winHeight - 110, size, size));
                    }
                }
                else
                {
                    if (i <= curHealth)
                    {
                        GUI.Image(heartIcon, new Rect((winWidth / 2) - 240 + forX, winHeight - 110, size, size));
                    }
                }
            }

            for (int i = MAX_HUNGER; i > 0; i--)
            {
                int forX = size * ((MAX_HUNGER - i) / 2) - (size/4);
                int curHunger = (int)Math.Ceiling(currentHunger);

                if (i > curHunger)
                    GUI.Image(hungerEmptyIcon, new Rect((winWidth / 2) + forX, winHeight - 110, size, size));

                if (i % 2 != 0)
                {
                    if (i == curHunger)
                    {
                        GUI.Image(hungerHalfIcon, new Rect((winWidth / 2) + forX, winHeight - 110, size, size));
                    }
                }
                else
                {
                    if (i <= curHunger)
                    {
                        GUI.Image(hungerIcon, new Rect((winWidth / 2) + forX, winHeight - 110, size, size));
                    }
                }
            }
        }

        public override void OnPreVoxelCollisionEnter()
        {
            //Fall damage here...
        }

        public static void SetControlsActive(bool active)
        {
            controlsEnabled = active;
        }

        public static void SetMouseVisible(bool visible, bool resetPos = true)
        {
            if (mouseHidden != visible)
                return;

            mouseHidden = !visible;
            Program.Window.CursorVisible = visible;
            Program.Window.CursorGrabbed = !visible;

            if(resetPos)
                Mouse.SetPosition(Program.Window.X + Program.Window.Width/2f, Program.Window.Y + Program.Window.Height/2f);
        }

        public Container GetInventory()
        {
            return inventory;
        }

        public void Die()
        {
            //The player died
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        public void SetHealth(int health)
        {
            currentHealth = health;
        }

        public int GetHealth() => (int)Math.Ceiling(currentHealth);
    }
}
