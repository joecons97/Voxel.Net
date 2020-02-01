using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics;
using TrueCraft.Core.TerrainGen.Noise;
using VoxelNet.Buffers;
using VoxelNet.Buffers.Ubos;
using VoxelNet.Misc;
using VoxelNet.Physics;
using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;
using VoxelNet.Entities;

namespace VoxelNet.Assets
{
    public class World : IImportable, IExportable, IDisposable
    {
        public string Name { get; }
        public string Seed { get; }

        public Camera WorldCamera { get; private set; }
        public Skybox Skybox { get; private set; }
        public TexturePack TexturePack { get; private set; }
        public Random Randomizer { get; private set; }
        public OpenSimplex TerrainNoise { get; private set; }
        public CellNoise BiomeNoise { get; private set; }

        List<Entity> loadedEntities = new List<Entity>();
        List<Chunk> loadedChunks = new List<Chunk>();

        LinkedList<Chunk> chunksToUpdate = new LinkedList<Chunk>();
        private int worldSize = 6;

        private static World instance;
        
        private LightingUniformBufferData lightBufferData;
        private float lightAngle;

        private Vector2 lastPlayerPos = Vector2.One;
        List<Vector2> chunksToKeep = new List<Vector2>();
        List<Vector2> newChunks = new List<Vector2>();

        [JsonIgnore]
        public string Path =>
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\Voxel.Net\\{Name}\\{Name}.world";

        public static World GetInstance()
        {
            return instance;
        }

        public World()
        {
            if (instance != null)
            {
                Dispose();
            }

            Begin();
        }

        public World(string name, string seed)
        {
            if (instance != null)
            {
                Dispose();
                return;
            }

            instance = this;

            Name = name;
            Seed = seed;

            loadedEntities.Add(new Player());

            Begin();
        }

        public Chunk[] GetLoadedChunks()
        {
            return loadedChunks.ToArray();
        }

        public void Begin()
        {
            BlockDatabase.Init();
            TexturePack = AssetDatabase.GetAsset<TexturePack>("");
            TerrainNoise = new OpenSimplex(Seed.GetHashCode());
            BiomeNoise = new CellNoise(Seed.GetHashCode());
            Randomizer = new Random(Seed.GetHashCode());
            WorldCamera = new Camera();

            Skybox = new Skybox(AssetDatabase.GetAsset<Material>("Resources/Materials/Sky.mat"));

            lightAngle = 60;
            lightBufferData = new LightingUniformBufferData();

            foreach (var entity in loadedEntities)
            {
                entity.Begin();
            }

            //Check for custom texture pack
        }

        public void RequestChunkUpdate(Chunk chunk)
        {
            chunksToUpdate.AddFirst(chunk);
        }

        public bool TryGetChunkAtPosition(int x, int y, out Chunk chunk)
        {
            var c = loadedChunks.FirstOrDefault(v => v.Position.X == x && v.Position.Y == y);
            if (c != null)
            {
                chunk = c;
                return true;
            }

            chunk = null;
            return false;
        }

        public void Update()
        {
            if (TexturePack == null) return;

            if (chunksToUpdate.Count > 0)
            {
                if (chunksToUpdate.First.Value.AreAllNeighboursSet)
                {
                    Chunk chunk = chunksToUpdate.First.Value;
                    chunksToUpdate.RemoveFirst();
                    chunk.GenerateMesh();
                }
                else
                {
                    Chunk chunk = chunksToUpdate.First.Value;
                    chunksToUpdate.RemoveFirst();
                    chunksToUpdate.AddLast(chunk);
                }
            }

            foreach (var entity in loadedEntities)
            {
                entity.Update();
            }

            float dayInMins = 10f;

            lightAngle += Time.DeltaTime * (6f/dayInMins);

            float colTime = (float) Math.Abs(Math.Cos(MathHelper.DegreesToRadians(lightAngle)));
            colTime = (float)Math.Pow(colTime, 10);

            lightBufferData.SunColour = Vector4.Lerp(Color4.LightYellow.ToVector4(), Color4.OrangeRed.ToVector4(), colTime);
            lightBufferData.SunDirection = new Vector4(Maths.GetForwardFromRotation(new Vector3(lightAngle, 0, 0)), 1);

            float t = Vector3.Dot(lightBufferData.SunDirection.Xyz, new Vector3(0, -1, 0));
            t = (float)Math.Pow(t, .25f) * 1.5f;

            lightBufferData.SunStrength = t;

            if (float.IsNaN(t))
                t = .0f;

            lightBufferData.AmbientColour = Vector4.Lerp(Color4.DarkSlateGray.ToVector4()/3f, Color4.DarkSlateGray.ToVector4(), t);

            UpdateView();
        }

