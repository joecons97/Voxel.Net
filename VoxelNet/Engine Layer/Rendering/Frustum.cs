using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Physics;
using VoxelNet.Rendering;

namespace VoxelNet
{
    public class Frustum
    {
        private const byte NEAR = 0;
        private const byte FAR = 1;
        private const byte LEFT = 2;
        private const byte RIGHT = 3;
        private const byte UP = 4;
        private const byte DOWN = 5;


        Plane[] planes = new Plane[6];

        public void UpdateMatrix(Matrix4 m)
        {
           planes[LEFT].Normal = new Vector3(
                m.M14 + m.M11,
                m.M24 + m.M21,
                m.M34 + m.M31);
            planes[LEFT].Distance = m.M44 + m.M41;
            planes[LEFT].Normalize();

            planes[RIGHT].Normal = new Vector3(
                m.M14 - m.M11,
                m.M24 - m.M21,
                m.M34 - m.M31);
            planes[RIGHT].Distance = m.M44 - m.M41;
            planes[RIGHT].Normalize();

            planes[UP].Normal = new Vector3(
                m.M14 - m.M12,
                m.M24 - m.M22,
                m.M34 - m.M32);
            planes[UP].Distance = m.M44 - m.M42;
            planes[UP].Normalize();

            planes[DOWN].Normal = new Vector3(
                m.M14 + m.M12,
                m.M24 + m.M22,
                m.M34 + m.M32);
            planes[DOWN].Distance = m.M44 + m.M42;
            planes[DOWN].Normalize();

            planes[NEAR].Normal = new Vector3(
                m.M13,
                m.M23,
                m.M33);
            planes[NEAR].Distance = m.M43;
            planes[NEAR].Normalize();

            planes[FAR].Normal = new Vector3(
                m.M14 - m.M13,
                m.M24 - m.M23,
                m.M34 - m.M33);
            planes[FAR].Distance = m.M44 - m.M43;
            planes[FAR].Normalize();
            
        }

        public Frustum(Matrix4 m)
        {
            for (int i = 0; i < 6; i++)
            {
                planes[i] = new Plane(Vector3.Zero, 0);
            }

            UpdateMatrix(m);
        }

        public bool Intersects(Vector3 p)
        {
            bool r = true;
            for (int i = 0; i < 6; i++)
            {
                if (planes[i].Dot(p) < 0)
                    r = false;
            }

            return r;
        }

        public bool Intersects(BoundingBox box)
        {
            for (int i = 0; i < 6; i++)
            {
                if (planes[i].Dot(box.Min) >= 0) continue;
                if (planes[i].Dot(new Vector3(box.Max.X, box.Min.Y, box.Min.Z)) >= 0) continue;
                if (planes[i].Dot(new Vector3(box.Min.X, box.Max.Y, box.Min.Z)) >= 0) continue;
                if (planes[i].Dot(new Vector3(box.Min.X, box.Min.Y, box.Max.Z)) >= 0) continue;
                if (planes[i].Dot(new Vector3(box.Max.X, box.Max.Y, box.Min.Z)) >= 0) continue;
                if (planes[i].Dot(new Vector3(box.Max.X, box.Min.Y, box.Max.Z)) >= 0) continue;
                if (planes[i].Dot(new Vector3(box.Max.X, box.Min.Y, box.Max.Z)) >= 0) continue;
                if (planes[i].Dot(new Vector3(box.Min.X, box.Max.Y, box.Max.Z)) >= 0) continue;
                if (planes[i].Dot(box.Max) >= 0) continue;

                return false;
            }

            return true;
        }
    }
}
