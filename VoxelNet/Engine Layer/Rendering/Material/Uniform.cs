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

            var numType = GetNumericType(Value);
            if (Value is Double d)
            {
                float f = (float)d;
                GL.ProgramUniform1(Shader.Handle, UniformLocation, f);
            }
            else if (Value is float f)
            {
                GL.ProgramUniform1(Shader.Handle, UniformLocation, f);
            }
            else if(numType == TypeCode.Int32)
            {
                int i = (int) Value;
                GL.ProgramUniform1(Shader.Handle, UniformLocation, i);
            }
            else
            {
                if (Value is Vector2 vec2)
                {
                    GL.ProgramUniform2(Shader.Handle, UniformLocation, vec2.X, vec2.Y);
                }
                else if (Value is Vector3 vec3)
                {
                    GL.ProgramUniform3(Shader.Handle, UniformLocation, vec3.X, vec3.Y, vec3.Z);
                }
                else if (Value is Vector4 vec4)
                {
                    GL.ProgramUniform4(Shader.Handle, UniformLocation, vec4.X, vec4.Y, vec4.Z, vec4.W);
                }
                else if (Value is Matrix4 mat4)
                {
                    GL.ProgramUniformMatrix4(Shader.Handle, UniformLocation, false, ref mat4);
                }
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

        public bool IsNumber(object o)
        {
            var type = o.GetType();
            return false;
        }
        

        public TypeCode GetNumericType(object o)
        {
            return Type.GetTypeCode(o.GetType());
        }
    }
}
