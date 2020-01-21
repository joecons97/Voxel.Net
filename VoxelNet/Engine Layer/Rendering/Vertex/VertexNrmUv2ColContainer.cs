using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet.Rendering
{
    public class VertexNrmUv2ColContainer : VertexContainer
    {
        public override int[] ElementCount => new[] {3, 2, 3, 2, 4};

        public VertexNrmUv2ColContainer(Vector3[] positions, Vector2[] uvs, Vector3[] normals, Vector2[] uv2, Vector4[] col)
        {
            if (positions.Length != uvs.Length)
                Debug.Assert("Vertex position array is not of the same length as vertex UV array!");

            int index = 0;
            foreach (var position in positions)
            {
                elements.Add(position.X);
                elements.Add(position.Y);
                elements.Add(position.Z);

                elements.Add(uvs[index].X);
                elements.Add(uvs[index].Y);

                elements.Add(normals[index].X);
                elements.Add(normals[index].Y);
                elements.Add(normals[index].Z);

                elements.Add(uv2[index].X);
                elements.Add(uv2[index].Y);

                elements.Add(col[index].X);
                elements.Add(col[index].Y);
                elements.Add(col[index].Z);
                elements.Add(col[index].W);

                index++;
            }
        }
    }
}
