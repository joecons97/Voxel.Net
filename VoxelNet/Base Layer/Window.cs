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
            GL.Enable(EnableCap.FramebufferSrgb);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(.39f, .58f, .92f, 1.0f);

            PostProcessingEffects.RegisterEffect(new Bloom());
            PostProcessingEffects.RegisterEffect(new ACESTonemapEffect());

            AssetDatabase.GetAsset<Material>("Resources/Materials/Fallback.mat");

            new TestMenu().Show();

            Program.Settings.UpdateAll();

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

            if (Focused)
            {
                Input.Input.Update();

                KeyboardState kbdState = Keyboard.GetState();

                if (kbdState.IsKeyDown(Key.F4) && kbdState.IsKeyDown(Key.AltLeft))
                    Exit();

                World.GetInstance()?.Update();

                PhysicSimulation.Simulate();
            }

            UniformBuffers.TimeBuffer.Update(new TimeUniformBuffer(){DeltaTime = Time.DeltaTime, Time = Time.GameTime});

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
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

            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle);
            base.OnResize(e);
        }
    }
}
