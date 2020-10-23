using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelNet.CraftingSystem.Recipes
{
    public class WoodenPlanksRecipe : CraftingRecipe
    {
        public override string[] RecipeLayouts { get; } = new[]
        {
            "#"
        };
        public override Dictionary<char, string> ItemsKey { get; } = new Dictionary<char, string>()
        {
            {'#', GameItems.LOG_OAK.Key }
        };

        public override CraftingRecipeOutput Output { get; } = new CraftingRecipeOutput(GameItems.PLANKS_OAK.Key, 4);
    }
}
