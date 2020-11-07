using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using VoxelNet;
using VoxelNet.Rendering.Material;

namespace VoxelNet.Rendering
{
    public enum CullType
    {
        None,
        Front,
        Back
    }

    public enum BlendType
    {
        None,
        OneMinus
    }

    public class Shader : IDisposable
    {
        public int Handle { get; private set; }
        public bool IsTransparent { get; private set; }
        public BlendType Blending { get; private set; }

        public string FileLocation{ get; }

        public CullType CullingType { get; private set; } = CullType.Back;

        Dictionary<string, Uniform> uniforms = new Dictionary<string, Uniform>();

        private bool disposedValue = false;

        public Shader(string fileLocation)
        {
            FileLocation = fileLocation;
            short shaderType = -1;
            string[] src = new string[2];
            //-1 = none, 0 = vertex, 1 = fragment
            List<string> lines = null;

            if (AssetDatabase.GetPackageFile().ContainsEntry(fileLocation))
            {
                var entry = AssetDatabase.GetPackageFile()[fileLocation];
                MemoryStream outputStream = new MemoryStream();
                entry.Extract(outputStream);
                string text = Encoding.ASCII.GetString(outputStream.ToArray());
                lines = text.Split('\n').ToList();
                for (int i = 0; i < lines.Count; i++)
                {
                    lines[i] = lines[i].Trim('\r');
                }
            }
            else
                lines = File.ReadAllLines(fileLocation).ToList();

            foreach (var line in lines)
            {
                if(line.Contains("#shader"))
                {
                    if (line.Contains("vertex"))
                        shaderType = 0;
                    else if (line.Contains("fragment"))
                        shaderType = 1;
                }
                else
                {
                    src[shaderType] += line + "\n";
                }
            }

            CompileShader(src[0], src[1]);
        }

        public int GetUniformLocation(string uniform)
        {
            int loc = GL.GetUniformLocation(Handle, uniform);
            if(loc == -1)
                Debug.Log($"Uniform {uniform} doesn't exist in shader {Handle}!", DebugLevel.Warning);

            return loc;
        }

        public void SetUniform(string name, object value)
        {
            if(uniforms.TryGetValue(name, out Uniform uniform))
            { 
                uniform.SetValue(value);
            }
        }

        public void BindUniform(string name)
        {
            if (uniforms.TryGetValue(name, out Uniform uniform))
            {
                uniform.Bind();
            }
        }

        public Uniform GetUniform(string name)
        {
            if (ContainsUniform(name))
            {
                return uniforms[name];
            }

            return null;
        }

        public bool ContainsUniform(string name)
        {
            return uniforms.ContainsKey(name);
        }

        public void Bind()
        {
            if (IsTransparent)
            {
                if (Blending != BlendType.None)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendEquation(BlendEquationMode.FuncAdd);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                }
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }

            switch (CullingType)
            {
                case CullType.None:
                    GL.Disable(EnableCap.CullFace);
                    break;
                case CullType.Front:
                    GL.Enable(EnableCap.CullFace);
                    GL.CullFace(CullFaceMode.Front);
                    break;
                case CullType.Back:
                    GL.Enable(EnableCap.CullFace);
                    GL.CullFace(CullFaceMode.Back);
                    break;
            }

            GL.UseProgram(Handle);

            foreach (var uniform in uniforms.Values)
            {
                uniform.Bind();
            }
        }

        public void Unbind()
        {
            GL.UseProgram(0);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        void CompileShader(string vertexSrc, string fragmentSrc)
        {
            void GetUniform(string line)
            {
                var elements = line.Split(' ');
                string type = elements[1];
                string name = elements[2].Trim(';');

                Debug.Log($"Found uniform: {name} of type {type}");

                //TODO Add to dictionary/list/whatever
                uniforms.Add(name, new Uniform(name, GetUniformLocation(name), this, null));
            }

            void CheckForUniforms(string src)
            {
                var lines = src.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("uniform") && !line.StartsWith("//") && !line.Contains("layout(std140)"))
                        GetUniform(line);
                }
            }

            void CheckForIncludes()
            {
                Check(ref vertexSrc);
                Check(ref fragmentSrc);

                void Check(ref string source)
                {
                    var lines = source.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("#include "))
                        {
                            string inc = line.Split(' ')[1].Trim('"');
                            string location = "Resources/Shaders/" + inc;

                            var entry = AssetDatabase.GetPackageFile()[location];
                            MemoryStream outputStream = new MemoryStream();
                            entry.Extract(outputStream);
                            string text = Encoding.ASCII.GetString(outputStream.ToArray());

                            text = text.Replace("?", "").Replace("\r", "").Replace("\t", "");

                            string file = text;
                            source = source.Replace(line, file);
                        }
                    }
                }
            }

            void CheckForQueue()
            {
                var lines = fragmentSrc.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("#queue"))
                    {
                        var types = line.Split(' ');
                        var type = types[1];
                        if (type == "transparent")
                            IsTransparent = true;
                        else
                            IsTransparent = false;

                        fragmentSrc = fragmentSrc.Replace(line, "");
                    }
                }
                
            }

            void CheckForBlending()
            {
                var lines = fragmentSrc.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("#blend"))
                    {
                        var types = line.Split(' ');
                        var type = types[1];
                        if (type == "none")
                            Blending = BlendType.None;
                        else if (type == "oneminus")
                            Blending = BlendType.OneMinus;

                        fragmentSrc = fragmentSrc.Replace(line, "");
                    }
                }

            }

            void CheckForCulling()
            {
                var lines = fragmentSrc.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("#culling"))
                    {
                        var types = line.Split(' ');
                        var type = types[1];
                        if (type == "front")
                            CullingType = CullType.Front;
                        else if(type == "none")
                            CullingType = CullType.None;
                        else if (type == "back")
                            CullingType = CullType.Back;

                        fragmentSrc = fragmentSrc.Replace(line, "");
                    }
                }

            }

            int vertShader = 0, fragShader = 0;

            CheckForIncludes();

            CheckForQueue();
            CheckForBlending();
            CheckForCulling();

            vertShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertShader, vertexSrc);

            fragShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragShader, fragmentSrc);

            GL.CompileShader(vertShader);

            string infoLogVert = GL.GetShaderInfoLog(vertShader);
            if (infoLogVert != System.String.Empty)
            {
                //Debug.Assert(infoLogVert);
                throw new Exception("Vertex Shader " + vertShader + " failed to compile!\n" + infoLogVert);
            }
            else
            {
                Debug.Log($"VertexContainer shader {vertShader} compiled successfully!");
            }

            GL.CompileShader(fragShader);

            string infoLogFrag = GL.GetShaderInfoLog(fragShader);

            if (infoLogFrag != System.String.Empty)
            {
                throw new Exception("FragShader " + fragShader + " failed to compile!\n" + infoLogFrag);

            }
            else
            {
                Debug.Log($"Fragment shader {fragShader} compiled successfully!");
            }

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertShader);
            GL.AttachShader(Handle, fragShader);

            GL.LinkProgram(Handle);

            GL.DetachShader(Handle, vertShader);
            GL.DetachShader(Handle, fragShader);

            GL.DeleteShader(vertShader);
            GL.DeleteShader(fragShader);

            CheckForUniforms(vertexSrc);
            CheckForUniforms(fragmentSrc);
        }
    }
}
