using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Assets;

namespace VoxelNet.Containers
{
    public class CraftingContainer : Container
    {
        public override Vector2 ContainerSize { get; } = new Vector2(2, 2);

        public ItemStack Output { get; private set; } = null;

        public void UpdateOutput()
        {
            var recipe = CraftingRecipeDatabase.GetMatchingRecipe(this);
            if(recipe != null)
                Output = new ItemStack(recipe.Output.ItemKey, (int)recipe.Output.Count, new Vector2(ContainerSize.X + 1, ContainerSize.Y + 1));
        }
    }
}
