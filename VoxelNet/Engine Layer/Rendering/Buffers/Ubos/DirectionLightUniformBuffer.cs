using OpenTK;

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
