using VoxelNet.Rendering.Material;

namespace VoxelNet.PostProcessing
{
    public class ACESTonemapEffect : BlitEffect
    {
        public override Material BlitMaterial => AssetDatabase.GetAsset<Material>("Resources/Materials/Post Processing/ACESPPE.mat");
    }
}
