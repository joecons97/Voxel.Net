using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace VoxelNet.Rendering
{
    public static class Renderer
    {
        public static void Draw(Mesh mesh)
        {
        }

        public static void Draw(IndexBuffer indexBuffer, VertexArray Vao, Shader shader)
        {
            shader.Bind();

            indexBuffer.Bind();

            Vao.Bind();

            GL.DrawElements(PrimitiveType.Triangles, indexBuffer.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}
