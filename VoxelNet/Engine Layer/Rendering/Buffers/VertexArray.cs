using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace VoxelNet.Rendering
{
    public class VertexArray : IDisposable
    {
        public int Handle { get; private set; }

        private VertexBuffer vertBuffer;

        public VertexArray(VertexBuffer vb)
        {
            Handle = GL.GenVertexArray();
            vertBuffer = vb;

            Bind();
            vertBuffer.Bind();

            int offset = 0;
            for (int i = 0; i < vb.VertexContainer.ElementCount.Length; i++)
            {
                int element = vb.VertexContainer.ElementCount[i];
                int elementSize = element * sizeof(float);
                int totalSize = vb.VertexContainer.GetTotalElementCount() * sizeof(float);

                GL.EnableVertexAttribArray(i);
                GL.VertexAttribPointer(i, element, VertexAttribPointerType.Float, false, totalSize, offset);

                offset += elementSize;
            }
        }

        public void Bind()
        {
            GL.BindVertexArray(Handle);
            //vertBuffer.Bind();
        }

        public void Unbind()
        {
            GL.BindVertexArray(0);
            //vertBuffer.Unbind();
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(Handle);
        }
    }
}
