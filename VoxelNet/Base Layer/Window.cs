using System;
using ImGuiNet.OpenTK;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using VoxelNet.Assets;
using VoxelNet.Buffers;
using VoxelNet.Buffers.Ubos;
using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;
using VoxelNet.Menus;
using VoxelNet.Physics;
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
            #if !DEBUG
                WindowState = WindowState.Fullscreen;
            #endif

            GameBlocks.Init();
            //Debug.Log(GameBlocks.DIRT.Key);
            AssetDatabase.SetPack(AssetDatabase.DEFAULTPACK);
            
            TargetUpdateFrequency = Program.Settings.FPS;
            TargetRenderFrequency = Program.Settings.FPS;
            VSync = VSyncMode.Off;
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(.39f, .58f, .92f, 1.0f);

            guiController = new ImGuiController(Width, Height);

            AssetDatabase.GetAsset<Material>("Resources/Materials/Fallback.mat");

            new TestMenu().Show();

            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            World.GetInstance()?.Dispose();

            AssetDatabase.Dispose();

            guiController?.Dispose();

            UniformBuffers.Dispose();

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
            
            Renderer.DrawCalls = 0;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Enable(EnableCap.DepthTest);

            World.GetInstance()?.Render();
            World.GetInstance()?.GUI();

            Renderer.DrawQueue();

            Menu.GUIAll();

            guiController.Render();
            
            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0,0,Width, Height);
            guiController.WindowResized(Width,Height);
            base.OnResize(e);
        }
    }
}
