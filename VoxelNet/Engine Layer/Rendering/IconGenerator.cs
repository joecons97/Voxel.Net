using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VoxelNet.Assets;
using VoxelNet.Buffers;
using VoxelNet.Buffers.Ubos;

namespace VoxelNet.Rendering
{
    public static class IconGenerator
    {
        public static void GenerateBlockItemIcons()
        {
            const int ICON_SIZE = 128;

            FrameBufferObject fbo = null;

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(0, 0, 0, 0);

            LightingUniformBufferData lighting = new LightingUniformBufferData();
            lighting.AmbientColour = new Vector4(0.5f, 0.5f, 0.5f, 1);
            lighting.SunColour = new Vector4(1, 1, 1, 1);
            lighting.SunStrength = 1;
            lighting.SunDirection = new Vector4(0.5f, 0.5f, 0.5f, 1);

            Chunk tempChunk = new Chunk(Vector2.Zero);
            tempChunk.Blocks = new Chunk.BlockState[Chunk.WIDTH, Chunk.HEIGHT, Chunk.WIDTH];

            Camera tempCamera = new Camera();
            tempCamera.ProjectionType = CameraProjectionType.Orthographic;
            tempCamera.CameraSize = new Vector2(0.8f);
            tempCamera.GenerateProjectionMatrix();
            tempCamera.Position = new Vector3(0, 0, 15f);

            UniformBuffers.DirectionLightBuffer.Update(lighting);
            tempCamera.Update();

            var items = ItemDatabase.GetItems();

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is BlockItem item)
                {
                    fbo = new FrameBufferObject(ICON_SIZE, ICON_SIZE, FBOType.None);
                    fbo.Bind();
                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    var newState = new Chunk.BlockState(0, 0, 0, tempChunk);
                    newState.id = (short) item.Block.ID;
                    tempChunk.Blocks[0, 0, 0] = newState;
                    tempChunk.GenerateMesh();

                    tempChunk.RenderForIcon();

                    //Render here...
                    Texture icon = new Texture(fbo, true);
                    AssetDatabase.RegisterAsset(icon, "GeneratedIcons/" + item.Key);

                    item.IconLocation = "GeneratedIcons/" + item.Key;
                    item.GenerateGraphics();
                }
            }

            fbo?.Unbind();

            fbo?.DisposeWithoutColorHandle();
            tempChunk.Dispose();

            GL.ClearColor(Window.CLEAR_COLOUR.X, Window.CLEAR_COLOUR.Y, Window.CLEAR_COLOUR.Z, Window.CLEAR_COLOUR.W);

            fbo = null;
            tempChunk = null;
            tempCamera = null;
        }
    }
}
