using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using SimplexNoise;
using SixLabors.ImageSharp.ColorSpaces;
using VoxelNet.Assets;
using VoxelNet.Blocks;
using VoxelNet.Decoration;
using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;

namespace VoxelNet
{
    public class Chunk: IDisposable
    {
        public class BlockState
        {
            public const float LIGHTUNIT = 1f/16f;

            public short id;
            public sbyte x;
            public sbyte y;
            public sbyte z;

            public BlockState(sbyte x, sbyte y, sbyte z, Chunk chunk)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }

        public static Material ChunkMaterial;
        public static Material ChunkWaterMaterial;

        public const int LAYERCOUNT = 8;
        public const int WIDTH = 16;
        public const int HEIGHT = 128;

        public Vector2 Position { get; private set; }

        public bool AreAllNeighboursSet
        {
            get
            {
                return LeftNeighbour != null  &&
                       RightNeighbour != null &&
                       FrontNeighbour != null &&
                       BackNeighbour != null;
            }
        }

        public Chunk LeftNeighbour;
        public Chunk RightNeighbour;
        public Chunk FrontNeighbour;
        public Chunk BackNeighbour;

        public BlockState[,,] Blocks = new BlockState[WIDTH,HEIGHT,WIDTH];

        private Mesh mesh;
        private Mesh waterMesh;
        private float[,] heightmap;

        private Matrix4 worldMatrix;

        private float noiseScale = 0.25f;

        #region MeshData
        private VertexBlockContainer blockContainer;
        private VertexNormalContainer waterContainer;

        private bool shouldRebuildMesh;
        private bool shouldRebuildWaterMesh;

        private uint[] indices;
        private uint[] indicesWater;
        #endregion

        public Chunk(Vector2 position)
        {
            Position = position;
            worldMatrix = Matrix4.CreateTranslation(Position.X * WIDTH, 0, Position.Y * WIDTH);
        }

        public float[,] GetHeightMap()
        {
            return heightmap;
        }

