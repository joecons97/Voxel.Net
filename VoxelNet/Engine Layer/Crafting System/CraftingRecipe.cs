using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Containers;

namespace VoxelNet.CraftingSystem
{
    public abstract class CraftingRecipe
    {
        public abstract string[] RecipeLayouts { get; }

        public abstract Dictionary<char, string> ItemsKey { get; }

        public abstract CraftingRecipeOutput Output { get; }

        public int Width => RecipeLayouts.Length;
        public int Height => RecipeLayouts[0].Length;

        public bool Matches(CraftingContainer container)
        {
            bool CheckMatch(int x, int y, bool flipX)
            {
                for (int i = 0; i < container.ContainerSize.X; i++)
                {
                    for (int j = 0; j < container.ContainerSize.Y; j++)
                    {
                        int k = i - x;
                        int l = j - y;

                        char ingredientKey = ' ';

                        if (k >= 0 && l >= 0 && k < Width && l < Height)
                        {
                            if (flipX)
                            {
                                ingredientKey = RecipeLayouts[l][Width - k - 1];
                            }
                            else
                            {
                                ingredientKey = RecipeLayouts[l][k];
                            }
                        }

                        if (ingredientKey == ' ' && container.GetItemStackByLocation(i, (int)container.ContainerSize.Y - j - 1) == null)
                            continue;

                        if (ingredientKey == ' ' ||
                            container.GetItemStackByLocation(i, (int)container.ContainerSize.Y - j - 1) == null ||
                            container.GetItemStackByLocation(i, (int)container.ContainerSize.Y - j - 1).ItemKey != ItemsKey[ingredientKey])
                            return false;
                    }
                }

                return true;
            }

            for (int i = 0; i <= container.ContainerSize.X - Width; i++)
            {
                for (int y = 0; y <= container.ContainerSize.Y - Height; y++)
                {
                    if(CheckMatch(i, y, true))
                        return true;

                    if (CheckMatch(i, y, false))
                        return true;
                }
            }

            return false;
        }
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
