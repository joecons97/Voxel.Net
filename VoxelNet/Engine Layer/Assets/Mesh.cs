using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelNet.Rendering
{
    public class Mesh : IDisposable
    {
        public IndexBuffer IndexBuffer {get; private set; }
        public VertexBuffer VertexBuffer { get; private set; }
        public VertexArray VertexArray { get; private set; }

        //Material...

        public Mesh(VertexContainer verticesContainer, uint[] indices)
        {
            VertexBuffer = new VertexBuffer(verticesContainer);

            VertexArray = new VertexArray(VertexBuffer);

            IndexBuffer = new IndexBuffer(indices);
        }

        public void Draw()
        {
            Renderer.Draw(this);
        }

        public void Dispose()
        {
            VertexBuffer?.Dispose();
            VertexArray?.Dispose();
            IndexBuffer?.Dispose();
        }
    }
}