        public void GUI()
        {
            foreach (var entity in loadedEntities)
            {
                entity.GUI();
            }
        }

        void UpdateView()
        {
            int roundedX = (int)Math.Ceiling(WorldCamera.Position.X / 16) * 16;
            int roundedZ = (int)Math.Ceiling(WorldCamera.Position.Z / 16) * 16;

            if (roundedX != (int)lastPlayerPos.X || roundedZ != (int)lastPlayerPos.Y)
            {
                for (int x = -worldSize - 1; x < worldSize; x++)
                {
                    for (int z = -worldSize - 1; z < worldSize; z++)
                    {
                        int wantedX = x + (roundedX / 16);
                        int wantedZ = z + (roundedZ / 16);

                        if (TryGetChunkAtPosition(wantedX, wantedZ, out Chunk oChunk))
                        {
                            chunksToKeep.Add(new Vector2(wantedX, wantedZ));
                            continue;
                        }

                        Chunk c = new Chunk(new Vector2(wantedX, wantedZ));
                        newChunks.Add(c.Position);
                        c.GenerateHeightMap();
                        c.FillBlocks();
                        if (TryGetChunkAtPosition(wantedX - 1, wantedZ, out oChunk))
                        {
                            c.LeftNeighbour = oChunk;
                            oChunk.RightNeighbour = c;
                        }

                        if (TryGetChunkAtPosition(wantedX + 1, wantedZ, out oChunk))
                        {
                            c.RightNeighbour = oChunk;
                            oChunk.LeftNeighbour = c;
                        }
                        if (TryGetChunkAtPosition(wantedX, wantedZ - 1, out oChunk))
                        {
                            c.BackNeighbour = oChunk;
                            oChunk.FrontNeighbour = c;
                        }

                        if (TryGetChunkAtPosition(wantedX, wantedZ + 1, out oChunk))
                        {
                            c.FrontNeighbour = oChunk;
                            oChunk.BackNeighbour = c;
                        }
                        loadedChunks.Add(c);
                        RequestChunkUpdate(c);
                    }
                }

                for (int i = 0; i < loadedChunks.Count; i++)
                {
                    if (chunksToKeep.Any(v => (int)v.X == (int)loadedChunks[i].Position.X && (int)v.Y == (int)loadedChunks[i].Position.Y) ||
                        newChunks.Any(v => (int)v.X == (int)loadedChunks[i].Position.X && (int)v.Y == (int)loadedChunks[i].Position.Y))
                        continue;

                    loadedChunks[i].Dispose();
                    loadedChunks.Remove(loadedChunks[i]);
                }

                chunksToKeep.Clear();
                newChunks.Clear();
                lastPlayerPos = new Vector2(roundedX, roundedZ);
            }

            
        }

        public void Render()
        {
            UniformBuffers.DirectionLightBuffer.Update(lightBufferData);
            WorldCamera.Update();
            Skybox.Render();
            foreach (var entity in loadedEntities)
            {
                entity.Render();
            }
            foreach (var loadedChunk in loadedChunks)
            {
                loadedChunk.Render();
            }
        }

        public IImportable Import(string path, ZipFile pack)
        {
            if (instance != null)
                return null;

            return JsonConvert.DeserializeObject<World>(File.ReadAllText(path));
        }

        public void Export()
        {
            string json = JsonConvert.SerializeObject(this);
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(Path)))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));

            File.WriteAllText(Path,json);
        }

        public void Dispose()
        {
            foreach (var entity in loadedEntities)
            {
                entity.End();
            }
            foreach (var loadedChunk in loadedChunks)
            {
                loadedChunk.Dispose();
            }
            TexturePack?.Dispose();
            Skybox.Dispose();
        }
    }
}
