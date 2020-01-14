using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet.Rendering
{
    public class VertexContainer
    {
        public virtual int[] ElementCount { get; } = {3, 2};

        protected List<float> elements = new List<float>();

        public float[] GetElements()
        {
            return elements.ToArray();
        }

        public int GetTotalElementCount()
        {
            int val = 0;
            for (int i = 0; i < ElementCount.Length; i++)
            {
                val += ElementCount[i];
            }

            return val;
        }

        public int GetLength()
        {
            return elements.Count;
        }

        public VertexContainer() { }

        public VertexContainer(Vector3[] positions, Vector2[] uvs)
        {
            if(positions.Length != uvs.Length)
                Debug.Assert("Vertex position array is not of the same length as vertex UV array!");

            int index = 0;
            foreach (var position in positions)
            {
                elements.Add(position.X);
                elements.Add(position.Y);
                elements.Add(position.Z);

                elements.Add(uvs[index].X);
                elements.Add(uvs[index].Y);

                index++;
            }
        }
    }
}
