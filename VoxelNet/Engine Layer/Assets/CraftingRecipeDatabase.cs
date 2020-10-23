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

            var startingPoint = container.GetFirstFilledLocationInContainer();
            Debug.Log(startingPoint);
            //Container is empty
            if (startingPoint == new Vector2(-1, -1))
                return null;

            int sizeX = (int)(container.ContainerSize.X - startingPoint.X);
            int sizeY = (int)(container.ContainerSize.Y - startingPoint.Y);
            Debug.Log($"{sizeX}, {sizeY}");

            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe rec = recipes[i];

                //Skip recipe if it's too big to fit in the container
                /*if (rec.RecipeLayouts.Length > container.ContainerSize.Y || rec.RecipeLayouts[0].Length > container.ContainerSize.X)
                    continue;

                bool isValid = false;

                for (int x = (int)startingPoint.X; x <= Math.Min(sizeX, rec.RecipeLayouts[0].Length); x++)
                {
                    for (int y = (int)startingPoint.Y; y <= Math.Min(sizeY, rec.RecipeLayouts.Length); y++)
                    {
                        //Loop through the string in the recipe to see if it matches...
                        int recX = (int)(x - startingPoint.X);
                        int recY = (int)(y - startingPoint.Y);
                        Debug.Log($"{recX}, {recY}");

                        if (recY < rec.RecipeLayouts.Length && recX < rec.RecipeLayouts[recY].Length)
                        {
                            isValid = false;
                            char key = rec.RecipeLayouts[(rec.RecipeLayouts.Length -1) - recY][recX];
                            if (container.GetItemStackByLocation(x, y) != null && rec.ItemsKey[key] == container.GetItemStackByLocation(x, y).ItemKey)
                            {
                                isValid = true;
                            }
                        }
                    }
                }
                */

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
