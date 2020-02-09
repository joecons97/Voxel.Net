using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using Newtonsoft.Json;
using OpenTK;
using VoxelNet.Blocks;
using VoxelNet.Rendering;

namespace VoxelNet.Assets
{
    public class TexturePack : IImportable
    {
        const string texturesLocation = "Resources/Textures/";
        const string infoLocation = "Resources/Pack/Pack.json";
        const string blocksLocation = "Resources/Pack/Blocks.json";
        public const string DEFAULTPACK = "Resources/Packs/Default/";
        public string Name { get; set; }
        public string Description { get; set; }
        public Texture IconTexture { get; set; }
        public Texture Blocks { get; set; }
        public Texture Crosshair { get; set; }

        public TexturePackBlocks BlockData { get; set; }

        public IImportable Import(string path, ZipFile pack)
        {
            MemoryStream stream = new MemoryStream();
            pack[infoLocation].Extract(stream);
            TexturePack texPack = JsonConvert.DeserializeObject<TexturePack>(Encoding.ASCII.GetString(stream.ToArray()));//(File.ReadAllText(path + "/Pack.json"));

            texPack.IconTexture = AssetDatabase.GetAsset<Texture>(texturesLocation + "Pack_Icon.png");
            texPack.Blocks = AssetDatabase.GetAsset<Texture>(texturesLocation + "Blocks.png");
            texPack.Crosshair = AssetDatabase.GetAsset<Texture>(texturesLocation + "GUI/Crosshair.png");

            stream = new MemoryStream();
            pack[blocksLocation].Extract(stream);
            texPack.BlockData = JsonConvert.DeserializeObject<TexturePackBlocks>(Encoding.ASCII.GetString(stream.ToArray()));
            
            float oneSlotX = 1f / (float) texPack.BlockData.BlocksPerRow;
            float oneSlotY = 1f / (float)texPack.BlockData.BlocksPerColumn;

            foreach (var block in texPack.BlockData.Blocks)
            {
                var bl = BlockDatabase.GetBlock(block.block_id);

                if (bl == null)
                    continue;

                {
                    block.back_face.X *= oneSlotX;
                    block.back_face.Y *= oneSlotY;

                    block.front_face.X *= oneSlotX;
                    block.front_face.Y *= oneSlotY;

                    block.left_face.X *= oneSlotX;
                    block.left_face.Y *= oneSlotY;

                    block.right_face.X *= oneSlotX;
                    block.right_face.Y *= oneSlotY;

                    block.top_face.X *= oneSlotX;
                    block.top_face.Y *= oneSlotY;

                    block.bottom_face.X *= oneSlotX;
                    block.bottom_face.Y *= oneSlotY;

                    block.back_face_mask.X *= oneSlotX;
                    block.back_face_mask.Y *= oneSlotY;

                    block.front_face_mask.X *= oneSlotX;
                    block.front_face_mask.Y *= oneSlotY;

                    block.left_face_mask.X *= oneSlotX;
                    block.left_face_mask.Y *= oneSlotY;

                    block.right_face_mask.X *= oneSlotX;
                    block.right_face_mask.Y *= oneSlotY;

                    block.top_face_mask.X *= oneSlotX;
                    block.top_face_mask.Y *= oneSlotY;

                    block.bottom_face_mask.X *= oneSlotX;
                    block.bottom_face_mask.Y *= oneSlotY;

                }

                bl.BackFace = new Block.Face(new Rect(block.back_face.X, block.back_face.Y, block.back_face.X + oneSlotX, block.back_face.Y + oneSlotY),
                    new Rect(block.back_face_mask.X, block.back_face_mask.Y, block.back_face_mask.X + oneSlotX, block.back_face_mask.Y + oneSlotY));
                bl.FrontFace = new Block.Face(new Rect(block.front_face.X, block.front_face.Y, block.front_face.X + oneSlotX, block.front_face.Y + oneSlotY),
                    new Rect(block.front_face_mask.X, block.front_face_mask.Y, block.front_face_mask.X + oneSlotX, block.front_face_mask.Y + oneSlotY));

                bl.LeftFace = new Block.Face(new Rect(block.left_face.X, block.left_face.Y, block.left_face.X + oneSlotX, block.left_face.Y + oneSlotY),
                    new Rect(block.left_face_mask.X, block.left_face_mask.Y, block.left_face_mask.X + oneSlotX, block.left_face_mask.Y + oneSlotY));
                bl.RightFace = new Block.Face(new Rect(block.right_face.X, block.right_face.Y, block.right_face.X + oneSlotX, block.right_face.Y + oneSlotY),
                    new Rect(block.right_face_mask.X, block.right_face_mask.Y, block.right_face_mask.X + oneSlotX, block.right_face_mask.Y + oneSlotY));

                bl.TopFace = new Block.Face(new Rect(block.top_face.X, block.top_face.Y, block.top_face.X + oneSlotX, block.top_face.Y + oneSlotY),
                    new Rect(block.top_face_mask.X, block.top_face_mask.Y, block.top_face_mask.X + oneSlotX, block.top_face_mask.Y + oneSlotY));
                bl.BottomFace = new Block.Face(new Rect(block.bottom_face.X, block.bottom_face.Y, block.bottom_face.X + oneSlotX, block.bottom_face.Y + oneSlotY),
                    new Rect(block.bottom_face_mask.X, block.bottom_face_mask.Y, block.bottom_face_mask.X + oneSlotX, block.bottom_face_mask.Y + oneSlotY));

                BlockDatabase.SetBlock(block.block_id, bl);
            }

            return texPack;
        }

        public void Dispose()
        {
            IconTexture?.Dispose();
            Blocks?.Dispose();
        }

        public class TexturePackBlocks
        {
            public int BlocksPerRow;
            public int BlocksPerColumn;

            public Block[] Blocks;

            public class Block
            {
                public string block_id { get; set; }
                public Vector2 top_face = Vector2.Zero;
                public Vector2 bottom_face = Vector2.Zero;
                public Vector2 left_face = Vector2.Zero;
                public Vector2 right_face = Vector2.Zero;
                public Vector2 front_face = Vector2.Zero;
                public Vector2 back_face = Vector2.Zero;

                public Vector2 top_face_mask = Vector2.Zero;
                public Vector2 bottom_face_mask = Vector2.Zero;
                public Vector2 left_face_mask = Vector2.Zero;
                public Vector2 right_face_mask = Vector2.Zero;
                public Vector2 front_face_mask = Vector2.Zero;
                public Vector2 back_face_mask = Vector2.Zero;
            }
        }
    }
}
