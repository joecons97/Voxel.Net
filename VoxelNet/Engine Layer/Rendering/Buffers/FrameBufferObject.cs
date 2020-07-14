using System;
using OpenTK.Graphics.OpenGL4;

namespace VoxelNet
{
    public enum FBOType
    {
        None = 0,
        DepthTexture = 1,
        DepthRenderBuffer = 2
    }

    public class FrameBufferObject : IDisposable
    {
        public FBOType Type { get; }
        public int Width { get; }
        public int Height { get; }
        public int Handle { get; }
        public int ColorHandle { get; }
        public int DepthHandle { get; }

        public FrameBufferObject(int width, int height, FBOType type)
        {
            Type = type;
            Width = width;
            Height = height;

            Handle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            /*Initialise colour texture*/
            ColorHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ColorHandle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorHandle, 0);

            /*Initialise depth information*/
            switch (type)
            {
                case FBOType.DepthTexture:
                    DepthHandle = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, DepthHandle);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
                    //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
                    //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)DepthFunction.Less);
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthHandle, 0);
                    break;
                case FBOType.DepthRenderBuffer:
                    DepthHandle = GL.GenRenderbuffer();
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthHandle);
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);
                    GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, DepthHandle);
                    break;
            }
            Unbind();
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
            GL.Viewport(0, 0, Width, Height);
        }

        public void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, Program.Window.Width, Program.Window.Height);
        }

        public void BindToRead()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, Handle);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(Handle);
            GL.DeleteTexture(ColorHandle);
            GL.DeleteTexture(DepthHandle);
            GL.DeleteRenderbuffer(DepthHandle);
        }
    }
}
