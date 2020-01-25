using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using VoxelNet;
using VoxelNet.Rendering.Material;

namespace VoxelNet.Rendering
{
    public class Shader : IDisposable
    {
        public int Handle { get; private set; }

        Dictionary<string, Uniform> uniforms = new Dictionary<string, Uniform>();

        private bool disposedValue = false;

        public Shader(string fileLocation)
        {
            short shaderType = -1;
            string[] src = new string[2];
            //-1 = none, 0 = vertex, 1 = fragment
            var lines = File.ReadAllLines(fileLocation);
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

        public Shader(string vertexSrc, string fragmentSrc)
        {
            CompileShader(vertexSrc, fragmentSrc);
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

        public void Bind()
        {
            foreach (var uniform in uniforms.Values)
            {
                uniform.Bind();
            }
            GL.UseProgram(Handle);
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
                            string file = File.ReadAllText("Resources/Shaders/" + inc);
                            source = source.Replace(line, file);
                        }
                    }
                }
            }

            int vertShader = 0, fragShader = 0;

            CheckForIncludes();

            vertShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertShader, vertexSrc);

            fragShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragShader, fragmentSrc);

            GL.CompileShader(vertShader);

            string infoLogVert = GL.GetShaderInfoLog(vertShader);
            if (infoLogVert != System.String.Empty)
            {
                Debug.Assert(infoLogVert);
            }
            else
            {
                Debug.Log($"VertexContainer shader {vertShader} compiled successfully!");
            }

            GL.CompileShader(fragShader);

            string infoLogFrag = GL.GetShaderInfoLog(fragShader);

            if (infoLogFrag != System.String.Empty)
            {
                Debug.Assert(infoLogFrag);
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
