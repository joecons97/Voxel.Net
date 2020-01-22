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
            SetValue(Value);
        }

        public void SetValue(object value)
        {
            if (value == null)
                return;

            if (UniformLocation == -1)
                UniformLocation = Shader.GetUniformLocation(Name);

            Value = value;

            if(IsNumericType(value))
            {
                double d = Convert.ToDouble(value);
                GL.ProgramUniform1(Shader.Handle, UniformLocation, d);
            }
            else
            {
                if (value is Vector2 vec2)
                {
                    GL.ProgramUniform2(Shader.Handle, UniformLocation, vec2.X, vec2.Y);
                }
                else if(value is Vector3 vec3)
                {
                    GL.ProgramUniform3(Shader.Handle, UniformLocation, vec3.X, vec3.Y, vec3.Z);
                }
                else if (value is Vector4 vec4)
                {
                    GL.ProgramUniform4(Shader.Handle, UniformLocation, vec4.X, vec4.Y, vec4.Z, vec4.W);
                }
                else if (value is Matrix4 mat4)
                {
                    GL.ProgramUniformMatrix4(Shader.Handle, UniformLocation, false, ref mat4);
                }
            }
        }

        public bool IsNumericType(object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
