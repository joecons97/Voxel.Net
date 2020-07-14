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
        public enum NeighbourDirection
        {
            Left,
            Right,
            Front,
            Back
        }

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
        public bool IsMeshGenerated
        {
            get { return mesh != null; }
        }

        public Chunk LeftNeighbour;
        public Chunk RightNeighbour;
        public Chunk FrontNeighbour;
        public Chunk BackNeighbour;

        public short[,,] Blocks = new short[WIDTH,HEIGHT,WIDTH];

        private Mesh mesh;
        private Mesh waterMesh;
        private Material material;
        private Material waterMaterial;
        private float[,] heightmap;

        private float noiseScale = 0.25f;

        public Chunk(Vector2 position)
        {
            Position = position;
        }

        public void UpdateNeighbour(Chunk chunk, NeighbourDirection neighbour)
        {
            switch (neighbour)
            {
                case NeighbourDirection.Left:
                    if (LeftNeighbour == null)
                        LeftNeighbour = chunk;
                    break;
                case NeighbourDirection.Right:
                    if (RightNeighbour == null)
                        RightNeighbour = chunk;
                    break;
                case NeighbourDirection.Front:
                    if (FrontNeighbour == null)
                        FrontNeighbour = chunk;
                    break;
                case NeighbourDirection.Back:
                    if (BackNeighbour == null)
                        BackNeighbour = chunk;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(neighbour), neighbour, null);
            }
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
                                               .Octaves2D(NoiseX, NoiseY, 1, .4f, 2, noiseScale * 0.125f) + 1) / 2) * 6;

                    ocean -= 2;
                    ocean = (float)Math.Pow(MathHelper.Clamp(ocean, 0, 1) + (mainNoise/10f), 0.8f);

                    heightmap[x, y] = Math.Min(mainNoise, ocean) * 255f;
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
                            if (y > h)
                            {
                                if(y <= World.GetInstance().WaterHeight)
                                    Blocks[x, y, z] = (short)GameBlocks.WATER.ID;

                            }
                            else if (y == h)
                            {
                                if(y < World.GetInstance().WaterHeight + 3)
                                    Blocks[x, y, z] = (short)GameBlocks.SAND.ID;
                                else
                                {
                                    Blocks[x, y, z] = (short)GameBlocks.GRASS.ID;
                                    decorator.DecorateAtBlock(this, x, y, z);
                                }

                            }
                            else if (y > h - 5)
                            {
                                if (y < World.GetInstance().WaterHeight + 3)
                                    Blocks[x, y, z] = (short)GameBlocks.SAND.ID;
                                else
                                    Blocks[x, y, z] = (short)GameBlocks.DIRT.ID;
                            }
                            else
                            {
                                Blocks[x, y, z] = (short)GameBlocks.STONE.ID;
                            }
                        }
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

            VertexNormalContainer WaterContainer =
                new VertexNormalContainer(verticesWater.ToArray(), uvsWater.ToArray(), normalsWater.ToArray());

            waterMaterial = AssetDatabase.GetAsset<Material>("Resources/Materials/World/Water.mat");
            waterMaterial.SetTexture(0, World.GetInstance().TexturePack.Blocks);

            waterMesh = new Mesh(WaterContainer, indicesWater.ToArray());

            VertexNrmUv2ColContainer container =
                new VertexNrmUv2ColContainer(vertices.ToArray(), uvs.ToArray(), normals.ToArray(), uv2.ToArray(), col.ToArray());

            material = AssetDatabase.GetAsset<Material>("Resources/Materials/World/Blocks.mat");
            material.SetTexture(0, World.GetInstance().TexturePack.Blocks);

            mesh = new Mesh(container, indices.ToArray());
        }

        public bool ShouldDrawBlockFacing(int x, int y, int z, int workingBlockID)
        {
            short block = GetBlockID(x, y, z);

            if (block == 0)
                return true;

            if (BlockDatabase.GetBlock(block).IsTransparent)
            {
                if (block != workingBlockID)
                    return true;
                else
                    return false;
            }

            return false;
        }

        public short GetBlockID(int x, int y, int z)
        {
            if (x == -1)
            {
                if (LeftNeighbour != null)
                    return LeftNeighbour.GetBlockID(WIDTH - 1, y, z);

                return -1;
            }

            if (x == WIDTH)
            {
                if(RightNeighbour != null)
                    return RightNeighbour.GetBlockID(0, y, z);

                return -1;
            }

            if (z == -1)
            {
                if(BackNeighbour != null)
                    return BackNeighbour.GetBlockID(x, y, WIDTH - 1);

                return -1;
            }

            if (z == WIDTH)
            {
                if (FrontNeighbour != null)
                    return FrontNeighbour.GetBlockID(x, y, 0);

                return -1;
            }

            if (y < 0 || y > HEIGHT - 1)
                return 0;

            return Blocks[x, y, z];
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

                return;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbour != null)
                    RightNeighbour.PlaceBlock(x - WIDTH, y, z, blockIndex, updateChunk);

                return;
            }

            if (z <= -1)
            {
                if (BackNeighbour != null)
                    BackNeighbour.PlaceBlock(x, y, WIDTH + z, blockIndex, updateChunk);

                return;
            }

            if (z >= WIDTH)
            {
                if (FrontNeighbour != null)
                    FrontNeighbour.PlaceBlock(x, y, z - WIDTH, blockIndex, updateChunk);

                return;
            }

            int layer = (int)Math.Floor((float) y / (float) (HEIGHT / LAYERCOUNT));
            Blocks[x, y, z] = blockIndex;

            if(updateChunk)
                World.GetInstance().RequestChunkUpdate(this);
        }

        public void DestroyBlock(int x, int y, int z)
        {
            PlaceBlock(x, y, z, 0);

            if(x == 0 && LeftNeighbour != null)
                World.GetInstance().RequestChunkUpdate(LeftNeighbour);

            if (x == WIDTH - 1 && RightNeighbour != null)
                World.GetInstance().RequestChunkUpdate(RightNeighbour);

            if (z == 0 && BackNeighbour != null)
                World.GetInstance().RequestChunkUpdate(BackNeighbour);

            if (z == WIDTH - 1 && FrontNeighbour != null)
                World.GetInstance().RequestChunkUpdate(FrontNeighbour);
        }

        public void Render()
        {
            if (!IsMeshGenerated)
                return;

            var mat = Matrix4.CreateTranslation(Position.X * WIDTH, 0, Position.Y * WIDTH);
            Renderer.DrawRequest(waterMesh, waterMaterial, mat);
            Renderer.DrawRequest(mesh, material, mat);
        }

        public void Dispose()
        {
            if (RightNeighbour != null)
                RightNeighbour.LeftNeighbour = null;

            if (LeftNeighbour != null)
                LeftNeighbour.RightNeighbour = null;

            if (BackNeighbour != null)
                BackNeighbour.FrontNeighbour = null;

            if (FrontNeighbour != null)
                FrontNeighbour.BackNeighbour = null;

            mesh?.Dispose();
            waterMesh?.Dispose();
        }
    }
}
