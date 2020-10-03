using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using OpenTK.Graphics;
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
        public int Handle { get; }
        public int Width { get; }
        public int Height { get; }

        List<Color4> pixels = new List<Color4>();
        List<byte> bytePixels = new List<byte>();

        public Texture() { }

        /// <summary>
        /// Use AssetData.GetAsset instead!
        /// </summary>
        /// <param name="file"></param>
        public Texture(string file, bool srgb = true, bool mips = true)
        {
            Image<Rgba32> img = Image.Load(File.ReadAllBytes(file));
            Width = img.Width;
            Height = img.Height;
            //img.Mutate(x => x.Flip(FlipMode.Vertical));
            Rgba32[] tempPixels = img.GetPixelSpan().ToArray();

            foreach (Rgba32 p in tempPixels)
            {
                bytePixels.Add(p.R);
                bytePixels.Add(p.G);
                bytePixels.Add(p.B);
                bytePixels.Add(p.A);

                pixels.Add(new Color4(p.R / 255f, p.G / 255f, p.B / 255f, p.A / 255f));
            }

            Handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexImage2D(TextureTarget.Texture2D, 0, srgb ? PixelInternalFormat.Srgb8Alpha8 : PixelInternalFormat.Rgba8, img.Width, img.Height, 
                0, PixelFormat.Rgba, PixelType.UnsignedByte, bytePixels.ToArray());

            if(mips)
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        public Texture(MemoryStream file, bool srgb = true, bool mips = true)
        {
            Image<Rgba32> img = Image.Load(file.GetBuffer());
            Width = img.Width;
            Height = img.Height;
            //img.Mutate(x => x.Flip(FlipMode.Vertical));
            Rgba32[] tempPixels = img.GetPixelSpan().ToArray();

            foreach (Rgba32 p in tempPixels)
            {
                bytePixels.Add(p.R);
                bytePixels.Add(p.G);
                bytePixels.Add(p.B);
                bytePixels.Add(p.A);

                pixels.Add(new Color4(p.R/255f,p.G / 255f, p.B / 255f, p.A / 255f));
            }

            Handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexImage2D(TextureTarget.Texture2D, 0, srgb ? PixelInternalFormat.Srgb8Alpha8 : PixelInternalFormat.Rgba8, img.Width, img.Height,
                0, PixelFormat.Rgba, PixelType.UnsignedByte, bytePixels.ToArray());

            if (mips)
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public Texture(int width, int height)
        {
            Handle = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, Handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexImage2D(TextureTarget.Texture2D, 0,PixelInternalFormat.Alpha, width, height,
                0, PixelFormat.Alpha, PixelType.UnsignedByte, IntPtr.Zero);
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

        public Color4 GetPixel(int x, int y)
        {
            int index = x * (int)Height + y;
            var pixel = pixels[index];
            return pixel;
        }

        public void SetMinFilter(TextureMinFilter filter)
        {
            GL.TextureParameter(Handle, TextureParameterName.TextureMinFilter, (int)filter);
        }

        public void SetMagFilter(TextureMagFilter filter)
        {
            GL.TextureParameter(Handle, TextureParameterName.TextureMagFilter, (int) filter);
        }

        public IImportable Import(string path, ZipFile pack)
        {
            bool srgb = !path.Contains("/RenderGUI/") || path.Contains("GUI");

            if (pack.ContainsEntry(path))
            {
                var entry = pack[path];
                MemoryStream outputStream = new MemoryStream();
                entry.Extract(outputStream);
                Texture texture = new Texture(outputStream, srgb, srgb);
                Debug.Log("Loaded texture from pack");
                return texture;
            }
            else
            {
                Debug.Log("Loaded texture from file");
                Texture texture = new Texture(path, srgb, srgb);
                return texture;
            }
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
