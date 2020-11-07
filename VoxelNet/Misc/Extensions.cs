using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using VoxelNet.Assets;

namespace VoxelNet
{
    public static class Extensions
    {
        public static Vector4 ToVector4(this Color4 col)
        {
            return new Vector4(col.R, col.G, col.B, col.A);
        }

        /// <summary>
        /// Finds the position of the chunk that the vector is in
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector3 ToChunkPosition(this Vector3 vec)
        {
            Vector3 retVec = new Vector3();
            retVec.X = (float)Math.Floor(vec.X / Chunk.WIDTH);
            retVec.Y = 0f;
            retVec.Z = (float)Math.Floor(vec.Z / Chunk.WIDTH);

            return retVec;
        }

        /// <summary>
        /// Finds the position of the vector relative to the chunk it is inside
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector3 ToChunkSpace(this Vector3 vec)
        {
            var chunkPos = ToChunkPosition(vec) * Chunk.WIDTH;

            return (vec - chunkPos);
        }

        public static Vector3 ToChunkSpaceFloored(this Vector3 vec)
        {
            var final = ToChunkSpace(vec);

            return new Vector3((float)Math.Floor(final.X), (float)Math.Floor(final.Y), (float)Math.Floor(final.Z));
        }

        public static Vector3 ToRadians(this Vector3 vec)
        {
            return new Vector3(MathHelper.DegreesToRadians(vec.X), MathHelper.DegreesToRadians(vec.Y),
                MathHelper.DegreesToRadians(vec.Z));
        }

        public static Vector3 ToDegrees(this Vector3 vec)
        {
            return new Vector3(MathHelper.RadiansToDegrees(vec.X), MathHelper.RadiansToDegrees(vec.Y),
                MathHelper.RadiansToDegrees(vec.Z));
        }

        public static Color4 GetRandomColor()
        {
            byte r, g, b, a;
            r = (byte)World.GetInstance().Randomizer.Next(0, 255);
            g = (byte)World.GetInstance().Randomizer.Next(0, 255);
            b = (byte)World.GetInstance().Randomizer.Next(0, 255);
            a = (byte)World.GetInstance().Randomizer.Next(0, 255);

            return new Color4(r,g,b,a);
        }

        public static int GetSeedNum(this string str)
        {
            int num = 0;
            for (int i = 0; i < str.Length; i++)
            {
                int charNum = (int) str[i];
                num += charNum;
            }

            return num;
        }
    }
}
