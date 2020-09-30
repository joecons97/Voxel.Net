using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaterialEditor
{
    public class vec2
    {
        public float x;
        public float y;

        public override string ToString()
        {
            return $"{x},{y}";
        }
    }
    public class vec3
    {
        public float x;
        public float y;
        public float z;

        public override string ToString()
        {
            return $"{x},{y},{z}";
        }
    }
    public class vec4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public override string ToString()
        {
            return $"{x},{y},{z},{w}";
        }
    }
    public class sampler2D
    {
        public string textureLocation;
    }
}
