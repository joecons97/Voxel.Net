using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;

namespace VoxelNet.PostProcessing
{
    public class Bloom : BlitEffect
    {
        Material cutoffMaterial = AssetDatabase.GetAsset<Material>("Resources/Materials/Post Processing/CutoffPPE.mat");
        Material blurMaterial = AssetDatabase.GetAsset<Material>("Resources/Materials/Post Processing/BlurPPE.mat");
        public override Material BlitMaterial => AssetDatabase.GetAsset<Material>("Resources/Materials/Post Processing/BloomPPE.mat");

        FrameBufferObject fbo = new FrameBufferObject((int)((float)Program.Window.Width / 4), (int)((float)Program.Window.Height / 4), FBOType.DepthRenderBuffer);

        public int BlurIterations { get; set; } = 5;
        public float BrightnessCutoff { get; set; } = 1.2f;
        public float BloomStrength { get; set; } = .009f;

        public override void Render(FrameBufferObject src)
        {
            cutoffMaterial.Shader.SetUniform("u_Cutoff", BrightnessCutoff);

           Blit(src, fbo, cutoffMaterial);

            for (int i = 0; i < BlurIterations; i++)
            {
                Blit(fbo, fbo, blurMaterial);
            }

            PreRender(IsLastEffectInStack);

            BlitMaterial.Shader.SetUniform("u_BlurIterations", BlurIterations);
            BlitMaterial.Shader.SetUniform("u_BloomStrength", BloomStrength);
            BlitMaterial.SetScreenSourceTexture("u_Src_Small", fbo.ColorHandle, 1);
            BlitMaterial.SetScreenSourceTexture("u_Src", src.ColorHandle);
            Renderer.DrawNow(BlitMesh, BlitMaterial);
        }
    }
}