        public void GenerateHeightMap()
        {
            heightmap = new float[WIDTH, WIDTH];
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < WIDTH; y++)
                {
                    float NoiseX = ((float) x / (float) WIDTH) + (Position.X);
                    float NoiseY = ((float) y / (float) WIDTH) + (Position.Y);
                    float mainNoise = (float)((World.GetInstance().TerrainNoise
                                                .Octaves2D(NoiseX, NoiseY, 8, .4f, 2, noiseScale) + 1) / 2);

                    float ocean = (float)((World.GetInstance().TerrainNoise
                                               .Octaves2D(NoiseX, NoiseY, 8, .4f, 2, noiseScale * 0.125f) + 1) / 2) * 6;

                    ocean -= 2;
                    ocean = (float)Math.Pow(MathHelper.Clamp(ocean, 0, 1) + (mainNoise/10f), 0.6f);

                    heightmap[x, y] = Math.Max(0, Math.Min(mainNoise + 0.2f, ocean) * 255f);
                }
            }
        }

        public void FillBlocks()
        {
                for (int x = 0; x < WIDTH; x++)
                {
                    for (int z = 0; z < WIDTH; z++)
                    {
                        int h = GetHeightAtBlock(x, z);
                        for (int y = 0; y < HEIGHT; y++)
                        {
                            Blocks[x, y, z] = new BlockState((sbyte) x, (sbyte) y, (sbyte) z, this);
                            if (y > h)
                            {
                                if(y <= World.GetInstance().WaterHeight)
                                    Blocks[x, y, z].id = (short)GameBlocks.WATER.ID;

                            }
                            else if (y == h)
                            {
                                if(y < World.GetInstance().WaterHeight + 3)
                                    Blocks[x, y, z].id = (short)GameBlocks.SAND.ID;
                                else
                                {
                                    Blocks[x, y, z].id = (short)GameBlocks.GRASS.ID;
                                }

                            }
                            else if (y > h - 5)
                            {
                                if (y < World.GetInstance().WaterHeight + 3)
                                    Blocks[x, y, z].id = (short)GameBlocks.SAND.ID;
                                else
                                    Blocks[x, y, z].id = (short)GameBlocks.DIRT.ID;
                            }
                            else
                            {
                                Blocks[x, y, z].id = (short)GameBlocks.STONE.ID;
                            }
                        }
                    }
            }
            using (OakDecorator decorator = new OakDecorator())
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    for (int z = 0; z < WIDTH; z++)
                    {
                        int h = GetHeightAtBlock(x, z);
                        decorator.DecorateAtBlock(this, x, h, z);
                    }
                }
            }
        }

        public void GenerateMesh()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uv2 = new List<Vector2>();
            List<Vector4> col = new List<Vector4>();
            List<float> light = new List<float>();

            List<Vector3> verticesWater = new List<Vector3>();
            List<Vector2> uvsWater = new List<Vector2>();
            List<Vector3> normalsWater = new List<Vector3>();

            List<uint> indices = new List<uint>();
            List<uint> indicesWater = new List<uint>();

            uint indexCount = 0;
            uint indexCountWater = 0;

            Block workingBlock = null;

            List<Vector3> toPropagate = new List<Vector3>();
            int w = WIDTH * 3;
            byte[,,] lightmap = new byte[w, HEIGHT, w];

            for (int x = 0; x < w; ++x)
            {
                for (int z = 0; z < w; ++z)
                {
                    if ((x % 47) * (z % 47) == 0) //filters outer edges
                    {
                        //Debug.Log($"these should at least 0 or 47  ->  {x} {z}"); 
                        for (int yy = 0; yy < HEIGHT; yy++) //dont do outer edges
                        {
                            lightmap[x, yy, z] = 15; //set all edges to 15 to stop tracing at edges
                        }
                        continue;
                    }
                    int worldX = x - WIDTH;
                    int worldZ = z - WIDTH;
                    int height = Math.Max(0, GetHeightAtBlock(worldX, worldZ));

                    for (int y = height; y < HEIGHT; y++)
                    {
                        //Do manually here!
                        lightmap[x, y, z] = 15; //set all edges to 15 to stop tracing at edges
                    }

                    if (x < w - 2) height = Math.Max(height, GetHeightAtBlock(worldX + 1, worldZ));
                    if (x > 1) height = Math.Max(height, GetHeightAtBlock(worldX - 1, worldZ));
                    if (z < w - 2) height = Math.Max(height, GetHeightAtBlock(worldX, worldZ + 1));
                    if (z > 1) height = Math.Max(height, GetHeightAtBlock(worldX, worldZ - 1));

                    height = Math.Min(height + 1, HEIGHT - 1);
                    if (height < 2) continue;

                    toPropagate.Add(new Vector3(x, height, z));
                }
            }

            while (toPropagate.Count > 0)
            {
                Vector3 position = toPropagate.Last();
                toPropagate.RemoveAt(toPropagate.Count - 1);
                int x = (int)position.X;
                int y = (int)position.Y;
                int z = (int)position.Z;
                int worldX = x - WIDTH;
                int worldZ = z - WIDTH;

                byte lightVal = lightmap[x, y, z];

                if (x < w - 1)
                {
                    byte lightR = lightmap[x + 1, y, z];
                    if (lightR < lightVal - 1)
                    {
                        short bR = GetBlockID(worldX + 1, y, worldZ);
                        if (bR == 0)
                        {
                            lightmap[x + 1, y, z] = (byte)(lightVal - 1);
                            toPropagate.Add(new Vector3(x + 1, y, z));
                        }
                    }
                }
                if (x > 0)
                {
                    byte lightL = lightmap[x - 1, y, z];
                    if (lightL < lightVal - 1)
                    {
                        short bL = GetBlockID(worldX - 1, y, worldZ);
                        if (bL == 0)
                        {
                            lightmap[x - 1, y, z] = (byte)(lightVal - 1);
                            toPropagate.Add(new Vector3(x - 1, y, z));
                        }
                    }
                }

                if (y > 0)
                {
                    short bD = GetBlockID(worldX, y - 1, worldZ);
                    if (bD == 0)
                    {
                        if (lightVal == 15)
                        {
                            lightmap[x, y - 1, z] = (byte)(lightVal);
                            toPropagate.Add(new Vector3(x, y - 1, z));
                        }
                        else
                        {
                            byte lightD = lightmap[x, y - 1, z];
                            if (lightD < lightVal - 1)
                            {
                                lightmap[x, y - 1, z] = (byte)(lightVal - 1);
                                toPropagate.Add(new Vector3(x, y - 1, z));
                            }
                        }
                    }
                    else if(bD != -1)
                    {
                        sbyte op = BlockDatabase.GetBlock(bD).Opacity;
                        if (op < 15)
                        {
                            op = (sbyte)(lightVal - op);
                            op = Math.Max(op, (sbyte)0);
                            lightmap[x, y - 1, z] = (byte)op;
                            toPropagate.Add(new Vector3(x, y - 1, z));
                        }
                    }
                }
                if (y < HEIGHT - 1)
                {
                    byte lightU = lightmap[x, y + 1, z];
                    if (lightU < lightVal - 1)
                    {
                        short bU = GetBlockID(worldX, y + 1, worldZ);
                        if (bU == 0)
                        {
                            lightmap[x, y + 1, z] = (byte)(lightVal - 1);
                            toPropagate.Add(new Vector3(x, y + 1, z));
                        }
                    }
                }

                if (z < w - 1)
                {
                    byte lightF = lightmap[x, y, z + 1];
                    if (lightF < lightVal - 1)
                    {
                        short bF = GetBlockID(worldX, y, worldZ + 1);
                        if (bF == 0)
                        {
                            lightmap[x, y, z + 1] = (byte)(lightVal - 1);
                            toPropagate.Add(new Vector3(x, y, z + 1));
                        }
                    }
                }
                if (z > 0)
                {
                    byte lightB = lightmap[x, y, z - 1];
                    if (lightB < lightVal - 1)
                    {
                        short bB = GetBlockID(worldX, y, worldZ - 1);
                        if (bB == 0)
                        {
                            lightmap[x, y, z - 1] = (byte)(lightVal - 1);
                            toPropagate.Add(new Vector3(x, y, z - 1));
                        }
                    }
                }
            }

            for (int x = 0; x < WIDTH; x++)
            {
                for (int z = 0; z < WIDTH; z++)
                {
                    for (int y = 0; y < HEIGHT; y++)
                    {
                        int id = GetBlockID(x, y, z);

                        if (id == 0)
                            continue;

                        workingBlock = BlockDatabase.GetBlock(id);

                        if (workingBlock.ID == GameBlocks.WATER.ID)
                        {
                            if (ShouldDrawBlockFacing(x, y, z - 1, workingBlock.ID))
                                AddBackFaceWater(x, y, z);

                            if (ShouldDrawBlockFacing(x, y, z + 1, workingBlock.ID))
                                AddFrontFaceWater(x, y, z);

                            if (ShouldDrawBlockFacing(x - 1, y, z, workingBlock.ID))
                                AddLeftFaceWater(x, y, z);

                            if (ShouldDrawBlockFacing(x + 1, y, z, workingBlock.ID))
                                AddRightFaceWater(x, y, z);

                            if (ShouldDrawBlockFacing(x, y + 1, z, workingBlock.ID))
                                AddTopFaceWater(x, y, z);

                            if (ShouldDrawBlockFacing(x, y - 1, z, workingBlock.ID))
                                AddBottomFaceWater(x, y, z);
                        }
                        else
                        {
                            if (ShouldDrawBlockFacing(x, y, z - 1, workingBlock.ID))
                                AddBackFace(x, y, z);

                            if (ShouldDrawBlockFacing(x, y, z + 1, workingBlock.ID))
                                AddFrontFace(x, y, z);

                            if (ShouldDrawBlockFacing(x - 1, y, z, workingBlock.ID))
                                AddLeftFace(x, y, z);

                            if (ShouldDrawBlockFacing(x + 1, y, z, workingBlock.ID))
                                AddRightFace(x, y, z);

                            if (ShouldDrawBlockFacing(x, y + 1, z, workingBlock.ID))
                                AddTopFace(x, y, z);

                            if (ShouldDrawBlockFacing(x, y - 1, z, workingBlock.ID))
                                AddBottomFace(x, y, z);
                        }
                    }
                }
            }

            void AddFrontFace(int x, int y, int z)
            {
                vertices.Add(new Vector3(1 + x, 1 + y, 1 + z));
                vertices.Add(new Vector3(0 + x, 1 + y, 1 + z));
                vertices.Add(new Vector3(0 + x, 0 + y, 1 + z));
                vertices.Add(new Vector3(1 + x, 0 + y, 1 + z));

                float lightVal = lightmap[x + WIDTH, y, z + WIDTH + 1];//GetBlockLight(x, y, z + 1);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);

                uvs.Add(new Vector2(workingBlock.FrontFace.UVCoordinates.X, workingBlock.FrontFace.UVCoordinates.Y));
                uvs.Add(new Vector2(workingBlock.FrontFace.UVCoordinates.Width, workingBlock.FrontFace.UVCoordinates.Y));
                uvs.Add(new Vector2(workingBlock.FrontFace.UVCoordinates.Width, workingBlock.FrontFace.UVCoordinates.Height));
                uvs.Add(new Vector2(workingBlock.FrontFace.UVCoordinates.X, workingBlock.FrontFace.UVCoordinates.Height));

                uv2.Add(new Vector2(workingBlock.FrontFace.Mask.X, workingBlock.FrontFace.Mask.Y));
                uv2.Add(new Vector2(workingBlock.FrontFace.Mask.Width, workingBlock.FrontFace.Mask.Y));
                uv2.Add(new Vector2(workingBlock.FrontFace.Mask.Width, workingBlock.FrontFace.Mask.Height));
                uv2.Add(new Vector2(workingBlock.FrontFace.Mask.X, workingBlock.FrontFace.Mask.Height));

                col.Add(workingBlock.BlockColor(x,y,z).ToVector4());
                col.Add(workingBlock.BlockColor(x,y,z).ToVector4());
                col.Add(workingBlock.BlockColor(x,y,z).ToVector4());
                col.Add(workingBlock.BlockColor(x,y,z).ToVector4());

                normals.Add(new Vector3(0, 0, 1));
                normals.Add(new Vector3(0, 0, 1));
                normals.Add(new Vector3(0, 0, 1));
                normals.Add(new Vector3(0, 0, 1));

                indices.Add(indexCount);
                indices.Add(indexCount + 1);
                indices.Add(indexCount + 2);

                indices.Add(indexCount + 2);
                indices.Add(indexCount + 3);
                indices.Add(indexCount);

                indexCount += 4;
            }

            void AddBackFace(int x, int y, int z)
            {
                vertices.Add(new Vector3(0 + x, 1 + y, 0 + z));
                vertices.Add(new Vector3(1 + x, 1 + y, 0 + z));
                vertices.Add(new Vector3(1 + x, 0 + y, 0 + z));
                vertices.Add(new Vector3(0 + x, 0 + y, 0 + z));

                float lightVal = lightmap[x + WIDTH, y, z + WIDTH - 1];//GetBlockLight(x, y, z - 1);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);

                uvs.Add(new Vector2(workingBlock.BackFace.UVCoordinates.X, workingBlock.BackFace.UVCoordinates.Y));
                uvs.Add(new Vector2(workingBlock.BackFace.UVCoordinates.Width, workingBlock.BackFace.UVCoordinates.Y));
                uvs.Add(new Vector2(workingBlock.BackFace.UVCoordinates.Width, workingBlock.BackFace.UVCoordinates.Height));
                uvs.Add(new Vector2(workingBlock.BackFace.UVCoordinates.X, workingBlock.BackFace.UVCoordinates.Height));

                uv2.Add(new Vector2(workingBlock.BackFace.Mask.X, workingBlock.BackFace.Mask.Y));
                uv2.Add(new Vector2(workingBlock.BackFace.Mask.Width, workingBlock.BackFace.Mask.Y));
                uv2.Add(new Vector2(workingBlock.BackFace.Mask.Width, workingBlock.BackFace.Mask.Height));
                uv2.Add(new Vector2(workingBlock.BackFace.Mask.X, workingBlock.BackFace.Mask.Height));

                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());

                normals.Add(new Vector3(0,0,-1));
                normals.Add(new Vector3(0,0,-1));
                normals.Add(new Vector3(0,0,-1));
                normals.Add(new Vector3(0,0, -1));

                indices.Add(indexCount);
                indices.Add(indexCount + 1);
                indices.Add(indexCount + 2);

                indices.Add(indexCount + 2);
                indices.Add(indexCount + 3);
                indices.Add(indexCount);

                indexCount += 4;
            }

            void AddTopFace(int x, int y, int z)
            {
                vertices.Add(new Vector3(1 + x, 1 + y, 0 + z));
                vertices.Add(new Vector3(0 + x, 1 + y, 0 + z));
                vertices.Add(new Vector3(0 + x, 1 + y, 1 + z));
                vertices.Add(new Vector3(1 + x, 1 + y, 1 + z));

                float lightVal = y == HEIGHT - 1 ? 15 : lightmap[x + WIDTH, y + 1, z + WIDTH];//GetBlockLight(x, y + 1, z);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);

                uvs.Add(new Vector2(workingBlock.TopFace.UVCoordinates.X, workingBlock.TopFace.UVCoordinates.Y));
                uvs.Add(new Vector2(workingBlock.TopFace.UVCoordinates.Width, workingBlock.TopFace.UVCoordinates.Y));
                uvs.Add(new Vector2(workingBlock.TopFace.UVCoordinates.Width, workingBlock.TopFace.UVCoordinates.Height));
                uvs.Add(new Vector2(workingBlock.TopFace.UVCoordinates.X, workingBlock.TopFace.UVCoordinates.Height));

                uv2.Add(new Vector2(workingBlock.TopFace.Mask.X, workingBlock.TopFace.Mask.Y));
                uv2.Add(new Vector2(workingBlock.TopFace.Mask.Width, workingBlock.TopFace.Mask.Y));
                uv2.Add(new Vector2(workingBlock.TopFace.Mask.Width, workingBlock.TopFace.Mask.Height));
                uv2.Add(new Vector2(workingBlock.TopFace.Mask.X, workingBlock.TopFace.Mask.Height));

                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());

                normals.Add(new Vector3(0,1,0));
                normals.Add(new Vector3(0,1,0));
                normals.Add(new Vector3(0,1,0));
                normals.Add(new Vector3(0,1,0));

                indices.Add(indexCount);
                indices.Add(indexCount + 1);
                indices.Add(indexCount + 2);

                indices.Add(indexCount + 2);
                indices.Add(indexCount + 3);
                indices.Add(indexCount);

                indexCount += 4;
            }

            void AddBottomFace(int x, int y, int z)
            {
                vertices.Add(new Vector3(1 + x, 0 + y, 1 + z));
                vertices.Add(new Vector3(0 + x, 0 + y, 1 + z));
                vertices.Add(new Vector3(0 + x, 0 + y, 0 + z));
                vertices.Add(new Vector3(1 + x, 0 + y, 0 + z));

                float lightVal = y == 0 ? 15 : lightmap[x + WIDTH, y - 1, z + WIDTH]; //GetBlockLight(x, y - 1, z);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);

                uvs.Add(new Vector2(workingBlock.BottomFace.UVCoordinates.X, workingBlock.BottomFace.UVCoordinates.Y));
                uvs.Add(new Vector2(workingBlock.BottomFace.UVCoordinates.Width, workingBlock.BottomFace.UVCoordinates.Y));
                uvs.Add(new Vector2(workingBlock.BottomFace.UVCoordinates.Width, workingBlock.BottomFace.UVCoordinates.Height));
                uvs.Add(new Vector2(workingBlock.BottomFace.UVCoordinates.X, workingBlock.BottomFace.UVCoordinates.Height));

                uv2.Add(new Vector2(workingBlock.BottomFace.Mask.X, workingBlock.BottomFace.Mask.Y));
                uv2.Add(new Vector2(workingBlock.BottomFace.Mask.Width, workingBlock.BottomFace.Mask.Y));
                uv2.Add(new Vector2(workingBlock.BottomFace.Mask.Width, workingBlock.BottomFace.Mask.Height));
                uv2.Add(new Vector2(workingBlock.BottomFace.Mask.X, workingBlock.BottomFace.Mask.Height));

                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());

                normals.Add(new Vector3(0, -1, 0));
                normals.Add(new Vector3(0, -1, 0));
                normals.Add(new Vector3(0, -1, 0));
                normals.Add(new Vector3(0, -1, 0));

                indices.Add(indexCount);
                indices.Add(indexCount + 1);
                indices.Add(indexCount + 2);

                indices.Add(indexCount + 2);
                indices.Add(indexCount + 3);
                indices.Add(indexCount);

                indexCount += 4;
            }

            void AddRightFace(int x, int y, int z)
            {
                vertices.Add(new Vector3(1 + x, 1 + y, 0 + z));
                vertices.Add(new Vector3(1 + x, 1 + y, 1 + z));
                vertices.Add(new Vector3(1 + x, 0 + y, 1 + z));
                vertices.Add(new Vector3(1 + x, 0 + y, 0 + z));

                float lightVal = lightmap[x + WIDTH + 1, y, z + WIDTH]; //GetBlockLight(x + 1, y, z);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);

                uvs.Add(new Vector2(workingBlock.RightFace.UVCoordinates.X, workingBlock.RightFace.UVCoordinates.Y));
                uvs.Add(new Vector2(workingBlock.RightFace.UVCoordinates.Width, workingBlock.RightFace.UVCoordinates.Y));
                uvs.Add(new Vector2(workingBlock.RightFace.UVCoordinates.Width, workingBlock.RightFace.UVCoordinates.Height));
                uvs.Add(new Vector2(workingBlock.RightFace.UVCoordinates.X, workingBlock.RightFace.UVCoordinates.Height));

                uv2.Add(new Vector2(workingBlock.RightFace.Mask.X, workingBlock.RightFace.Mask.Y));
                uv2.Add(new Vector2(workingBlock.RightFace.Mask.Width, workingBlock.RightFace.Mask.Y));
                uv2.Add(new Vector2(workingBlock.RightFace.Mask.Width, workingBlock.RightFace.Mask.Height));
                uv2.Add(new Vector2(workingBlock.RightFace.Mask.X, workingBlock.RightFace.Mask.Height));

                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());

                normals.Add(new Vector3(1,0,0));
                normals.Add(new Vector3(1,0,0));
                normals.Add(new Vector3(1,0,0));
                normals.Add(new Vector3(1, 0, 0));

                indices.Add(indexCount);
                indices.Add(indexCount + 1);
                indices.Add(indexCount + 2);

                indices.Add(indexCount + 2);
                indices.Add(indexCount + 3);
                indices.Add(indexCount);

                indexCount += 4;
            }

            void AddLeftFace(int x, int y, int z)
            {
                vertices.Add(new Vector3(0 + x, 1 + y, 1 + z));
                vertices.Add(new Vector3(0 + x, 1 + y, 0 + z));
                vertices.Add(new Vector3(0 + x, 0 + y, 0 + z));
                vertices.Add(new Vector3(0 + x, 0 + y, 1 + z));

                float lightVal = lightmap[x + WIDTH - 1, y, z + WIDTH]; //GetBlockLight(x - 1, y, z);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);

                uvs.Add(new Vector2(workingBlock.LeftFace.UVCoordinates.X, workingBlock.LeftFace.UVCoordinates.Y));
                uvs.Add(new Vector2(workingBlock.LeftFace.UVCoordinates.Width, workingBlock.LeftFace.UVCoordinates.Y));
                uvs.Add(new Vector2(workingBlock.LeftFace.UVCoordinates.Width, workingBlock.LeftFace.UVCoordinates.Height));
                uvs.Add(new Vector2(workingBlock.LeftFace.UVCoordinates.X, workingBlock.LeftFace.UVCoordinates.Height));

                uv2.Add(new Vector2(workingBlock.LeftFace.Mask.X, workingBlock.LeftFace.Mask.Y));
                uv2.Add(new Vector2(workingBlock.LeftFace.Mask.Width, workingBlock.LeftFace.Mask.Y));
                uv2.Add(new Vector2(workingBlock.LeftFace.Mask.Width, workingBlock.LeftFace.Mask.Height));
                uv2.Add(new Vector2(workingBlock.LeftFace.Mask.X, workingBlock.LeftFace.Mask.Height));

                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());

                normals.Add(new Vector3(-1,0,0));
                normals.Add(new Vector3(-1, 0, 0));
                normals.Add(new Vector3(-1, 0, 0));
                normals.Add(new Vector3(-1, 0, 0));

                indices.Add(indexCount);
                indices.Add(indexCount + 1);
                indices.Add(indexCount + 2);

                indices.Add(indexCount + 2);
                indices.Add(indexCount + 3);
                indices.Add(indexCount);

                indexCount += 4;
            }

            void AddFrontFaceWater(int x, int y, int z)
            {
                verticesWater.Add(new Vector3(1 + x, 1 + y, 1 + z));
                verticesWater.Add(new Vector3(0 + x, 1 + y, 1 + z));
                verticesWater.Add(new Vector3(0 + x, 0 + y, 1 + z));
                verticesWater.Add(new Vector3(1 + x, 0 + y, 1 + z));

                uvsWater.Add(new Vector2(workingBlock.FrontFace.UVCoordinates.X, workingBlock.FrontFace.UVCoordinates.Y));
                uvsWater.Add(new Vector2(workingBlock.FrontFace.UVCoordinates.Width, workingBlock.FrontFace.UVCoordinates.Y));
                uvsWater.Add(new Vector2(workingBlock.FrontFace.UVCoordinates.Width, workingBlock.FrontFace.UVCoordinates.Height));
                uvsWater.Add(new Vector2(workingBlock.FrontFace.UVCoordinates.X, workingBlock.FrontFace.UVCoordinates.Height));

                normalsWater.Add(new Vector3(0, 0, 1));
                normalsWater.Add(new Vector3(0, 0, 1));
                normalsWater.Add(new Vector3(0, 0, 1));
                normalsWater.Add(new Vector3(0, 0, 1));

                indicesWater.Add(indexCountWater);
                indicesWater.Add(indexCountWater + 1);
                indicesWater.Add(indexCountWater + 2);

                indicesWater.Add(indexCountWater + 2);
                indicesWater.Add(indexCountWater + 3);
                indicesWater.Add(indexCountWater);

                indexCountWater += 4;
            }

            void AddBackFaceWater(int x, int y, int z)
            {
                verticesWater.Add(new Vector3(0 + x, 1 + y, 0 + z));
                verticesWater.Add(new Vector3(1 + x, 1 + y, 0 + z));
                verticesWater.Add(new Vector3(1 + x, 0 + y, 0 + z));
                verticesWater.Add(new Vector3(0 + x, 0 + y, 0 + z));

                uvsWater.Add(new Vector2(workingBlock.BackFace.UVCoordinates.X, workingBlock.BackFace.UVCoordinates.Y));
                uvsWater.Add(new Vector2(workingBlock.BackFace.UVCoordinates.Width, workingBlock.BackFace.UVCoordinates.Y));
                uvsWater.Add(new Vector2(workingBlock.BackFace.UVCoordinates.Width, workingBlock.BackFace.UVCoordinates.Height));
                uvsWater.Add(new Vector2(workingBlock.BackFace.UVCoordinates.X, workingBlock.BackFace.UVCoordinates.Height));

                normalsWater.Add(new Vector3(0, 0, -1));
                normalsWater.Add(new Vector3(0, 0, -1));
                normalsWater.Add(new Vector3(0, 0, -1));
                normalsWater.Add(new Vector3(0, 0, -1));

                indicesWater.Add(indexCountWater);
                indicesWater.Add(indexCountWater + 1);
                indicesWater.Add(indexCountWater + 2);

                indicesWater.Add(indexCountWater + 2);
                indicesWater.Add(indexCountWater + 3);
                indicesWater.Add(indexCountWater);

                indexCountWater += 4;
            }

            void AddTopFaceWater(int x, int y, int z)
            {
                verticesWater.Add(new Vector3(1 + x, 1 + y, 0 + z));
                verticesWater.Add(new Vector3(0 + x, 1 + y, 0 + z));
                verticesWater.Add(new Vector3(0 + x, 1 + y, 1 + z));
                verticesWater.Add(new Vector3(1 + x, 1 + y, 1 + z));

                uvsWater.Add(new Vector2(workingBlock.TopFace.UVCoordinates.X, workingBlock.TopFace.UVCoordinates.Y));
                uvsWater.Add(new Vector2(workingBlock.TopFace.UVCoordinates.Width, workingBlock.TopFace.UVCoordinates.Y));
                uvsWater.Add(new Vector2(workingBlock.TopFace.UVCoordinates.Width, workingBlock.TopFace.UVCoordinates.Height));
                uvsWater.Add(new Vector2(workingBlock.TopFace.UVCoordinates.X, workingBlock.TopFace.UVCoordinates.Height));

                normalsWater.Add(new Vector3(0, 1, 0));
                normalsWater.Add(new Vector3(0, 1, 0));
                normalsWater.Add(new Vector3(0, 1, 0));
                normalsWater.Add(new Vector3(0, 1, 0));

                indicesWater.Add(indexCountWater);
                indicesWater.Add(indexCountWater + 1);
                indicesWater.Add(indexCountWater + 2);

                indicesWater.Add(indexCountWater + 2);
                indicesWater.Add(indexCountWater + 3);
                indicesWater.Add(indexCountWater);

                indexCountWater += 4;
            }

            void AddBottomFaceWater(int x, int y, int z)
            {
                verticesWater.Add(new Vector3(1 + x, 0 + y, 1 + z));
                verticesWater.Add(new Vector3(0 + x, 0 + y, 1 + z));
                verticesWater.Add(new Vector3(0 + x, 0 + y, 0 + z));
                verticesWater.Add(new Vector3(1 + x, 0 + y, 0 + z));

                uvsWater.Add(new Vector2(workingBlock.BottomFace.UVCoordinates.X, workingBlock.BottomFace.UVCoordinates.Y));
                uvsWater.Add(new Vector2(workingBlock.BottomFace.UVCoordinates.Width, workingBlock.BottomFace.UVCoordinates.Y));
                uvsWater.Add(new Vector2(workingBlock.BottomFace.UVCoordinates.Width, workingBlock.BottomFace.UVCoordinates.Height));
                uvsWater.Add(new Vector2(workingBlock.BottomFace.UVCoordinates.X, workingBlock.BottomFace.UVCoordinates.Height));

                normalsWater.Add(new Vector3(0, -1, 0));
                normalsWater.Add(new Vector3(0, -1, 0));
                normalsWater.Add(new Vector3(0, -1, 0));
                normalsWater.Add(new Vector3(0, -1, 0));

                indicesWater.Add(indexCountWater);
                indicesWater.Add(indexCountWater + 1);
                indicesWater.Add(indexCountWater + 2);

                indicesWater.Add(indexCountWater + 2);
                indicesWater.Add(indexCountWater + 3);
                indicesWater.Add(indexCountWater);

                indexCountWater += 4;
            }

            void AddRightFaceWater(int x, int y, int z)
            {
                verticesWater.Add(new Vector3(1 + x, 1 + y, 0 + z));
                verticesWater.Add(new Vector3(1 + x, 1 + y, 1 + z));
                verticesWater.Add(new Vector3(1 + x, 0 + y, 1 + z));
                verticesWater.Add(new Vector3(1 + x, 0 + y, 0 + z));

                uvsWater.Add(new Vector2(workingBlock.RightFace.UVCoordinates.X, workingBlock.RightFace.UVCoordinates.Y));
                uvsWater.Add(new Vector2(workingBlock.RightFace.UVCoordinates.Width, workingBlock.RightFace.UVCoordinates.Y));
                uvsWater.Add(new Vector2(workingBlock.RightFace.UVCoordinates.Width, workingBlock.RightFace.UVCoordinates.Height));
                uvsWater.Add(new Vector2(workingBlock.RightFace.UVCoordinates.X, workingBlock.RightFace.UVCoordinates.Height));

                normalsWater.Add(new Vector3(1, 0, 0));
                normalsWater.Add(new Vector3(1, 0, 0));
                normalsWater.Add(new Vector3(1, 0, 0));
                normalsWater.Add(new Vector3(1, 0, 0));

                indicesWater.Add(indexCountWater);
                indicesWater.Add(indexCountWater + 1);
                indicesWater.Add(indexCountWater + 2);

                indicesWater.Add(indexCountWater + 2);
                indicesWater.Add(indexCountWater + 3);
                indicesWater.Add(indexCountWater);

                indexCountWater += 4;
            }

            void AddLeftFaceWater(int x, int y, int z)
            {
                verticesWater.Add(new Vector3(0 + x, 1 + y, 1 + z));
                verticesWater.Add(new Vector3(0 + x, 1 + y, 0 + z));
                verticesWater.Add(new Vector3(0 + x, 0 + y, 0 + z));
                verticesWater.Add(new Vector3(0 + x, 0 + y, 1 + z));

                uvsWater.Add(new Vector2(workingBlock.LeftFace.UVCoordinates.X, workingBlock.LeftFace.UVCoordinates.Y));
                uvsWater.Add(new Vector2(workingBlock.LeftFace.UVCoordinates.Width, workingBlock.LeftFace.UVCoordinates.Y));
                uvsWater.Add(new Vector2(workingBlock.LeftFace.UVCoordinates.Width, workingBlock.LeftFace.UVCoordinates.Height));
                uvsWater.Add(new Vector2(workingBlock.LeftFace.UVCoordinates.X, workingBlock.LeftFace.UVCoordinates.Height));

                normalsWater.Add(new Vector3(-1, 0, 0));
                normalsWater.Add(new Vector3(-1, 0, 0));
                normalsWater.Add(new Vector3(-1, 0, 0));
                normalsWater.Add(new Vector3(-1, 0, 0));

                indicesWater.Add(indexCountWater);
                indicesWater.Add(indexCountWater + 1);
                indicesWater.Add(indexCountWater + 2);

                indicesWater.Add(indexCountWater + 2);
                indicesWater.Add(indexCountWater + 3);
                indicesWater.Add(indexCountWater);

                indexCountWater += 4;
            }

            blockContainer = new VertexBlockContainer(vertices.ToArray(), uvs.ToArray(), normals.ToArray(), uv2.ToArray(), col.ToArray(), light.ToArray());
            vertices.Clear();
            uvs.Clear();
            normals.Clear();
            uv2.Clear();
            col.Clear();
            light.Clear();

            waterContainer = new VertexNormalContainer(verticesWater.ToArray(), uvsWater.ToArray(), normalsWater.ToArray());
            verticesWater.Clear();
            uvsWater.Clear();
            normalsWater.Clear();

            this.indices = indices.ToArray();
            this.indicesWater = indicesWater.ToArray();
            indices.Clear();
            indicesWater.Clear();

            shouldRebuildWaterMesh = true;
            shouldRebuildMesh = true;
        }

        public bool ShouldDrawBlockFacing(int x, int y, int z, int workingBlockID)
        {
            short block = GetBlockID(x, y, z);

            if (block == 0 || block == -1)
                return true;

            var theBlock = BlockDatabase.GetBlock(block);

            if (theBlock.IsTransparent)
            {
                //Remove above return for faster trees (possible setting)
                if (block != workingBlockID)
                    return true;

                if(theBlock.TransparencyCullsSelf)
                    return false;
                
                return true;
            }

            return false;
        }

        public BlockState GetBlockState(int x, int y, int z)
        {
            if (x <= -1)
            {
                if (LeftNeighbour != null)
                    return LeftNeighbour.GetBlockState(WIDTH + x, y, z);

                return null;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                    return RightNeighbour.GetBlockState(x - WIDTH, y, z);

                return null;
            }

            if (z <= -1)
            {
                if (BackNeighbour != null)
                    return BackNeighbour.GetBlockState(x, y, WIDTH + z);

                return null;
            }

            if (z >= WIDTH)
            {
                if (FrontNeighbour != null)
                    return FrontNeighbour.GetBlockState(x, y, z - WIDTH);

                return null;
            }

            if (y < 0 || y > HEIGHT - 1)
                return null;

            if (Blocks[x, y, z] == null)
                return null;

            return Blocks[x, y, z];
        }

        public short GetBlockID(int x, int y, int z)
        {
            if (x <= -1)
            {
                if (LeftNeighbour != null)
                    return LeftNeighbour.GetBlockID(WIDTH + x, y, z);

                return -1;
            }

            if (x >= WIDTH)
            {
                if(RightNeighbour != null)
                    return RightNeighbour.GetBlockID(x - WIDTH, y, z);

                return -1;
            }

            if (z <= -1)
            {
                if(BackNeighbour != null)
                    return BackNeighbour.GetBlockID(x, y, WIDTH + z);

                return -1;
            }

            if (z >= WIDTH)
            {
                if (FrontNeighbour != null)
                    return FrontNeighbour.GetBlockID(x, y, z - WIDTH);

                return -1;
            }

            if (y < 0 || y > HEIGHT - 1)
                return 0;

            if (Blocks[x, y, z] == null)
                return 0;

            return Blocks[x, y, z].id;
        }

        public int GetHeightAtBlock(int x, int z)
        {
            if (x <= -1)
            {
                if (LeftNeighbour != null)
                    return LeftNeighbour.GetHeightAtBlock(WIDTH + x, z);

                return 0;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                    return RightNeighbour.GetHeightAtBlock(x - WIDTH, z);

                return 0;
            }

            if (z <= -1)
            {
                if (BackNeighbour != null)
                    return BackNeighbour.GetHeightAtBlock(x, WIDTH + z);

                return 0;
            }

            if (z >= WIDTH)
            {
                if (FrontNeighbour != null)
                    return FrontNeighbour.GetHeightAtBlock(x, z - WIDTH);

                return 0;
            }

            int h = (int)(heightmap[x, z] / 4);
            h += 16;
            return h;
        }

        /*public byte GetBlockLightRaw(int x, int y, int z)
        {
            if (x <= -1)
            {
                if (LeftNeighbour != null)
                    return LeftNeighbour.GetBlockLightRaw(WIDTH + x, y, z);

                return 0;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                    return RightNeighbour.GetBlockLightRaw(x - WIDTH, y, z);

                return 0;
            }

            if (z <= -1)
            {
                if (BackNeighbour != null)
                    return BackNeighbour.GetBlockLightRaw(x, y, WIDTH + z);

                return 0;
            }

            if (z >= WIDTH)
            {
                if (FrontNeighbour != null)
                    return FrontNeighbour.GetBlockLightRaw(x, y, z - WIDTH);

                return 0;
            }

            if (y < 0 || y > HEIGHT - 1)
                return 0;

            return lightmap[x, y, z];
        }

        public float GetBlockLight(int x, int y, int z)
        {
            return BlockState.LIGHTUNIT * (float) GetBlockLightRaw(x, y, z);
        }

        public void SetBlockLight(int x, int y, int z, byte light)
        {
            if (x <= -1)
            {
                if (LeftNeighbour != null)
                    LeftNeighbour.SetBlockLight(WIDTH + x, y, z, light);
            }
            else if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                    RightNeighbour.SetBlockLight(x - WIDTH, y, z, light);
            }
            else if (z <= -1)
            {
                if (BackNeighbour != null)
                    BackNeighbour.SetBlockLight(x, y, WIDTH + z, light);
            }
            else if (z >= WIDTH)
            {
                if (FrontNeighbour != null)
                    FrontNeighbour.SetBlockLight(x, y, z - WIDTH, light);
            }
            else if (!(y < 0 || y > HEIGHT - 1) && Blocks[x, y, z] != null)
                lightmap[x, y, z] = light;
        }
        */
        public void PlaceBlock(int x, int y, int z, Block block, bool updateChunk = true)
        {
            PlaceBlock(x, y, z, (short)block.ID, updateChunk);
        }

        public void PlaceBlock(int x, int y, int z, short blockIndex, bool updateChunk = true)
        {
            if (y >= HEIGHT - 1)
            {
                Debug.Log($"Tried placing a block at: {x},{y},{z} but the Y value is too high!", DebugLevel.Warning);
                return;
            }

            if (x <= -1)
            {
                if (LeftNeighbour != null)
                    LeftNeighbour.PlaceBlock(WIDTH + x, y, z, blockIndex, updateChunk);
            }
            else if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                    RightNeighbour.PlaceBlock(x - WIDTH, y, z, blockIndex, updateChunk);
            }
            else if (z <= -1)
            {
                if (BackNeighbour != null)
                    BackNeighbour.PlaceBlock(x, y, WIDTH + z, blockIndex, updateChunk);
            }
            else if (z >= WIDTH)
            {
                if (FrontNeighbour != null)
                    FrontNeighbour.PlaceBlock(x, y, z - WIDTH, blockIndex, updateChunk);
            }
            else if (Blocks[x, y, z] != null)
            {
                Blocks[x, y, z].id = blockIndex;
                Rebuild();
            }
            else if(y < HEIGHT - 1)
            {
                Blocks[x, y, z] = new BlockState((sbyte)x, (sbyte)y, (sbyte)z, this);
                Blocks[x, y, z].id = blockIndex;
                Rebuild();
            }

            void Rebuild()
            {
                if (updateChunk)
                {
                    World.GetInstance().RequestChunkUpdate(this, x, z, true);
                }

                if (y > GetHeightAtBlock(x, z))
                {
                    int newY = y - 16;
                    newY *= 4;
                    int oldHeight = (int)heightmap[x, z];
                    heightmap[x, z] = newY;
                    Debug.Log("Updated the heightmap from: " + oldHeight + " to " + newY);
                }
            }
        }

        public void DestroyBlock(int x, int y, int z)
        {
            PlaceBlock(x, y, z, 0, true);
            if (y == GetHeightAtBlock(x, z))
            {
                int newY = y - 16;
                newY *= 4;
                int oldHeight = (int)heightmap[x, z];
                heightmap[x, z] = newY;
                Debug.Log("Updated the heightmap from: " + oldHeight + " to " + newY);
            }
        }

        public void Render()
        {
            if (shouldRebuildMesh)
            {
                if (ChunkMaterial == null)
                {
                    ChunkMaterial = AssetDatabase.GetAsset<Material>("Resources/Materials/World/Blocks.mat");
                    ChunkMaterial.SetTexture(0, World.GetInstance().TexturePack.Blocks);
                }

                mesh?.Dispose();
                mesh = new Mesh(blockContainer, indices);
                shouldRebuildMesh = false;
            }

            if (shouldRebuildWaterMesh)
            {
                if (ChunkWaterMaterial == null)
                {
                    ChunkWaterMaterial = AssetDatabase.GetAsset<Material>("Resources/Materials/World/Water.mat");
                    ChunkWaterMaterial.SetTexture(0, World.GetInstance().TexturePack.Blocks);
                }

                waterMesh?.Dispose();
                waterMesh = new Mesh(waterContainer, indicesWater);
                shouldRebuildWaterMesh = false;
            }

            if(waterMesh != null)
                Renderer.DrawRequest(waterMesh, ChunkWaterMaterial, worldMatrix);
            if(mesh != null)
                Renderer.DrawRequest(mesh, ChunkMaterial, worldMatrix);
        }

        public void Dispose()
        {
            if (RightNeighbour != null)
            {
                RightNeighbour.LeftNeighbour = null;
                RightNeighbour = null;
            }

            if (LeftNeighbour != null)
            {
                LeftNeighbour.RightNeighbour = null;
                LeftNeighbour = null;
            }

            if (BackNeighbour != null)
            {
                BackNeighbour.FrontNeighbour = null;
                BackNeighbour = null;
            }

            if (FrontNeighbour != null)
            {
                FrontNeighbour.BackNeighbour = null;
                FrontNeighbour = null;
            }

            mesh?.Dispose();
            waterMesh?.Dispose();
        }
    }
}
