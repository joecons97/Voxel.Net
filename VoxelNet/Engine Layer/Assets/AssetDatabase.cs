using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VoxelNet.Assets;
using VoxelNet.Rendering;

namespace VoxelNet
{
    public static class AssetDatabase
    {
        static Dictionary<string, IImportable> assets = new Dictionary<string, IImportable>();

        public static T GetAsset<T>(string assetPath) where T : IImportable
        {
            if (assets.TryGetValue(assetPath, out IImportable asset))
            {
                T cast = (T) asset;
                if (cast != null)
                {
                    return cast;
                }
            }

            var importable = CreateAsset<T>(assetPath);
            assets.Add(assetPath, importable);

            return importable;
        }

        public static T CreateAsset<T>(string path) where T : IImportable
        {
            var importable = (IImportable) Activator.CreateInstance<T>();
            return (T) importable.Import(path);
        }

        public static void Dispose()
        {
            foreach (var value in assets.Values)
            {
                value.Dispose();
            }
        }
    }
}
