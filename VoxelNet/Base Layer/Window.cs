using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using VoxelNet.Assets;
using VoxelNet.Buffers;
using VoxelNet.Buffers.Ubos;
using VoxelNet.Containers;
using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;
using VoxelNet.Menus;
using VoxelNet.Physics;
using VoxelNet.PostProcessing;
using Vector2 = System.Numerics.Vector2;

namespace VoxelNet
{
    public class Window : GameWindow
    {
        public static int WindowWidth { get; private set; }
        public static int WindowHeight { get; private set; }

        public static readonly Vector4 CLEAR_COLOUR = new Vector4(.39f, .58f, .92f, 1.0f);

        public static bool IsLoadingDone { get; private set; }

        public MainMenu MainMenu { get; private set; }

        public Window(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            IsLoadingDone = false;
            AssetDatabase.SetPack(AssetDatabase.DEFAULTPACK);

            GameBlocks.Init();
            GameItems.Init();

            CraftingRecipeDatabase.Init();

            VSync = VSyncMode.Off;
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.FramebufferSrgb);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(CLEAR_COLOUR.X, CLEAR_COLOUR.Y, CLEAR_COLOUR.Z, CLEAR_COLOUR.Z);

            AssetDatabase.GetAsset<Material>("Resources/Materials/Fallback.mat");

            MainMenu = new MainMenu();
            MainMenu.Show();

            Program.Settings.UpdateAll();


            //Load texture pack before generating icons
            AssetDatabase.GetAsset<TexturePack>("");

            IconGenerator.GenerateBlockItemIcons();

            WindowWidth = Program.Settings.WindowWidth;
            WindowHeight = Program.Settings.WindowHeight;

            PostProcessingEffects.RegisterEffect(new Bloom());
            PostProcessingEffects.RegisterEffect(new ACESTonemapEffect());

            IsLoadingDone = true;

            OnResize(null);

            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            World.GetInstance()?.Dispose();

            AssetDatabase.Dispose();

            UniformBuffers.Dispose();

            PostProcessingEffects.Dispose();

            base.OnUnload(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Time.GameTime += (float)e.Time;
            Time.DeltaTime = (float)e.Time;

            Time.UpdateFrameRate(1f / Time.DeltaTime);

            if (IsLoadingDone)
            {
                WindowWidth = this.Width;
                WindowHeight = this.Height;

                Input.Input.Update();

                if (World.GetInstance() != null)
                {
                    World.GetInstance().Update();

                    PhysicSimulation.Simulate(Time.DeltaTime);
                }

                if (Focused)
                {
                    KeyboardState kbdState = Keyboard.GetState();

                    if (kbdState.IsKeyDown(Key.F4) && kbdState.IsKeyDown(Key.AltLeft))
                        Exit();

                }

                UniformBuffers.TimeBuffer.Update(new TimeUniformBuffer()
                    {DeltaTime = Time.DeltaTime, Time = Time.GameTime});
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (IsLoadingDone)
            {
                GUI.NewFrame();

                Renderer.ClippedCount = 0;
                Renderer.DrawCalls = 0;

                World.GetInstance()?.Render();

                Renderer.DrawQueue();

                PostProcessingEffects.RenderEffects();

                World.GetInstance()?.RenderGUI();
                ContainerRenderer.RenderGUI();
                Menu.RenderGUI();

                Input.Input.PostRenderUpdate();

                GUI.EndFrame();
            }

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (IsLoadingDone)
            {
                GL.Viewport(ClientRectangle);
            }

            base.OnResize(e);
        }
    }
}
