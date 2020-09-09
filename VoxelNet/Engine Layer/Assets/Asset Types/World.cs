using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics;
using TrueCraft.Core.TerrainGen.Noise;
using VoxelNet.Buffers;
using VoxelNet.Buffers.Ubos;
using VoxelNet.Misc;
using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;
using VoxelNet.Entities;

namespace VoxelNet.Assets
{
    public class World : IImportable, IExportable, IDisposable
    {
        public string Name { get; }
        public string Seed { get; }
        public bool HasFinishedInitialLoading { get; private set; }

        public Camera WorldCamera { get; private set; }
        public Skybox Skybox { get; private set; }
        public TexturePack TexturePack { get; private set; }
        public Random Randomizer { get; private set; }
        public OpenSimplex TerrainNoise { get; private set; }
        public CellNoise BiomeNoise { get; private set; }
        public float WaterHeight { get; }

        private Player player;

        List<Entity> loadedEntities = new List<Entity>();
        List<Chunk> loadedChunks = new List<Chunk>();

        List<Entity> entitiesToDestroy = new List<Entity>();

        LinkedList<Chunk> chunksToUpdate = new LinkedList<Chunk>();
        private int worldSize = 9;
        private int requiredChunksLoadedNum = 0;
        private int currentChunksLoadedNum = 0;
        Texture loadingScreenTexture;
        Texture loadingScreenTextureDickJoke;
        bool isDickJoke = false;
        GUIStyle loadingScreenStyle;

        private static World instance;
        
        private LightingUniformBufferData lightBufferData;
        private float lightAngle;

        private Vector2 lastPlayerPos = Vector2.One;
        List<Vector2> chunksToKeep = new List<Vector2>();
        List<Vector2> newChunks = new List<Vector2>();

        [JsonIgnore]
        public string Path =>
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\Blue Eyes\\Voxel.Net\\Worlds\\{Name}\\{Name}.world";

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
            WaterHeight = 30;

            player = new Player();
            Player.SetControlsActive(false);

            loadedEntities.Add(player);

            Begin();
        }

        public Chunk[] GetLoadedChunks()
        {
            return loadedChunks.ToArray();
        }

        public bool IsChunkQueuedForRegeneration(Chunk chunk)
        {
            return chunksToUpdate.Contains(chunk);
        }

        public void Begin()
        {
            TexturePack = AssetDatabase.GetAsset<TexturePack>("");
            TerrainNoise = new OpenSimplex(Seed.GetSeedNum());
            BiomeNoise = new CellNoise(Seed.GetSeedNum());
            Randomizer = new Random(Seed.GetSeedNum());
            WorldCamera = new Camera();

            Skybox = new Skybox(AssetDatabase.GetAsset<Material>("Resources/Materials/World/Sky.mat"));
            loadingScreenTexture = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/img_loading_screen.png");
            loadingScreenTextureDickJoke = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/img_loading_screen_willy.png");
            isDickJoke = Maths.Chance(0.1f);
            loadingScreenStyle = new GUIStyle()
            {
                Normal = new GUIStyleOption()
                {
                    TextColor = Color4.White
                },
                HorizontalAlignment = HorizontalAlignment.Middle,
                VerticalAlignment = VerticalAlignment.Middle,
                FontSize = 48,
                Font = GUI.LabelStyle.Font
            };

            lightAngle = 5;
            lightBufferData = new LightingUniformBufferData();

            HasFinishedInitialLoading = false;
            requiredChunksLoadedNum = (worldSize + worldSize + 1) * (worldSize + worldSize + 1);

            foreach (var entity in loadedEntities)
            {
                entity.Begin();
            }

            ThreadStart chunkThreadStart = ChunkThread;
            Thread chunkThread = new Thread(chunkThreadStart) {Name = "Chunk Generation Thread"};
            chunkThread.Start();
        }

        void ChunkThread()
        {
            while (Program.IsRunning)
            {
                try
                {
                    if (chunksToUpdate.First != null)
                    {
                        Chunk chunk = chunksToUpdate.First.Value;
                        lock (chunk)
                        {
                            chunk.GenerateMesh();
                            chunksToUpdate.Remove(chunk);
                            if (!HasFinishedInitialLoading)
                                currentChunksLoadedNum++;
                        }
                    }
                }
                catch (SynchronizationLockException ex)
                {
                    Debug.Log(ex.Message + ": " + ex.Source + " - " + ex.StackTrace, DebugLevel.Error);
                }
            }
        }

