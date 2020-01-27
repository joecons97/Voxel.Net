using System;
using System.Linq;
using ImGuiNET;
using ImGuiNet.OpenTK;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using VoxelNet.Assets;
using VoxelNet.Buffers;
using VoxelNet.Physics;
using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;
using Vector2 = System.Numerics.Vector2;

namespace VoxelNet
{
    public class Window : GameWindow
    {
        private World world;

        private ImGuiController guiController;

        public Window(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            #if !DEBUG
                WindowState = WindowState.Fullscreen;
            #endif

            AssetDatabase.SetPack("Default");
            
            CursorVisible = false;
            CursorGrabbed = true;
            TargetUpdateFrequency = Program.Settings.FPS;
            TargetRenderFrequency = Program.Settings.FPS;
            VSync = VSyncMode.Off;
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(.39f, .58f, .92f, 1.0f);

            KeyDown += (sender, args) =>
            {
                if (args.IsRepeat)
                    return;

                var inputs = Program.Settings.Input.Settings.Where(x => x.Main.KeyButton == args.Key);
                foreach (var input in inputs)
                {
                    input.KeyDown?.Invoke();
                }
            };
            KeyUp += (sender, args) =>
            {
                var inputs = Program.Settings.Input.Settings.Where(x => x.Main.KeyButton == args.Key);
                foreach (var input in inputs)
                {
                    input.KeyUp?.Invoke();
                }
            };
            MouseDown += (sender, args) =>
            {
                var inputs = Program.Settings.Input.Settings.Where(x => x.Main.MouseButton == args.Button);
                foreach (var input in inputs)
                {
                    input.KeyDown?.Invoke();
                }
            };
            MouseUp += (sender, args) =>
            {
                var inputs = Program.Settings.Input.Settings.Where(x => x.Main.MouseButton == args.Button);
                foreach (var input in inputs)
                {
                    input.KeyUp?.Invoke();
                }
            };

            guiController = new ImGuiController(Width, Height);

            AssetDatabase.GetAsset<Material>("Resources/Materials/Fallback.mat");

            world = new World("poo", "bigduck");

            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            world?.Dispose();

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

                world.Update();
            }

            Time.GameTime += (float)e.Time;
            Time.DeltaTime = (float)e.Time;

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

            world.Render();

            world.GUI();

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
