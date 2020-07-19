using System;
using ImGuiNet.OpenTK;
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
        //private World world;

        private ImGuiController guiController;

        public Window(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            AssetDatabase.SetPack(AssetDatabase.DEFAULTPACK);

            GameBlocks.Init();
            GameItems.Init();

            VSync = VSyncMode.Off;
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(.39f, .58f, .92f, 1.0f);

            PostProcessingEffects.RegisterEffect(new Bloom());
            PostProcessingEffects.RegisterEffect(new ACESTonemapEffect());

            guiController = new ImGuiController(Width, Height);

            AssetDatabase.GetAsset<Material>("Resources/Materials/Fallback.mat");

            new TestMenu().Show();

            Program.Settings.UpdateAll();

            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            World.GetInstance()?.Dispose();

            AssetDatabase.Dispose();

            guiController?.Dispose();

            UniformBuffers.Dispose();

            PostProcessingEffects.Dispose();

            base.OnUnload(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Focused)
            {
                Input.Input.Update();

                KeyboardState kbdState = Keyboard.GetState();

                if (kbdState.IsKeyDown(Key.Escape))
                    Exit();

                World.GetInstance()?.Update();

                PhysicSimulation.Simulate(Time.DeltaTime);
            }

            Time.GameTime += (float)e.Time;
            Time.DeltaTime = (float)e.Time;

            Time.UpdateFrameRate(1f / Time.DeltaTime);

            UniformBuffers.TimeBuffer.Update(new TimeUniformBuffer(){DeltaTime = Time.DeltaTime, Time = Time.GameTime});

            base.OnUpdateFrame(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            guiController.PressChar(e.KeyChar);

            base.OnKeyPress(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            guiController.Update(this, (float)e.Time);
            GUI.NewFrame();

            Renderer.ClippedCount = 0;
            Renderer.DrawCalls = 0;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Enable(EnableCap.DepthTest);

            World.GetInstance()?.Render();

            PostProcessingEffects.BeginPostProcessing();

            Renderer.DrawQueue();

            PostProcessingEffects.EndPostProcessing();

            PostProcessingEffects.RenderEffects();

            World.GetInstance()?.RenderGUI();
            ContainerRenderer.RenderGUI();
            Menu.RenderGUI();

            Input.Input.PostRenderUpdate();

            guiController.Render();

            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle);
            guiController.WindowResized(ClientSize.Width, ClientSize.Height);
            base.OnResize(e);
        }
    }
}
