using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using SimplexNoise;
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
            public byte x;
            public byte y;
            public byte z;
            private byte lightStrength = 15;

            public BlockState(byte x, byte y, byte z, Chunk chunk)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public byte LightStrength
            {
                get { return lightStrength;}
                set
                {
                    lightStrength = value;
                }
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

        Vector3[] faceCheckOffsets =
        {
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0),
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
        };

        #region MeshData
        private VertexBlockContainer blockContainer;
        private VertexNormalContainer waterContainer;

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

                    heightmap[x, y] = Math.Min(mainNoise + 0.2f, ocean) * 255f;
                }
            }
        }

        public int GetHeightAtBlock(int x, int z)
        {
            int h = (int)heightmap[x, z] / 4;
            h += 16;
            return h;
        }

        public void FillBlocks()
        {
            using (OakDecorator decorator = new OakDecorator())
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    for (int z = 0; z < WIDTH; z++)
                    {
                        int h = GetHeightAtBlock(x, z);
                        for (int y = 0; y < HEIGHT; y++)
                        {
                            Blocks[x, y, z] = new BlockState((byte) x, (byte) y, (byte) z, this);
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
                                    decorator.DecorateAtBlock(this, x, y, z);
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
            }
        }

        void UpdateLight(int x, int z, int startY)
        {
            if (startY > HEIGHT - 1)
            {
                startY = HEIGHT - 1;
                Debug.Log("Attempted to cast natural light from above the world");
            }

            bool isObstructed = false;
            for (int y = startY - 1; y > -1; y--)
            {
                BlockState state = Blocks[x, y, z];

                if (isObstructed)
                {
                    state.LightStrength = 1;
                }
                else if (state.id != 0 && !BlockDatabase.GetBlock(state.id).IsTransparent)
                {
                    state.LightStrength = 1;
                    isObstructed = true;
                }
                else
                    state.LightStrength = 15;
            }
        }

        public void CalculateNaturalLight()
        {
            for (int x = 0; x < WIDTH; x++)
            {
                for (int z = 0; z < WIDTH; z++)
                {
                    UpdateLight(x, z, HEIGHT - 1);
                }
            }
        }

        /*public void PropagateBlockState(BlockState state)
        {
            if (state.LightStrength < BlockState.LIGHTUNIT * 2)
                return;

            for (int i = 0; i < 6; i++)
            {
                var neighbour = state.GetNeighbour(i);
                if (neighbour != null)
                {
                    if (neighbour.LightStrength < state.LightStrength - BlockState.LIGHTUNIT)
                    {
                        neighbour.LightStrength = state.LightStrength - BlockState.LIGHTUNIT;
                    }
                }
            }
        }
        */

        public bool IsInChunk(int x, int y, int z)
        {
            return x >= 0 && x < WIDTH && z >= 0 && z < WIDTH && y >= 0 && y < HEIGHT;
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

                float lightVal = GetBlockLight(x, y, z + 1);
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

                float lightVal = GetBlockLight(x, y, z - 1);
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

                float lightVal = GetBlockLight(x, y + 1, z);
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

                float lightVal = GetBlockLight(x, y - 1, z);
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

                float lightVal = GetBlockLight(x + 1, y, z);
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

                float lightVal = GetBlockLight(x - 1, y, z);
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

            mesh?.Dispose();
            mesh = null;

            waterMesh?.Dispose();
            waterMesh = null;
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

        public float GetBlockLight(int x, int y, int z)
        {
            if (x <= -1)
            {
                if (LeftNeighbour != null)
                    return LeftNeighbour.GetBlockLight(WIDTH + x, y, z);

                return 0;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                    return RightNeighbour.GetBlockLight(x - WIDTH, y, z);

                return 0;
            }

            if (z <= -1)
            {
                if (BackNeighbour != null)
                    return BackNeighbour.GetBlockLight(x, y, WIDTH + z);

                return 0;
            }

            if (z >= WIDTH)
            {
                if (FrontNeighbour != null)
                    return FrontNeighbour.GetBlockLight(x, y, z - WIDTH);

                return 0;
            }

            if (y < 0 || y > HEIGHT - 1)
                return 0;

            return BlockState.LIGHTUNIT*(float)Blocks[x, y, z].LightStrength;
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
                Blocks[x, y, z].LightStrength = light;
        }

        public void PlaceBlock(int x, int y, int z, Block block, bool updateChunk = true)
        {
            PlaceBlock(x, y, z, (short)block.ID, updateChunk);
        }

        public void PlaceBlock(int x, int y, int z, short blockIndex, bool updateChunk = true)
        {
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

            if (Blocks[x, y, z] != null)
            {
                Blocks[x, y, z].id = blockIndex;
                if (updateChunk)
                {
                    World.GetInstance().RequestChunkUpdate(this);

                    if (x == 0 && LeftNeighbour != null)
                        World.GetInstance().RequestChunkUpdate(LeftNeighbour, true);

                    if (x == WIDTH - 1 && RightNeighbour != null)
                        World.GetInstance().RequestChunkUpdate(RightNeighbour, true);

                    if (z == 0 && BackNeighbour != null)
                        World.GetInstance().RequestChunkUpdate(BackNeighbour, true);

                    if (z == WIDTH - 1 && FrontNeighbour != null)
                        World.GetInstance().RequestChunkUpdate(FrontNeighbour, true);
                }
            }
        }

        public void DestroyBlock(int x, int y, int z)
        {
            PlaceBlock(x, y, z, 0);

            if(x == 0 && LeftNeighbour != null)
                World.GetInstance().RequestChunkUpdate(LeftNeighbour, true);

            if (x == WIDTH - 1 && RightNeighbour != null)
                World.GetInstance().RequestChunkUpdate(RightNeighbour, true);

            if (z == 0 && BackNeighbour != null)
                World.GetInstance().RequestChunkUpdate(BackNeighbour, true);

            if (z == WIDTH - 1 && FrontNeighbour != null)
                World.GetInstance().RequestChunkUpdate(FrontNeighbour, true);
        }

        public void Render()
        {
            if (waterMesh == null || mesh == null)
            {
                if (indices != null)
                {
                    mesh = new Mesh(blockContainer, indices);
                }
                if (indicesWater != null)
                {
                    waterMesh = new Mesh(waterContainer, indicesWater);
                }

                if (ChunkMaterial == null)
                {
                    ChunkMaterial = AssetDatabase.GetAsset<Material>("Resources/Materials/World/Blocks.mat");
                    ChunkMaterial.SetTexture(0, World.GetInstance().TexturePack.Blocks);
                }

                if (ChunkWaterMaterial == null)
                {
                    ChunkWaterMaterial = AssetDatabase.GetAsset<Material>("Resources/Materials/World/Water.mat");
                    ChunkWaterMaterial.SetTexture(0, World.GetInstance().TexturePack.Blocks);
                }

                if (indices == null && indicesWater == null)
                    return;
            }

            if(waterMesh != null)
                Renderer.DrawRequest(waterMesh, ChunkWaterMaterial, worldMatrix);
            if(mesh != null)
                Renderer.DrawRequest(mesh, ChunkMaterial, worldMatrix);
        }

        public void Dispose()
        {
            //Debug.Log("Disposing chunk " + Position.ToString());
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
