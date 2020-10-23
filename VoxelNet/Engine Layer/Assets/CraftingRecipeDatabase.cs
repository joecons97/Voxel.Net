using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Containers;
using VoxelNet.CraftingSystem;
using VoxelNet.CraftingSystem.Recipes;

namespace VoxelNet.Assets
{
    public class CraftingRecipeDatabase
    {
        static List<CraftingRecipe> recipes = new List<CraftingRecipe>();

        public static void Init()
        {
            RegisterRecipe(new WoodenPlanksRecipe());
        }

        public static void RegisterRecipe(CraftingRecipe recipe)
        {
            if (recipes.Contains(recipe))
            {
                Debug.Log($"Recipe: {recipe.GetType().Name} has already been registered. Skipping.", DebugLevel.Warning);
                return;
            }
            recipes.Add(recipe);
        }

        public static CraftingRecipe GetMatchingRecipe(CraftingContainer container)
        {
            CraftingRecipe recipe = null;

            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe rec = recipes[i];

                if (rec.Matches(container))
                {
                    recipe = rec;
                    break;
                }
            }

            return recipe;
        }
    }
}
