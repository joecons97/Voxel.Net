using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet
{
    public class Plane
    {
        public Vector3 Normal { get; set; }
        public float Distance { get; set; }

        public Plane(Vector3 nrml, float dist)
        {
            Normal = nrml;
            Distance = dist;
        }

        public void Normalize()
        {
            float scale = 1 / Normal.Length;
            Normal *= scale;
            Distance *= scale;
        }

        public float Dot(Vector3 p)
        {
            return Normal.X * p.X + Normal.Y * p.Y + Normal.Z * p.Z + Distance;
        }
    }
}
