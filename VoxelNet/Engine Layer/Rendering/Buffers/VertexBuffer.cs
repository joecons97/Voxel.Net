using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace VoxelNet.Rendering
{
    public class VertexBuffer : IDisposable
    {
        public int Handle { get; private set; }

        public VertexContainer VertexContainer { get; private set; }

        public VertexBuffer(VertexContainer verticesVertexContainer)
        {
            Handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);

            VertexContainer = verticesVertexContainer;

            GL.BufferData(BufferTarget.ArrayBuffer, VertexContainer.GetLength() * sizeof(float), VertexContainer.GetElements(), BufferUsageHint.StaticDraw);
        }
        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
        }

        public void Dispose()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(Handle);
            GC.SuppressFinalize(this);
        }
    }
}
