using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;

namespace VoxelNet.Assets
{
    public interface IImportable
    {
        IImportable Import(string path, ZipFile pack);

        void Dispose();
    }
}
