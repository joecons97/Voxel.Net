using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace VoxelNet.Buffers
{
    public class UniformBuffer<T> where T : struct
    {
        protected static int TotalUBOs = 0;

        public int Handle { get; private set; }

        private T data;
        private int size;
        private int port;
        private string name;

        public UniformBuffer(T Data, string blockName)
        {
            Handle = GL.GenBuffer();

            name = blockName;
            size = Marshal.SizeOf<T>();

            port = TotalUBOs;
            TotalUBOs++;

            Update(data);
        }

        public void Update(T Data)
        {
            data = Data;
            GL.BindBuffer(BufferTarget.UniformBuffer, Handle);
            GL.BufferData(BufferTarget.UniformBuffer, size, ref data, BufferUsageHint.StaticDraw);
        }

        public void Bind(int program)
        {
            int blockIndex = GL.GetUniformBlockIndex(program, name);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, port, Handle);
            GL.UniformBlockBinding(program, blockIndex, port);
        }

        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(Handle);
        }
    }
}
