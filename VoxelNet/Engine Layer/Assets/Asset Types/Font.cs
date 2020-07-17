using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using SharpFont;
using VoxelNet.Assets;
using VoxelNet.Rendering;

namespace VoxelNet
{
    public struct Character
    {
        public float AdvanceX;
        public float AdvanceY;

        public float BitmapWidth;
        public float BitmapHeight;

        public float BitmapLeft;
        public float BitmapTop;

        public float TexCoordX;
        public float TexCoordY;
    }

    public class Font : IImportable
    {
        private const float MAX_SIZE = 2048;
        private const uint FONT_SIZE = 48;

        private static Library fntLibrary = null;

        public Texture AtlasTexture { get; private set; }
        public float FontSize { get; } = FONT_SIZE;

        public float LineHeight { get; private set; }

        private Face fntFace;

        private Dictionary<char, Character> characters;

        private int atlasWidth;
        private int atlasHeight;

        static Font()
        {
            if(fntLibrary == null)
                fntLibrary = new Library();

        }

        public Font()
        {

        }

        Font(byte[] fontData)
        {
            atlasWidth = 0;
            atlasHeight = 0;

            int roww = 0;
            int rowh = 0;

            fntFace = new Face(fntLibrary, fontData, 0);
            fntFace.SetPixelSizes(0, FONT_SIZE);

            for (uint i = 0; i < 512; i++)
            {
                fntFace.LoadChar(i, LoadFlags.Render, LoadTarget.Normal);
                if (roww + fntFace.Glyph.Bitmap.Width + 1 >= MAX_SIZE)
                {
                    atlasWidth = Math.Max(atlasWidth, roww);
                    atlasHeight += rowh;
                    roww = 0;
                    rowh = 0;
                }

                roww += fntFace.Glyph.Bitmap.Width + 1;
                rowh = Math.Max(rowh, fntFace.Glyph.Bitmap.Rows);
            }

            atlasWidth = Math.Max(atlasWidth, roww);
            atlasHeight += rowh;

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            AtlasTexture = new Texture(atlasWidth, atlasHeight);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, AtlasTexture.Handle);
            int ox = 0, oy = 0;
            rowh = 0;

            characters = new Dictionary<char, Character>();

            for (uint i = 32; i < 512; i++)
            {
                fntFace.LoadChar(i, LoadFlags.Render, LoadTarget.Normal);

                if (ox + fntFace.Glyph.Bitmap.Width + 1 >= MAX_SIZE)
                {
                    oy += rowh;
                    rowh = 0;
                    ox = 0;
                }

                GL.TexSubImage2D(TextureTarget.Texture2D, 0 ,ox, oy, fntFace.Glyph.Bitmap.Width, fntFace.Glyph.Bitmap.Rows, PixelFormat.Alpha, PixelType.UnsignedByte, fntFace.Glyph.Bitmap.Buffer);
                var chara = new Character
                {
                    AdvanceX = fntFace.Glyph.Advance.X.Value >> 6,
                    AdvanceY = fntFace.Glyph.Advance.Y.Value >> 6,
                    BitmapWidth = fntFace.Glyph.Bitmap.Width,
                    BitmapHeight = fntFace.Glyph.Bitmap.Rows,
                    BitmapLeft = fntFace.Glyph.BitmapLeft,
                    BitmapTop = fntFace.Glyph.BitmapTop,
                    TexCoordX = (float)ox / (float) atlasWidth,
                    TexCoordY = (float)oy / (float) atlasHeight
                };

                rowh = Math.Max(rowh, fntFace.Glyph.Bitmap.Rows);
                ox += fntFace.Glyph.Bitmap.Width + 1;
                characters.Add((char)i, chara);
                if(fntFace.Glyph.Bitmap.Rows > LineHeight)
                    LineHeight = fntFace.Glyph.Bitmap.Rows;
            }

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
        }

        public IImportable Import(string path, ZipFile pack)
        {
            byte[] data = null;
            if (pack.ContainsEntry(path))
            {
                MemoryStream stream = new MemoryStream();
                pack[path].Extract(stream);
                data = stream.GetBuffer();
            }
            else
            {
                data = File.ReadAllBytes(path);
            }

            return new Font(data);
        }

        public Character RequestCharacter(char chara)
        {
            if(ContainsCharacter(chara))
                return characters[chara];

            return default;
        }

        public float GetAtlasWidth()
        {
            return atlasWidth;
        }

        public float GetAtlasHeight()
        {
            return atlasHeight;
        }

        public bool ContainsCharacter(char chara)
        {
            return characters.Keys.Contains(chara);
        }

        public void Dispose()
        {
            fntFace?.Dispose();
            fntLibrary?.Dispose();
            AtlasTexture?.Dispose();
        }
    }
}
