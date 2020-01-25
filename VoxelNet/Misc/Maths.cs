using System;
using System.Collections.Generic;
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
            float yaw = MathHelper.DegreesToRadians(Rotation.Y + 90);
            float pitch = MathHelper.DegreesToRadians(Rotation.X);

            float x = (float)(Math.Cos(yaw) * Math.Cos(pitch));
            float y = (float)Math.Sin(pitch);
            float z = (float)(Math.Cos(pitch) * Math.Sin(yaw));

            return new Vector3(-x, -y, -z).Normalized();
        }
        public static Vector3 GetRightFromRotation(Vector3 Rotation)
        {
            float yaw = MathHelper.DegreesToRadians(Rotation.Y);

            float x = (float)Math.Cos(yaw);
            float z = (float)Math.Sin(yaw);

            return new Vector3(x, 0, z);
        }
        public static Vector3 GetUpFromRotation(Vector3 Rotation)
        {
            float pitch = MathHelper.DegreesToRadians(Rotation.X + 90);

            float y = (float)Math.Sin(pitch);

            return new Vector3(0, y, 0);
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
