using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VoxelNet.Assets;

namespace VoxelNet.Rendering.Material
{
    public class Material : IImportable, IDisposable
    {
        public Shader Shader { get; private set; }

        List<Texture> textures = new List<Texture>();

        public Material() { }

        /// <summary>
        /// Use AssetData.GetAsset instead!
        /// </summary>
        /// <param name="file"></param>
        public Material(string file)
        {
            var lines = File.ReadAllLines(file);

            string shader = "";
            List<string[]> uniforms = new List<string[]>();

            //Store data to be process in correct order (textures don't matter)
            foreach (var line in lines)
            {
                if (line.StartsWith("//"))
                    continue;

                if (line.StartsWith("texture"))
                {
                    textures.Add(AssetDatabase.GetAsset<Texture>(line.Split(' ')[1]));
                }
                else if (line.StartsWith("shader"))
                {
                    shader = line.Split(' ')[1];
                }
                else if (line.StartsWith("uniform"))
                {
                    var split = line.Split(' ');
                    uniforms.Add(split);
                }
            }

            //Instantiate shader
            Shader = new Shader(shader);
            foreach (var uniform in uniforms)
            {
                ProcessUniform(uniform);
            }

            void ProcessUniform(string[] uniform)
            {
                string type = uniform[1];
                string name = uniform[2];
                string value = uniform[3];

                try
                {
                    switch (type)
                    {
                        case "number":
                            var f = float.Parse(value);
                            Shader.SetUniform(name, f);
                            break;
                        case "mat4":
                            Matrix4 mat4 = new Matrix4();
                            var mat4Val = value.Split(',');
                            if (mat4Val.Length == 16)
                            {
                                mat4.M11 = float.Parse(mat4Val[0]);
                                mat4.M12 = float.Parse(mat4Val[1]);
                                mat4.M13 = float.Parse(mat4Val[2]);
                                mat4.M14 = float.Parse(mat4Val[3]);
                                mat4.M21 = float.Parse(mat4Val[4]);
                                mat4.M22 = float.Parse(mat4Val[5]);
                                mat4.M23 = float.Parse(mat4Val[6]);
                                mat4.M24 = float.Parse(mat4Val[7]);
                                mat4.M31 = float.Parse(mat4Val[8]);
                                mat4.M32 = float.Parse(mat4Val[9]);
                                mat4.M33 = float.Parse(mat4Val[10]);
                                mat4.M34 = float.Parse(mat4Val[11]);
                                mat4.M41 = float.Parse(mat4Val[12]);
                                mat4.M42 = float.Parse(mat4Val[13]);
                                mat4.M43 = float.Parse(mat4Val[14]);
                                mat4.M44 = float.Parse(mat4Val[15]);

                                Shader.SetUniform(name, mat4);
                            }
                            break;
                        case "vec2":
                            var vec2Val = value.Split(',');
                            if (vec2Val.Length == 2)
                            {
                                Shader.SetUniform(name, new Vector2(float.Parse(vec2Val[0]), float.Parse(vec2Val[1])));
                            }
                            break;
                        case "vec3":
                            var vec3Val = value.Split(',');
                            if (vec3Val.Length == 3)
                            {
                                Shader.SetUniform(name, new Vector3(float.Parse(vec3Val[0]), float.Parse(vec3Val[1]), float.Parse(vec3Val[2])));
                            }
                            break;
                        case "vec4":
                            var vec4Val = value.Split(',');
                            if (vec4Val.Length == 4)
                            {
                                Shader.SetUniform(name, new Vector4(float.Parse(vec4Val[0]), float.Parse(vec4Val[1]), float.Parse(vec4Val[2]), float.Parse(vec4Val[3])));
                            }
                            break;
                    }
                }
                catch
                {
                    Debug.Log($"There was any error parsing material {file}", DebugLevel.Error);
                }
            }
        }
        public Material(string[] lines)
        {
            string shader = "";
            List<string[]> uniforms = new List<string[]>();

            //Store data to be process in correct order (textures don't matter)
            foreach (var line in lines)
            {
                if (line.StartsWith("//"))
                    continue;

                if (line.StartsWith("texture"))
                {
                    textures.Add(AssetDatabase.GetAsset<Texture>(line.Split(' ')[1]));
                }
                else if (line.StartsWith("shader"))
                {
                    shader = line.Split(' ')[1];
                }
                else if (line.StartsWith("uniform"))
                {
                    var split = line.Split(' ');
                    uniforms.Add(split);
                }
            }

            //Instantiate shader
            Shader = new Shader(shader);
            foreach (var uniform in uniforms)
            {
                ProcessUniform(uniform);
            }

            void ProcessUniform(string[] uniform)
            {
                string type = uniform[1];
                string name = uniform[2];
                string value = uniform[3];

                try
                {
                    switch (type)
                    {
                        case "number":
                            Shader.SetUniform(name, double.Parse(value));
                            break;
                        case "mat4":
                            Matrix4 mat4 = new Matrix4();
                            var mat4Val = value.Split(',');
                            if (mat4Val.Length == 16)
                            {
                                mat4.M11 = float.Parse(mat4Val[0]);
                                mat4.M12 = float.Parse(mat4Val[1]);
                                mat4.M13 = float.Parse(mat4Val[2]);
                                mat4.M14 = float.Parse(mat4Val[3]);
                                mat4.M21 = float.Parse(mat4Val[4]);
                                mat4.M22 = float.Parse(mat4Val[5]);
                                mat4.M23 = float.Parse(mat4Val[6]);
                                mat4.M24 = float.Parse(mat4Val[7]);
                                mat4.M31 = float.Parse(mat4Val[8]);
                                mat4.M32 = float.Parse(mat4Val[9]);
                                mat4.M33 = float.Parse(mat4Val[10]);
                                mat4.M34 = float.Parse(mat4Val[11]);
                                mat4.M41 = float.Parse(mat4Val[12]);
                                mat4.M42 = float.Parse(mat4Val[13]);
                                mat4.M43 = float.Parse(mat4Val[14]);
                                mat4.M44 = float.Parse(mat4Val[15]);

                                Shader.SetUniform(name, mat4);
                            }
                            break;
                        case "vec2":
                            var vec2Val = value.Split(',');
                            if (vec2Val.Length == 2)
                            {
                                Shader.SetUniform(name, new Vector2(float.Parse(vec2Val[0]), float.Parse(vec2Val[1])));
                            }
                            break;
                        case "vec3":
                            var vec3Val = value.Split(',');
                            if (vec3Val.Length == 3)
                            {
                                Shader.SetUniform(name, new Vector3(float.Parse(vec3Val[0]), float.Parse(vec3Val[1]), float.Parse(vec3Val[2])));
                            }
                            break;
                        case "vec4":
                            var vec4Val = value.Split(',');
                            if (vec4Val.Length == 4)
                            {
                                Shader.SetUniform(name, new Vector4(float.Parse(vec4Val[0]), float.Parse(vec4Val[1]), float.Parse(vec4Val[2]), float.Parse(vec4Val[3])));
                            }
                            break;
                    }
                }
                catch
                {
                    Debug.Log($"There was any error parsing material loading from raw lines", DebugLevel.Error);
                }
            }
        }
        public void Dispose()
        {
            for (int i = 0; i < textures.Count; i++)
            {
                textures[i].Dispose();
            }
            Shader?.Dispose();
        }

