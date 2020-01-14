using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet.Rendering
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Vector3 GetForward()
        {
            float yaw = MathHelper.DegreesToRadians(Rotation.Y + 90);
            float pitch = MathHelper.DegreesToRadians(Rotation.X);

            float x = (float) (Math.Cos(yaw) * Math.Cos(pitch));
            float y = (float)Math.Sin(pitch);
            float z = (float)(Math.Cos(pitch) * Math.Sin(yaw));

            return new Vector3(-x, -y, -z);
        }

        public Vector3 GetRight()
        {
            float yaw = MathHelper.DegreesToRadians(Rotation.Y);

            float x = (float)Math.Cos(yaw);
            float z = (float)Math.Sin(yaw);

            return new Vector3(x, 0, z);
        }

        public Vector3 GetUp()
        {
            float pitch = MathHelper.DegreesToRadians(Rotation.X + 90);
            float yaw = MathHelper.DegreesToRadians(Rotation.Y);

            float x = 0;//(float)(Math.Cos(pitch) * Math.Sin(yaw));
            float y = (float)Math.Sin(pitch);
            float z = 0;//(float)(Math.Cos(pitch) * Math.Sin(yaw));

            return new Vector3(-x, y, -z);
        }
    }
}
