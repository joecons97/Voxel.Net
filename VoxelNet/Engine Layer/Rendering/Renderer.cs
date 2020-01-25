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
        public static void Draw(Mesh mesh, Material.Material material)
        {
            if (material == null)
            {
                material = AssetDatabase.GetAsset<Material.Material>("Resources/Materials/Fallback.mat");
            }

            material.Bind();

            UniformBuffers.BindAll(material.Shader.Handle);

            mesh.VertexArray.Bind();
            mesh.IndexBuffer.Bind();

            DrawCalls++;
            GL.DrawElements(PrimitiveType.Triangles, mesh.IndexBuffer.Length, DrawElementsType.UnsignedInt, 0);

            mesh.VertexArray.Unbind();
            mesh.IndexBuffer.Unbind();
            material.Unbind();
            UniformBuffers.UnbindAll();
        }

    }
}
