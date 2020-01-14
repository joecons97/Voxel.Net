using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelNet.Assets
{
    public abstract class Importable
    {
        public Importable() { }

        public abstract Importable Import(string path);
    }
}
