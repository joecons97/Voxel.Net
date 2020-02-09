using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelNet.Buffers.Ubos;
using VoxelNet.Rendering;

namespace VoxelNet.Buffers
{
    public static class UniformBuffers
    {
        public static int TotalUBOs { get; set; }

        public static UniformBuffer<LightingUniformBufferData> DirectionLightBuffer { get; }
        public static UniformBuffer<CameraUniformBuffer> WorldCameraBuffer { get; }
        public static UniformBuffer<TimeUniformBuffer> TimeBuffer { get; }

        static UniformBuffers()
        {
            DirectionLightBuffer = new UniformBuffer<LightingUniformBufferData>(default(LightingUniformBufferData), "U_Lighting");
            WorldCameraBuffer = new UniformBuffer<CameraUniformBuffer>(default(CameraUniformBuffer), "U_Camera");
            TimeBuffer = new UniformBuffer<TimeUniformBuffer>(default(TimeUniformBuffer), "U_Time");
        }

        public static void Dispose()
        {
            DirectionLightBuffer.Dispose();
            WorldCameraBuffer.Dispose();
            TimeBuffer.Dispose();
        }

        public static void BindAll(int program)
        {
            DirectionLightBuffer.Bind(program);
            WorldCameraBuffer.Bind(program);
            TimeBuffer.Bind(program);
        }

        public static void UnbindAll()
        {
            DirectionLightBuffer.Unbind();
            WorldCameraBuffer.Unbind();
            TimeBuffer.Unbind();
        }
    }
}
