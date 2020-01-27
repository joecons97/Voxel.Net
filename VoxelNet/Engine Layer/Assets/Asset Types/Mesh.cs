using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using OpenTK;
using VoxelNet.Assets;

namespace VoxelNet.Rendering
{
    public class Mesh : IImportable, IDisposable
    {
        public IndexBuffer IndexBuffer {get; private set; }
        public VertexBuffer VertexBuffer { get; private set; }
        public VertexArray VertexArray { get; private set; }

        //Material...
        public Mesh() { }

        /// <summary>
        /// Use AssetData.GetAsset instead!
        /// </summary>
        /// <param name="file"></param>
        public Mesh(VertexContainer verticesContainer, uint[] indices)
        {
            VertexBuffer = new VertexBuffer(verticesContainer);

            VertexArray = new VertexArray(VertexBuffer);

            IndexBuffer = new IndexBuffer(indices);
        }

        public void Dispose()
        {
            VertexArray?.Dispose();
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }

        public IImportable Import(string path, ZipFile pack)
        {
            if (path.ToLower().EndsWith(".obj"))
            {
                if (pack.ContainsEntry(path))
                {
                    MemoryStream stream = new MemoryStream();
                    pack[path].Extract(stream);
                    var text = Encoding.ASCII.GetString(stream.ToArray());
                    string[] lines = text.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        lines[i] = lines[i].Trim('\r');
                    }
                    return ParseObj(lines);
                }
                return ParseObj(File.ReadAllLines(path));
            }
            else
            {
                return null;
            }
        }

        Mesh ParseObj(string[] lines)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<uint> vIndices = new List<uint>();
            List<uint> nIndices = new List<uint>();
            List<uint> uvIndices = new List<uint>();

            foreach (var line in lines)
            {
                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    bool success = false;
                    switch (parts[0])
                    {
                        case "v":
                            float x, y, z;
                            success = float.TryParse(parts[1], out x); 
                            if (!success)
                            {
                                Debug.Log("Failed to parse X of vertex", DebugLevel.Error);
                                break;
                            }

                            success = float.TryParse(parts[2], out y);
                            if (!success)
                            {
                                Debug.Log("Failed to parse Y of vertex", DebugLevel.Error);
                                break;
                            }

                            success = float.TryParse(parts[3], out z);
                            if (!success)
                            {
                                Debug.Log("Failed to parse Z of vertex", DebugLevel.Error);
                                break;
                            }

                            vertices.Add(new Vector3(x, y, z));
                            break;
                        case "vn":
                            float nx, ny, nz;
                            success = float.TryParse(parts[1], out nx); 
                            if (!success)
                            {
                                Debug.Log("Failed to parse X of vertex", DebugLevel.Error);
                                break;
                            }

                            success = float.TryParse(parts[2], out ny);
                            if (!success)
                            {
                                Debug.Log("Failed to parse Y of vertex", DebugLevel.Error);
                                break;
                            }

                            success = float.TryParse(parts[3], out nz); 
                            if (!success)
                            {
                                Debug.Log("Failed to parse Z of vertex", DebugLevel.Error);
                                break;
                            }

                            normals.Add(new Vector3(nx, ny, nz));
                            break;
                        case "vt":
                            float u, v;
                            success = float.TryParse(parts[1], out u);
                            if (!success)
                            {
                                Debug.Log("Failed to parse U of vertex", DebugLevel.Error);
                                break;
                            }


                            success = float.TryParse(parts[2], out v);
                            if (!success)
                            {
                                Debug.Log("Failed to parse V of vertex", DebugLevel.Error);
                                break;
                            }

                            uvs.Add(new Vector2(u,v));
                            break;
                        case "f":
                            int vcount = parts.Count() - 1;
                            for (int i = 0; i < vcount; i++)
                            {
                                string[] data = parts[i + 1].Split('/');

                                uint index = 0;
                                success = uint.TryParse(data[0], out index); 
                                if (!success)
                                {
                                    Debug.Log("Failed to parse index", DebugLevel.Error);
                                    break;
                                }
                                vIndices.Add(index - 1);
                                if (data.Length > 1)
                                {
                                    success = uint.TryParse(data[1], out index);
                                    if (!success)
                                    {
                                        Debug.Log("Failed to parse index", DebugLevel.Error);
                                        break;
                                    }
                                    uvIndices.Add(index - 1);
                                }
                                if(data.Length > 2)
                                {
                                    success = uint.TryParse(data[2], out index);
                                    if (!success)
                                    {
                                        Debug.Log("Failed to parse index", DebugLevel.Error);
                                        break;
                                    }
                                    nIndices.Add(index - 1);
                                }
                            }
                            break;
                    }
                }
            }

            List<Vector3> Finalvertices = new List<Vector3>();
            List<Vector2> Finaluvs = new List<Vector2>();
            List<Vector3> Finalnormals = new List<Vector3>();
            List<uint> indices = new List<uint>();

            for (int i = 0; i < vIndices.Count; i++)
            {
                Finalvertices.Add(vertices[(int) vIndices[i]]);

                if (uvs.Count > 0)
                    Finaluvs.Add(uvs[(int) uvIndices[i]]);

                if (normals.Count > 0)
                    Finalnormals.Add(normals[(int) nIndices[i]]);

                indices.Add((uint) i);
            }

            VertexContainer container = null;

            if(Finalnormals.Count > 0)
                container = new VertexNormalContainer(Finalvertices.ToArray(), Finaluvs.ToArray(), Finalnormals.ToArray());
            else
                container = new VertexContainer(Finalvertices.ToArray(), Finaluvs.ToArray());

            return new Mesh(container, indices.ToArray());
        }
    }
}
