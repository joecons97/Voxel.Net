using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VoxelNet.Assets;

namespace VoxelNet.Rendering
{
    public class Texture : Importable, IDisposable
    {
        public int Handle { get; private set; }

        List<byte> pixels = new List<byte>();

        public Texture() { }

        public Texture(string file)
        {
            Image<Rgba32> img = Image.Load(File.ReadAllBytes(file));
            img.Mutate(x => x.Flip(FlipMode.Vertical));
            Rgba32[] tempPixels = img.GetPixelSpan().ToArray();

            foreach (Rgba32 p in tempPixels)
            {
                pixels.Add(p.R);
                pixels.Add(p.G);
                pixels.Add(p.B);
                pixels.Add(p.A);
            }

            Handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, img.Width, img.Height, 
                0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override Importable Import(string path)
        {
            Texture texture = new Texture(path);
            return texture;
        }

        public void Bind(int slot = 0)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        public void Dispose()
        {
            GL.DeleteTexture(Handle);
        }
    }
}
