using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet.Rendering
{
    public class VertexBlockContainer : VertexContainer
    {
        public override int[] ElementCount => new[] {3, 2, 3, 2, 4, 1};

        public Vector3[] positions;
        public Vector2[] uvs;
        public Vector3[] normals;
        public Vector2[] uv2;
        public Vector4[] col;
        public float[] lighting
        {
            get { return _lighting; }
            set
            {
                _lighting = value;
                UpdateValues();
            }
        }
        private float[] _lighting;

        void UpdateValues()
        {
            elements.Clear();
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

                elements.Add(uv2[index].X);
                elements.Add(uv2[index].Y);

                elements.Add(col[index].X);
                elements.Add(col[index].Y);
                elements.Add(col[index].Z);
                elements.Add(col[index].W);

                elements.Add(lighting[index]);

                index++;
            }
        }

        public VertexBlockContainer(Vector3[] positions, Vector2[] uvs, Vector3[] normals, Vector2[] uv2, Vector4[] col, float[] lighting)
        {
            if (positions.Length != uvs.Length)
                Debug.Assert("Vertex position array is not of the same length as vertex UV array!");

            this.positions = positions;
            this.uvs = uvs;
            this.normals = normals;
            this.uv2 = uv2;
            this.col = col;

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

                elements.Add(uv2[index].X);
                elements.Add(uv2[index].Y);

                elements.Add(col[index].X);
                elements.Add(col[index].Y);
                elements.Add(col[index].Z);
                elements.Add(col[index].W);

                elements.Add(lighting[index]);

                index++;
            }
        }
    }
}
