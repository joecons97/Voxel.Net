using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using VoxelNet.Assets;

namespace VoxelNet.Misc
{
    public static class Maths
    {
        public static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }
        public static Vector3 GetForwardFromRotation(Vector3 Rotation)
        {
            var quat = new Quaternion(Rotation.ToRadians()).Inverted();
            return Vector3.Transform(new Vector3(0, 0, -1), quat);
        }
        public static Vector3 GetRightFromRotation(Vector3 Rotation)
        {
            var quat = new Quaternion(Rotation.ToRadians()).Inverted();
            return Vector3.Transform(new Vector3(1, 0, 0), quat);
        }
        public static Vector3 GetUpFromRotation(Vector3 Rotation)
        {
            var quat = new Quaternion(Rotation.ToRadians()).Inverted();
            return Vector3.Transform(new Vector3(0, 1, 0), quat);
        }

        public static bool Chance(float chance)
        {
            if (chance >= 1)
                return true;

            if (chance <= 0)
                return false;

            double val = World.GetInstance().Randomizer.NextDouble();

            return val <= chance;
        }
    }
}
