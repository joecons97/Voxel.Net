using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace VoxelNet.Rendering
{
    public class IndexBuffer : IDisposable
    {
        public int Handle { get; private set; }

        public int Length { get; private set; }

        private uint[] indices;

        public IndexBuffer(uint[] indices)
        {
            Length = indices.Length;

            Handle = GL.GenBuffer();

            this.indices = indices;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Length * sizeof(uint), this.indices, BufferUsageHint.StaticDraw);
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
        }

        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Dispose()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DeleteBuffer(Handle);
            //GC.SuppressFinalize(this);
        }
    }
}
