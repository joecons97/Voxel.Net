using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VoxelNet.Rendering.Material
{
    public class Uniform
    {
        public string Name { get; private set; }
        public int UniformLocation { get; private set; }
        public object Value { get; private set; }
        public Shader Shader { get; private set; }

        public Uniform(string name, int location, Shader shader, object value)
        {
            Name = name;
            UniformLocation = location;
            Shader = shader;
            SetValue(value);
        }

        public void Bind()
        {
            if (Value == null) return;

            switch (Value)
            {
                case double d:
                    GL.ProgramUniform1(Shader.Handle, UniformLocation, (float)d);
                    break;
                case float f:
                    GL.ProgramUniform1(Shader.Handle, UniformLocation, f);
                    break;
                case int i:
                    GL.ProgramUniform1(Shader.Handle, UniformLocation, (int)i);
                    break;
                case Vector2 vec2:
                    GL.ProgramUniform2(Shader.Handle, UniformLocation, vec2.X, vec2.Y);
                    break;
                case Vector3 vec3:
                    GL.ProgramUniform3(Shader.Handle, UniformLocation, vec3.X, vec3.Y, vec3.Z);
                    break;
                case Vector4 vec4:
                    GL.ProgramUniform4(Shader.Handle, UniformLocation, vec4.X, vec4.Y, vec4.Z, vec4.W);
                    break;
                case Matrix4 mat4:
                    GL.ProgramUniformMatrix4(Shader.Handle, UniformLocation, false, ref mat4);
                    break;
            }
        }

        public void SetValue(object value)
        {
            if (value == null)
                return;

            if (UniformLocation == -1)
                UniformLocation = Shader.GetUniformLocation(Name);

            Value = value;
        }
    }
}
