using System;
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
            CursorGrabbed = true;
            CursorVisible = false;
            TargetRenderFrequency = 120;
            VSync = VSyncMode.Off;
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(.39f, .58f, .92f, 1.0f);

            guiController = new ImGuiController(Width, Height);

            AssetDatabase.GetAsset<Material>("Resources/Materials/Fallback.mat");

            world = new World("poo", "poohead");

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
                KeyboardState kbdState = Keyboard.GetState();

                if (kbdState.IsKeyDown(Key.Escape))
                    Exit();

                world.Update();
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (Raycast.CastVoxel(world.WorldCamera.Position, world.WorldCamera.GetForward(), 5, out RayVoxelOut op))
            {
                if (e.Button == MouseButton.Left)
                {
                    if (world.TryGetChunkAtPosition((int) op.ChunkPosition.X, (int) op.ChunkPosition.Y, out Chunk chunk))
                    {
                        chunk.DestroyBlock((int) op.BlockPosition.X, (int) op.BlockPosition.Y, (int) op.BlockPosition.Z);

                        world.RequestChunkUpdate(chunk);
                    }
                }
                else
                {
                    if (world.TryGetChunkAtPosition((int)op.PlacementChunk.X, (int)op.PlacementChunk.Y, out Chunk chunk))
                    {
                        chunk.PlaceBlock((int)op.PlacementPosition.X, (int)op.PlacementPosition.Y, (int)op.PlacementPosition.Z, 1);

                        world.RequestChunkUpdate(chunk);
                    }
                }


            }
            base.OnMouseDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            guiController.PressChar(e.KeyChar);

            base.OnKeyPress(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            guiController.Update(this, (float)e.Time);
            
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            world.Render();

            ImGui.Begin("", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.Text($"{(int)(1f/Time.DeltaTime)}fps" + $" {(int)(Time.DeltaTime * 1000f)}ms");

            ImGui.Text($"World Pos:  {world.WorldCamera.Position.ToString()}");
            ImGui.Text($"Chunk:  {world.WorldCamera.Position.ToChunkPosition().ToString()}");
            ImGui.Text($"Pos In Chunk:  {world.WorldCamera.Position.ToChunkSpace().ToString()}");

            ImGui.End();

            ImGui.Begin("crosshair", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.Image((IntPtr)world.TexturePack.Crosshair.Handle, Vector2.One*32);

            ImGui.SetWindowPos("crosshair", new Vector2(((float)Width/2f) - 16, ((float)Height /2f) - 16));

            ImGui.End();

            guiController.Render();
            
            Context.SwapBuffers();

            Time.GameTime += (float)e.Time;
            Time.DeltaTime = (float)e.Time;

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
