using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using VoxelNet.Rendering;

namespace VoxelNet.PostProcessing
{
    public static class PostProcessingEffects
    {
        static List<BlitEffect> effects = new List<BlitEffect>();

        public static void Dispose()
        {
            //fbo.Dispose();
            foreach (var blitEffect in effects)
            {
                blitEffect.Dispose();
            }
        }

        public static void RegisterEffect(BlitEffect effect)
        {
            effects.Add(effect);
        }

        public static void BeginPostProcessing()
        {
            if (effects.Count == 0)
                return;
        }

        public static void RenderEffects()
        {
            if (effects.Count == 0) return;

            effects[0].PreRender(effects.Count == 1);
            effects[0].Render(Renderer.FrameBuffer);
            effects[0].PostRender(effects.Count == 1);

            for (var index = 1; index < effects.Count; index++)
            {
                var effect = effects[index];

                if (index == effects.Count - 1)
                    GL.Enable(EnableCap.FramebufferSrgb);
                else
                    GL.Disable(EnableCap.FramebufferSrgb);

                effect.PreRender(index == effects.Count - 1);
                effect.Render(effects[index-1].SourceFbo);
                effect.PostRender(index == effects.Count - 1);
            }
        }
    }
}
