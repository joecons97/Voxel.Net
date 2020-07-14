using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;

namespace VoxelNet.PostProcessing
{
    public class BlitEffect : IDisposable
    {
        static Vector3[] meshPoses = new []{new Vector3(-1,-1,0), new Vector3(-1, 1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 0) };
        static Vector2[] meshUvs = new[] { new Vector2(0,0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1,1 ) };

        public static Mesh BlitMesh = new Mesh(new VertexContainer(meshPoses, meshUvs), new uint[] {1, 2, 0, 3, 2, 1});

        public virtual Material BlitMaterial { get; set; }

        protected bool IsLastEffectInStack;

        public FrameBufferObject SourceFbo { get; private set; } = new FrameBufferObject(Program.Window.Width, Program.Window.Height, FBOType.DepthRenderBuffer);

        public BlitEffect()
        {
            Program.Window.Resize += delegate (object sender, EventArgs args)
            {
                SourceFbo = new FrameBufferObject(Program.Window.Width, Program.Window.Height, FBOType.DepthRenderBuffer);
            };
        }

        public virtual void Render(FrameBufferObject src)
        {
            BlitMaterial.SetScreenSourceTexture("u_Src", src.ColorHandle);
            Renderer.DrawNow(BlitMesh, BlitMaterial);
        }

        protected void Blit(FrameBufferObject source, FrameBufferObject destination, Material material)
        {
            destination.Bind();
            material.SetScreenSourceTexture("u_Src", source.ColorHandle);
            Renderer.DrawNow(BlitMesh, material);
            destination.Unbind();
        }

        public virtual void PreRender(bool isLast)
        {
            IsLastEffectInStack = isLast;
            GL.Disable(EnableCap.DepthTest);

            if (!isLast)
            {
                SourceFbo.Bind();
            }
            else
            {
                SourceFbo.Unbind();
            }
        }

        public virtual void PostRender(bool isLast)
        {
            IsLastEffectInStack = isLast;
            GL.Enable(EnableCap.DepthTest);

            SourceFbo.Unbind();
        }

        public void Dispose()
        {
            BlitMaterial?.Dispose();
            if (BlitMesh != null)
            {
                BlitMesh.Dispose();
                BlitMesh = null;
            }
        }
    }
}
