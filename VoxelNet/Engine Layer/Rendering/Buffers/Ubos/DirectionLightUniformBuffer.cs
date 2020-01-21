using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Rendering.Material;

namespace VoxelNet.Buffers.Ubos
{
    public struct LightingUniformBufferData
    {
        public Vector4 AmbientColour;
        public Vector4 SunDirection;
        public Vector4 SunColour;
        public float SunStrength;
    }
}
