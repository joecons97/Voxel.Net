using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;

#pragma warning disable 0612

namespace VoxelNet.Assets
{
    public interface IImportable
    {
        IImportable Import(string path, ZipFile pack);

        [Obsolete("This should only be called by the Asset System! Are you sure you want to dispose of this object?")]
        void Dispose();
    }
}