        public void SetTexture(int index, Texture tex)
        {
            if (index >= textures.Count) return;

            textures[index] = tex;
        }

        public void SetTexture(int index, int handle)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + index);
            GL.BindTexture(TextureTarget.Texture2D, handle);
        }

        public void SetScreenSourceTexture(string srcImage, int handle, int slot = 0)
        {
            var uniform = Shader.GetUniform(srcImage);

           // int slot = 1;//uniform.UniformLocation;
            var slotenum = TextureUnit.Texture0 + slot;
            GL.ActiveTexture(slotenum);
            GL.BindTexture(TextureTarget.Texture2D, handle);
            Shader.SetUniform(srcImage, slot);
        }

        public void Bind()
        {
            Shader.Bind();
            for (int i = 0; i < textures.Count; i++)
            {
                textures[i].Bind(i);
            }
        }

        public void Unbind()
        {
            Shader.Unbind();
        }

        public IImportable Import(string path, ZipFile pack)
        {
            if (pack.ContainsEntry(path))
            {
                var entry = pack[path];
                MemoryStream outputStream = new MemoryStream();
                entry.Extract(outputStream);
                string text = Encoding.ASCII.GetString(outputStream.ToArray());
                string[] lines = text.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].Trim('\r');
                }
                Debug.Log("Loaded material from pack");
                return new Material(lines);
            }

            Debug.Log("Loaded material from file");
            return new Material(path);
        }
    }
}
