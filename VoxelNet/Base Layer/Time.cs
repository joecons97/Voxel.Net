using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelNet
{
    public static class Time
    {
        public static float GameTime { get; set; }
        public static float DeltaTime { get; set; }
        public static float FramesPerSecond { get; private set; }
        private static float lastFpsUpdateTime = 0;

        public static void UpdateFrameRate(float fps)
        {
            if (GameTime > lastFpsUpdateTime + 0.5)
            {
                FramesPerSecond = fps;
                lastFpsUpdateTime = GameTime;
            }
        }
    }
}
