using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Physics;

namespace VoxelNet.Rendering
{
    public class VertexContainer
    {
        public virtual int[] ElementCount { get; } = {3, 2};

        protected List<float> elements = new List<float>();

        protected BoundingBox bounds = new BoundingBox(0,0,0,0,0,0);

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

        public BoundingBox GetBoundingBox()
        {
            return bounds;
        }

        protected void RecalculateBounds(Vector3 position)
        {
            if (position.X > bounds.Max.X)
                bounds.Max = new Vector3(position.X, bounds.Max.Y, bounds.Max.Z);
            if (position.Y > bounds.Max.Y)
                bounds.Max = new Vector3(bounds.Max.X, position.Y, bounds.Max.Z);
            if (position.Z > bounds.Max.Z)
                bounds.Max = new Vector3(bounds.Max.X, bounds.Max.Y, position.Z);

            if (position.X < bounds.Min.X)
                bounds.Min = new Vector3(position.X, bounds.Min.Y, bounds.Min.Z);
            if (position.Y < bounds.Min.Y)
                bounds.Min = new Vector3(bounds.Min.X, position.Y, bounds.Min.Z);
            if (position.Z < bounds.Min.Z)
                bounds.Min = new Vector3(bounds.Min.X, bounds.Min.Y, position.Z);

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

                RecalculateBounds(position);

                elements.Add(uvs[index].X);
                elements.Add(uvs[index].Y);

                index++;
            }
        }
    }
}
