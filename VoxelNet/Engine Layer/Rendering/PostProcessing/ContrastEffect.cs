using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelNet.Rendering.Material;

namespace VoxelNet.PostProcessing
{
    public class ContrastEffect : BlitEffect
    {
        public override Material BlitMaterial => AssetDatabase.GetAsset<Material>("Resources/Materials/ContrastPPE.mat");

        public float Contrast { get; set; }

        public override void Render(FrameBufferObject src)
        {
            BlitMaterial.Shader.SetUniform("u_Contrast", Contrast);
            base.Render(src);
        }
    }
}