        public void RequestChunkUpdate(Chunk chunk, bool isPriority, int modXPos, int modZPos, bool threaded = true)
        {
            if (threaded)
            {
                lock (chunksToUpdate)
                {
                    if (!chunksToUpdate.Contains(chunk))
                    {
                        if(chunk.AreAllNeighboursSet)
                        {
                            bool f = modZPos == 15;
                            bool b = modZPos == 0;
                            bool l = modXPos == 15;
                            bool r = modXPos == 0;

                            if (f)
                                chunksToUpdate.AddFirst(chunk.FrontNeighbour);
                            else
                                chunksToUpdate.AddLast(chunk.FrontNeighbour);

                            if (b)
                                chunksToUpdate.AddFirst(chunk.BackNeighbour);
                            else
                                chunksToUpdate.AddLast(chunk.BackNeighbour);

                            if (l)
                            {
                                if (chunk.LeftNeighbour.AreAllNeighboursSet)
                                {
                                    chunksToUpdate.AddFirst(chunk.LeftNeighbour.FrontNeighbour);
                                    chunksToUpdate.AddFirst(chunk.LeftNeighbour.BackNeighbour);
                                }
                                chunksToUpdate.AddFirst(chunk.LeftNeighbour);
                            }
                            else
                            {
                                if (chunk.LeftNeighbour.AreAllNeighboursSet)
                                {
                                    chunksToUpdate.AddLast(chunk.LeftNeighbour.FrontNeighbour);
                                    chunksToUpdate.AddLast(chunk.LeftNeighbour.BackNeighbour);
                                }
                                chunksToUpdate.AddLast(chunk.LeftNeighbour);
                            }

                            if (r)
                            {
                                if (chunk.RightNeighbour.AreAllNeighboursSet)
                                {
                                    chunksToUpdate.AddFirst(chunk.RightNeighbour.FrontNeighbour);
                                    chunksToUpdate.AddFirst(chunk.RightNeighbour.BackNeighbour);
                                }
                                chunksToUpdate.AddFirst(chunk.RightNeighbour);
                            }
                            else
                            {
                                if (chunk.RightNeighbour.AreAllNeighboursSet)
                                {
                                    chunksToUpdate.AddLast(chunk.RightNeighbour.FrontNeighbour);
                                    chunksToUpdate.AddLast(chunk.RightNeighbour.BackNeighbour);
                                }
                                chunksToUpdate.AddLast(chunk.RightNeighbour);
                            }
                        }
                        if(isPriority)
                            chunksToUpdate.AddFirst(chunk);
                        else
                            chunksToUpdate.AddLast(chunk);
                    }
                }
            }
            else
            {
                chunk.GenerateMesh();
            }
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

            for (var index = 0; index < loadedEntities.Count; index++)
            {
                loadedEntities[index].Update();
            }

            float dayInMins = 60f;

            lightAngle += Time.DeltaTime * (6f / dayInMins);

            float colTime = (float)Math.Abs(Math.Cos(MathHelper.DegreesToRadians(lightAngle)));
            colTime = (float)Math.Pow(colTime, 10);

            lightBufferData.SunColour = Vector4.Lerp(Color4.LightYellow.ToVector4(), Color4.OrangeRed.ToVector4(), colTime);
            lightBufferData.SunDirection = new Vector4(Maths.GetForwardFromRotation(new Vector3(lightAngle, 0, 0)), 1);

            float t = Vector3.Dot(lightBufferData.SunDirection.Xyz, new Vector3(0, -1, 0));
            t = (float)Math.Pow(t, .25f) * 1.75f;

            lightBufferData.SunStrength = t;

            Vector4 col = Vector4.Lerp(Color4.DarkSlateGray.ToVector4() / 5f, Color4.DarkSlateGray.ToVector4(), t) / 5;
            lightBufferData.AmbientColour = col;

            UpdateView();
            ClearUpEntities();
        }

        void ClearUpEntities()
        {
            for (var i = 0; i < entitiesToDestroy.ToArray().Length; i++)
            {
                if (loadedEntities.Contains(entitiesToDestroy[i]))
                { 
                    int index = loadedEntities.IndexOf(entitiesToDestroy[i]);
                    loadedEntities[index].Destroyed();
                    loadedEntities[index] = null;
                    loadedEntities.RemoveAt(index);
                }
            }

            entitiesToDestroy.Clear();
        }

        public void RenderGUI()
        {
            if (HasFinishedInitialLoading)
            {
                foreach (var entity in loadedEntities)
                {
                    entity.RenderGUI();
                }
            }
            else
            {
                int perc = (int)(((float)currentChunksLoadedNum / (float)requiredChunksLoadedNum / 2f) * 100f);
                if (isDickJoke)
                {
                    GUI.Image(loadingScreenTextureDickJoke, new Rect(0, 0, Program.Settings.WindowWidth, Program.Settings.WindowHeight));
                    GUI.Label($"LMAO IT'S A PENIS", new Rect(0, -48, Program.Settings.WindowWidth, Program.Settings.WindowHeight), loadingScreenStyle);
                }
                else
                {
                    GUI.Image(loadingScreenTexture, new Rect(0, 0, Program.Settings.WindowWidth, Program.Settings.WindowHeight));
                }
                GUI.Label($"LOADING...", new Rect(0, 0, Program.Settings.WindowWidth, Program.Settings.WindowHeight), loadingScreenStyle);
                GUI.Label($"{perc}%", new Rect(0, 48, Program.Settings.WindowWidth, Program.Settings.WindowHeight), loadingScreenStyle);
            }
        }

