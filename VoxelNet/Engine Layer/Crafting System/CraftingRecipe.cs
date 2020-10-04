using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelNet.CraftingSystem
{
    public abstract class CraftingRecipe
    {
        public abstract string[] RecipeLayouts { get; }

        public abstract Dictionary<char, string> ItemsKey { get; }

        public abstract CraftingRecipeOutput Output { get; }
    }

    public struct CraftingRecipeOutput
    {
        public string ItemKey { get; }
        public byte Count { get; }

        public CraftingRecipeOutput(string itemKey, byte count)
        {
            ItemKey = itemKey;
            Count = count;
        }
    }
}
