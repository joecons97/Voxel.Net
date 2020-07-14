using System;
using System.Collections.Generic;
using Ionic.Zip;
using VoxelNet.Assets;
using VoxelNet.Rendering;

namespace VoxelNet
{
    public static class AssetDatabase
    {
        public static string DEFAULTPACK = "Default";
        private static ZipFile packFile;
        static Dictionary<string, IImportable> assets = new Dictionary<string, IImportable>();

        public static ZipFile GetPackageFile()
        {
            return packFile;
        }

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

        public static bool ContainsAssetOfType<T>(string path, T type) where T : Type
        {
            if (!assets.ContainsKey(path))
                return false;

            if (assets[path].GetType() == type)
                return true;

            return false;
        }

        public static bool RegisterAsset<T>(T asset, string path) where T : IImportable
        {
            if (assets.ContainsKey(path))
                return false;

            assets.Add(path, asset);

            return true;
        }

        public static T CreateAsset<T>(string path) where T : IImportable
        {
            var importable = (IImportable) Activator.CreateInstance<T>();
            return (T) importable.Import(path, packFile);
        }

        public static void SetPack(string pack)
        {
            packFile = ZipFile.Read("Resources/" + pack + ".vnp");
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