        void UpdateView()
        {
            int roundedX = (int) Math.Floor(WorldCamera.Position.X / 16);//Math.Ceiling(WorldCamera.Position.X / 16) * 16;
            int roundedZ = (int) Math.Floor(WorldCamera.Position.Z / 16);//Math.Ceiling(WorldCamera.Position.Z / 16) * 16;

            if (roundedX != (int)lastPlayerPos.X || roundedZ != (int)lastPlayerPos.Y)
            {
                for (int x = -worldSize - 1; x < worldSize; x++)
                {
                    for (int z = -worldSize - 1; z < worldSize; z++)
                    {
                        int wantedX = x + (roundedX);
                        int wantedZ = z + (roundedZ);

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

                            if (oChunk.AreAllNeighboursSet)
                            {
                                //7 as x & z as to not force update surrounding chunks too
                                RequestChunkUpdate(oChunk, false, 7, 7, true);
                            }
                        }
                        if (TryGetChunkAtPosition(wantedX + 1, wantedZ, out oChunk))
                        {
                            c.RightNeighbour = oChunk;
                            oChunk.LeftNeighbour = c;

                            if (oChunk.AreAllNeighboursSet)
                            {
                                //7 as x & z as to not force update surrounding chunks too
                                RequestChunkUpdate(oChunk, false, 7, 7, true);
                            }
                        }
                        if (TryGetChunkAtPosition(wantedX, wantedZ - 1, out oChunk))
                        {
                            c.BackNeighbour = oChunk;
                            oChunk.FrontNeighbour = c;

                            if (oChunk.AreAllNeighboursSet)
                            {
                                //7 as x & z as to not force update surrounding chunks too
                                RequestChunkUpdate(oChunk, false, 7, 7, true);
                            }
                        }
                        if (TryGetChunkAtPosition(wantedX, wantedZ + 1, out oChunk))
                        {
                            c.FrontNeighbour = oChunk;
                            oChunk.BackNeighbour = c;

                            if (oChunk.AreAllNeighboursSet)
                            {
                                //7 as x & z as to not force update surrounding chunks too
                                RequestChunkUpdate(oChunk, false, 7, 7, true);
                            }
                        }
                        loadedChunks.Add(c);
                    }
                }

                for (int i = 0; i < loadedChunks.Count; i++)
                {
                    if (chunksToKeep.Any(v => (int)v.X == (int)loadedChunks[i].Position.X && (int)v.Y == (int)loadedChunks[i].Position.Y) ||
                        newChunks.Any(v => (int)v.X == (int)loadedChunks[i].Position.X && (int)v.Y == (int)loadedChunks[i].Position.Y))
                        continue;

                    if (chunksToUpdate.Contains(loadedChunks[i]))
                        chunksToUpdate.Remove(loadedChunks[i]);

                    var chunk = loadedChunks[i];
                    loadedChunks.Remove(chunk);
                    chunk.Dispose();
                }

                chunksToKeep.Clear();
                newChunks.Clear();
                lastPlayerPos = new Vector2(roundedX, roundedZ);
            }

            if (!HasFinishedInitialLoading)
            {
                if (currentChunksLoadedNum >= requiredChunksLoadedNum * 2)
                {
                    HasFinishedInitialLoading = true;
                    Player.SetControlsActive(true);
                }
            }
        }

        public Player GetPlayer()
        {
            return player;
        }

        public void Render()
        {
            UniformBuffers.DirectionLightBuffer.Update(lightBufferData);
            WorldCamera.Update();
            for (var index = 0; index < loadedEntities.Count; index++)
            {
                loadedEntities[index].Render();
            }

            for (var index = 0; index < loadedChunks.Count; index++)
            {
                loadedChunks[index].Render();
            }

            Skybox.Render();
        }

        public IImportable Import(string path, ZipFile pack)
        {
            if (instance != null)
                return null;

            return JsonConvert.DeserializeObject<World>(File.ReadAllText(path));
        }

        public void AddEntity(Entity entity)
        {
            loadedEntities.Add(entity);
            entity.Begin();
        }

        public void DestroyEntity(Entity entity)
        {
            entitiesToDestroy.Add(entity);
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
            Chunk.ChunkMaterial?.Dispose();
            Chunk.ChunkWaterMaterial?.Dispose();
            TexturePack?.Dispose();
            Skybox.Dispose();
        }
    }
}
