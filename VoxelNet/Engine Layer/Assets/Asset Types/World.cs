using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using SimplexNoise;
using VoxelNet.Blocks;
using VoxelNet.Buffers;
using VoxelNet.Buffers.Ubos;
using VoxelNet.Misc;
using VoxelNet.Physics;
using VoxelNet.Rendering;
using VoxelNet.Rendering.Material;

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

        List<Chunk> loadedChunks = new List<Chunk>();

        LinkedList<Chunk> chunksToUpdate = new LinkedList<Chunk>();
        private int worldSize = 5;

        private static World instance;
        
        private Vector2 lastMousePos;
        private Vector2 mouseD;

        private LightingUniformBufferData lightBufferData;
        private float lightAngle;

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

            Begin();
        }

        public Chunk[] GetLoadedChunks()
        {
            return loadedChunks.ToArray();
        }

        public void Begin()
        {
            BlockDatabase.Init();
            TexturePack = AssetDatabase.GetAsset<TexturePack>(TexturePack.DEFAULTPACK);
            Randomizer = new Random(Seed.GetHashCode());
            WorldCamera = new Camera();
            Skybox = new Skybox(AssetDatabase.GetAsset<Material>("Resources/Materials/Sky.mat"));

            lightAngle = 60;
            lightBufferData = new LightingUniformBufferData();

            for (int x = -worldSize - 1; x < worldSize; x++)
            {
                for (int z = -worldSize - 1; z < worldSize; z++)
                {
                    if (TryGetChunkAtPosition(x, z, out Chunk oChunk))
                        continue;

                    Chunk c = new Chunk(new Vector2(x, z));
                    c.GenerateHeightMap();
                    c.FillBlocks();
                    if (TryGetChunkAtPosition(x - 1, z, out oChunk))
                    {
                        c.LeftNeighbour = oChunk;
                        oChunk.RightNeighbour = c;
                    }

                    if (TryGetChunkAtPosition(x + 1, z, out oChunk))
                    {
                        c.RightNeighbour = oChunk;
                        oChunk.LeftNeighbour = c;
                    }
                    if (TryGetChunkAtPosition(x, z - 1, out oChunk))
                    {
                        c.BackNeighbour = oChunk;
                        oChunk.FrontNeighbour = c;
                    }

                    if (TryGetChunkAtPosition(x, z + 1, out oChunk))
                    {
                        c.FrontNeighbour = oChunk;
                        oChunk.BackNeighbour = c;
                    }
                    loadedChunks.Add(c);
                    RequestChunkUpdate(c);
                }
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

            KeyboardState kbdState = Keyboard.GetState();
            MouseState mseState = Mouse.GetState();

            mouseD.X = (mseState.X - lastMousePos.X);
            mouseD.Y = (mseState.Y - lastMousePos.Y);

            lastMousePos.X = mseState.X;
            lastMousePos.Y = mseState.Y;

            if (kbdState.IsKeyDown(Key.S))
                WorldCamera.Position -= WorldCamera.GetForward() / 10;

            if (kbdState.IsKeyDown(Key.W))
                WorldCamera.Position += WorldCamera.GetForward() / 10;

            if (kbdState.IsKeyDown(Key.A))
                WorldCamera.Position -= WorldCamera.GetRight() / 10;

            if (kbdState.IsKeyDown(Key.D))
                WorldCamera.Position += WorldCamera.GetRight() / 10;

            WorldCamera.Rotation = new Vector3(WorldCamera.Rotation.X + mouseD.Y * .05f, 
                WorldCamera.Rotation.Y + mouseD.X * .05f, WorldCamera.Rotation.Z);

            lightAngle += 0.01f;

            lightBufferData.SunColour = Color4.LightYellow.ToVector4();
            lightBufferData.SunDirection = new Vector4(Maths.GetForwardFromRotation(new Vector3(lightAngle, 0, 0)), 1);
            lightBufferData.SunStrength = Vector3.Dot(lightBufferData.SunDirection.Xyz, new Vector3(0, -1, 0)) * 2f;
            float t = Vector3.Dot(lightBufferData.SunDirection.Xyz, new Vector3(0, -1, 0));
            lightBufferData.AmbientColour = Vector4.Lerp(Color4.DarkSlateGray.ToVector4()/1.5f, Color4.DarkSlateGray.ToVector4(), t);
        }

        public void Render()
        {
            UniformBuffers.DirectionLightBuffer.Update(lightBufferData);
            WorldCamera.Update();
            Skybox.Render();
            foreach (var loadedChunk in loadedChunks)
            {
                loadedChunk.Render();
            }
        }

        public IImportable Import(string path)
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
            foreach (var loadedChunk in loadedChunks)
            {
                loadedChunk.Dispose();
            }
            TexturePack?.Dispose();
            Skybox.Dispose();
        }
    }
}
