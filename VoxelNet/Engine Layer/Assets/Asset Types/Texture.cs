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
    public class Texture : IImportable, IDisposable
    {
        public int Handle { get; private set; }

        List<byte> pixels = new List<byte>();

        public Texture() { }

        /// <summary>
        /// Use AssetData.GetAsset instead!
        /// </summary>
        /// <param name="file"></param>
        public Texture(string file)
        {
            Image<Rgba32> img = Image.Load(File.ReadAllBytes(file));
            //img.Mutate(x => x.Flip(FlipMode.Vertical));
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

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, img.Width, img.Height, 
                0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());

            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public Texture(IntPtr data, int width, int height)
        {
            int tex = 0;
            GL.CreateTextures(TextureTarget.Texture2D, 1, out tex);
            Handle = tex;

            GL.TextureStorage2D(Handle, 1, SizedInternalFormat.Rgba8, width, height);

            GL.TextureSubImage2D(Handle, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TextureParameter(Handle, TextureParameterName.TextureMaxLevel, 0);
        }

        public void SetMinFilter(TextureMinFilter filter)
        {
            GL.TextureParameter(Handle, TextureParameterName.TextureMinFilter, (int)filter);
        }

        public void SetMagFilter(TextureMagFilter filter)
        {
            GL.TextureParameter(Handle, TextureParameterName.TextureMagFilter, (int) filter);
        }

        public IImportable Import(string path)
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
