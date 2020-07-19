using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Rendering;

namespace VoxelNet
{
    public abstract class Item
    {
        public virtual string Name { get; protected set; }
        public virtual string IconLocation { get; protected set; } = "Resources/Textures/Items/Def_Item.png";
        public int ID { get; set; }
        public abstract string Key { get; }
        public Texture Icon { get; private set; }
        public Mesh Mesh { get; private set; }

        public int MaxStackSize { get; protected set; } = 64;

        public Item()
        {
            
        }

        protected void GenerateGraphics()
        {
            Icon = AssetDatabase.GetAsset<Texture>(IconLocation);
            if (Icon != null)
            {
                string path = $"Item/Model/{Name}";
                if (AssetDatabase.ContainsAssetOfType(path, typeof(Mesh)))
                    Mesh = AssetDatabase.GetAsset<Mesh>(path);
                else
                {
                    Mesh = CreateModel();

                    if (!AssetDatabase.RegisterAsset(Mesh, path))
                        Mesh = null;
                }
            }
        }

        Mesh CreateModel()
        {
            List<Vector3> verts = new List<Vector3>();
            List<uint> indices = new List<uint>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();

            int w = Icon.Width;
            int h = Icon.Height;

            float wDiff = 1f / w;
            float hDiff = 1f / h;

            float depth = (wDiff + hDiff) / 2;

            for (int x = 0; x < w; x++)
            {
                for (int y = h - 1; y >= 0; y--)
                {
                    var col = Icon.GetPixel(x, y);
                    if (col.A == 0) continue;

                    var uvTl = new Vector2((float)x / w, (float)(y - 1) / h);
                    var uvTr = new Vector2((float)(x + 1) / w, (float)(y - 1) / h);
                    var uvBl = new Vector2((float)x / w, (float)y / h);
                    var uvBr = new Vector2((float)(x + 1) / w, (float)y / h);

                    Vector3 tl = new Vector3(x * wDiff, y * hDiff, 0);
                    if (!verts.Contains(tl))
                    {
                        verts.Add(tl);
                        normals.Add(new Vector3(0,0,1));
                        uvs.Add(uvTl);
                    }

                    uint tli = (uint)verts.IndexOf(tl);

                    Vector3 tr = new Vector3((x * wDiff) + wDiff, y * hDiff, 0);
                    if (!verts.Contains(tr))
                    {
                        verts.Add(tr);
                        normals.Add(new Vector3(0, 0, 1));
                        uvs.Add(uvTr);
                    }

                    uint tri = (uint)verts.IndexOf(tr);

                    Vector3 bl = new Vector3(x * wDiff, (y * hDiff) + wDiff, 0);
                    if (!verts.Contains(bl))
                    {
                        verts.Add(bl);
                        normals.Add(new Vector3(0, 0, 1));
                        uvs.Add(uvBl);
                    }

                    uint bli = (uint)verts.IndexOf(bl);

                    Vector3 br = new Vector3((x * wDiff) + wDiff, (y * hDiff) + wDiff, 0);
                    if (!verts.Contains(br))
                    {
                        verts.Add(br);
                        normals.Add(new Vector3(0, 0, 1));
                        uvs.Add(uvBr);
                    }

                    uint bri = (uint)verts.IndexOf(br);

                    indices.Add(tli);
                    indices.Add(tri);
                    indices.Add(bli);

                    indices.Add(tri);
                    indices.Add(bri);
                    indices.Add(bli);

                    if ((x > 0 && Icon.GetPixel(x - 1, y).A == 0) || x == 0)
                    {
                        Vector3 tlD = new Vector3(x * wDiff, y * hDiff, -depth);
                        Vector3 blD = new Vector3(x * wDiff, (y * hDiff) + hDiff, -depth);

                        if (!verts.Contains(tlD))
                        {
                            verts.Add(tlD);
                            normals.Add(new Vector3(-1, 0, 0));
                            uvs.Add(uvTl);
                        }

                        uint tlDi = (uint)verts.IndexOf(tlD);

                        if (!verts.Contains(blD))
                        {
                            verts.Add(blD);
                            normals.Add(new Vector3(-1, 0, 0));
                            uvs.Add(uvBl);
                        }

                        uint blDi = (uint)verts.IndexOf(blD);

                        indices.Add(tlDi);
                        indices.Add(tli);
                        indices.Add(blDi);

                        indices.Add(bli);
                        indices.Add(blDi);
                        indices.Add(tli);
                    }
                    if ((x < w - 1 && Icon.GetPixel(x + 1, y).A == 0) || x == w - 1)
                    {
                        Vector3 trD = new Vector3((x * wDiff) + wDiff, y * hDiff, -depth);
                        Vector3 brD = new Vector3((x * wDiff) + wDiff, (y * hDiff) + hDiff, -depth);

                        if (!verts.Contains(trD))
                        {
                            verts.Add(trD);
                            normals.Add(new Vector3(1, 0, 0));
                            uvs.Add(uvTr);
                        }

                        uint trDi = (uint)verts.IndexOf(trD);

                        if (!verts.Contains(brD))
                        {
                            verts.Add(brD);
                            normals.Add(new Vector3(1, 0, 0));
                            uvs.Add(uvBr);
                        }

                        uint brDi = (uint)verts.IndexOf(brD);

                        indices.Add(tri);
                        indices.Add(trDi);
                        indices.Add(bri);

                        indices.Add(brDi);
                        indices.Add(bri);
                        indices.Add(trDi);

                    }
                    if ((y > 0 && Icon.GetPixel(x, y - 1).A == 0) || y == 0)
                    {
                        Vector3 tlD = new Vector3((x * wDiff), y * hDiff, -depth);
                        Vector3 trD = new Vector3((x * wDiff) + wDiff, y * hDiff, -depth);

                        if (!verts.Contains(trD))
                        {
                            verts.Add(trD);
                            normals.Add(new Vector3(0, -1, 0));
                            uvs.Add(uvTr);
                        }

                        uint trDi = (uint)verts.IndexOf(trD);

                        if (!verts.Contains(tlD))
                        {
                            verts.Add(tlD);
                            normals.Add(new Vector3(0, -1, 0));
                            uvs.Add(uvTl);
                        }

                        uint tlDi = (uint)verts.IndexOf(tlD);

                        indices.Add(tli);
                        indices.Add(tlDi);
                        indices.Add(trDi);

                        indices.Add(tri);
                        indices.Add(tli);
                        indices.Add(trDi);

                    }
                    if ((y < h - 1 && Icon.GetPixel(x, y + 1).A == 0) || y == h - 1)
                    {
                        Vector3 blD = new Vector3((x * wDiff), (y * hDiff) + hDiff, -depth);
                        Vector3 brD = new Vector3((x * wDiff) + wDiff, (y * hDiff) + hDiff, -depth);

                        if (!verts.Contains(blD))
                        {
                            verts.Add(blD);
                            normals.Add(new Vector3(0, 1, 0));
                            uvs.Add(uvBl);
                        }

                        uint blDi = (uint)verts.IndexOf(blD);

                        if (!verts.Contains(brD))
                        {
                            verts.Add(brD);
                            normals.Add(new Vector3(0, 1, 0));
                            uvs.Add(uvBr);
                        }

                        uint brDi = (uint)verts.IndexOf(brD);

                        indices.Add(bri);
                        indices.Add(brDi);
                        indices.Add(blDi);

                        indices.Add(blDi);
                        indices.Add(bli);
                        indices.Add(bri);
                    }

                    Vector3 tlB = new Vector3(x * wDiff, y * hDiff, -depth);
                    if (!verts.Contains(tlB))
                    {
                        verts.Add(tlB);
                        normals.Add(new Vector3(0, 0, -1));
                        uvs.Add(uvTl);
                    }

                    uint tlBi = (uint)verts.IndexOf(tlB);

                    Vector3 trB = new Vector3((x * wDiff) + wDiff, y * hDiff, -depth);
                    if (!verts.Contains(trB))
                    {
                        verts.Add(trB);
                        normals.Add(new Vector3(0, 0, -1));
                        uvs.Add(uvTr);
                    }

                    uint trBi = (uint)verts.IndexOf(trB);

                    Vector3 blB = new Vector3(x * wDiff, (y * hDiff) + wDiff, -depth);
                    if (!verts.Contains(blB))
                    {
                        verts.Add(blB);
                        normals.Add(new Vector3(0, 0, -1));
                        uvs.Add(uvBl);
                    }

                    uint blBi = (uint)verts.IndexOf(blB);

                    Vector3 brB = new Vector3((x * wDiff) + wDiff, (y * hDiff) + wDiff, -depth);
                    if (!verts.Contains(brB))
                    {
                        verts.Add(brB);
                        normals.Add(new Vector3(0, 0, -1));
                        uvs.Add(uvBr);
                    }

                    uint brBi = (uint)verts.IndexOf(brB);

                    indices.Add(blBi);
                    indices.Add(trBi);
                    indices.Add(tlBi);

                    indices.Add(blBi);
                    indices.Add(brBi);
                    indices.Add(trBi);
                }
            }

            for (var index = 0; index < verts.Count; index++)
            {
                verts[index] += new Vector3(-.5f,-1, 0);
            }

            var container = new VertexNormalContainer(verts.ToArray(), uvs.ToArray(), normals.ToArray());

            return new Mesh(container, indices.ToArray());
        }
    }
}
