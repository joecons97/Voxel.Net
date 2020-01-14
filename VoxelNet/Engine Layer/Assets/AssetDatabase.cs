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
        static Dictionary<string, Importable> assets = new Dictionary<string, Importable>();

        public static T GetAsset<T>(string assetPath) where T : Importable
        {
            if (assets.TryGetValue(assetPath, out Importable asset))
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

        public static T CreateAsset<T>(string path) where T : Importable
        {
            var importable = (Importable) Activator.CreateInstance<T>();
            return (T) importable.Import(path);
        }
    }
}
