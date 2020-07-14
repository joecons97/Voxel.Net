using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet.Rendering
{
    public class VertexNormalContainer : VertexContainer
    {
        public override int[] ElementCount => new[] {3, 2, 3};

        public VertexNormalContainer(Vector3[] positions, Vector2[] uvs, Vector3[] normals)
        {
            if (positions.Length != uvs.Length)
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

                elements.Add(normals[index].X);
                elements.Add(normals[index].Y);
                elements.Add(normals[index].Z);

                index++;
            }
        }
    }
}
