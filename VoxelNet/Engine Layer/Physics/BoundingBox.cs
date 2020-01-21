using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet
{
    public class BoundingBox
    {
        public Vector3 Min { get; }

        public Vector3 Max { get; }

        public Vector3 Size
        {
            get { return Max - Min; }
        }

        public BoundingBox(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            Min = new Vector3(minX, minY, minZ);
            Max = new Vector3(maxX, maxY, maxZ);
        }

        public bool Intersects(BoundingBox box)
        {
            return (Min.X < box.Max.X &&
                Max.X > box.Min.X &&
                Min.Y < box.Max.Y &&
                Max.Y > box.Min.Y &&
                Min.Z < box.Max.Z &&
                Max.Z > box.Min.Z);
        }
    }
}
