using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VoxelNet.Assets;
using VoxelNet.Buffers;

namespace VoxelNet.Rendering
{
    public static class Renderer
    {
        public static int DrawCalls { get; set; }

        static List<QueuedDraw> drawQueue = new List<QueuedDraw>();

        public static void DrawRequest(Mesh mesh, Material.Material material, Matrix4 worldMatrix = default)
        {
            if(material == null)
                material = AssetDatabase.GetAsset<Material.Material>("Resources/Materials/Fallback.mat");

            if(worldMatrix == default)
                worldMatrix = Matrix4.Identity;

            if (material.Shader.IsTransparent)
            {
                drawQueue.Add(new QueuedDraw()
                {
                    mesh = mesh,
                    material = material,
                    worldMatrix = worldMatrix
                });
            }
            else
            {
                drawQueue.Insert(0, new QueuedDraw()
                {
                    mesh = mesh,
                    material = material,
                    worldMatrix = worldMatrix
                });
            }
            DrawCalls = drawQueue.Count;
        }

        public static void DrawQueue()
        {
            for (int i = 0; i < drawQueue.Count; i++)
            {
                if (drawQueue[i].worldMatrix != default)
                    drawQueue[i].material.Shader.SetUniform("u_World", drawQueue[i].worldMatrix);

                drawQueue[i].material.Bind();

                UniformBuffers.BindAll(drawQueue[i].material.Shader.Handle);

                drawQueue[i].mesh.VertexArray.Bind();
                drawQueue[i].mesh.IndexBuffer.Bind();

                GL.DrawElements(PrimitiveType.Triangles, drawQueue[i].mesh.IndexBuffer.Length, DrawElementsType.UnsignedInt, 0);

                drawQueue[i].mesh.VertexArray.Unbind();
                drawQueue[i].mesh.IndexBuffer.Unbind();
                drawQueue[i].material.Unbind();
                UniformBuffers.UnbindAll();
            }

            drawQueue.Clear();
        }

        struct QueuedDraw
        {
            public Mesh mesh;
            public Material.Material material;
            public Matrix4 worldMatrix;
        }
    }
}
