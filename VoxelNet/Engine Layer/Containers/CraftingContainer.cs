using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Assets;
using VoxelNet.CraftingSystem;

namespace VoxelNet.Containers
{
    public class CraftingContainer : Container
    {
        public override Vector2 ContainerSize { get; } = new Vector2(2, 2);

        public CraftingRecipe OutputRecipe { get; private set; } = null;
        public ItemStack OutputStack { get; private set; } = null;

        public void UpdateOutput()
        {
            var recipe = CraftingRecipeDatabase.GetMatchingRecipe(this);
            OutputRecipe = recipe;

            if (OutputRecipe != null)
                OutputStack = new ItemStack(OutputRecipe.Output.ItemKey, OutputRecipe.Output.Count,
                    new Vector2(-1, -1));
            else
                OutputStack = null;
        }

        public override void RenderGUI()
        {
            float winWidth = Window.WindowWidth;
            float winHeight = Window.WindowHeight;

            float slotSize = ContainerRenderer.SLOT_SIZE;
            Vector2 size = ContainerSize * slotSize * 2;

            Rect parentRect = new Rect((winWidth / 2f) - (size.X / 4f) - slotSize / 2f, (winHeight / 2f) - (size.Y/1.25f) - slotSize / 2f,
                (size.X / 2f + 35), (size.Y / 2f + 30) + 12);

            bool anySlotSelected = false;
            for (int x = 0; x < ContainerSize.X; x++)
            {
                for (int y = 0; y < ContainerSize.Y; y++)
                {
                    var rect = new Rect(x * (slotSize + 2) + parentRect.X + 10, ((ContainerSize.Y - 1) - y) * (slotSize + 2) + parentRect.Y + 10,
                        slotSize, slotSize);

                    if (y == 0)
                        rect.Y += 12;

                    if (rect.IsPointInside(GUI.MousePosition))
                    {
                        SelectedSlot = new Vector2(x, y);
                        anySlotSelected = true;
                    }

                    RenderCell(x, y, rect);
                }
            }

            var outputRect = new Rect(2 * (slotSize + 2) + parentRect.X + 10, ((ContainerSize.Y - 1) - 0.5f) * (slotSize + 2) + parentRect.Y + 10,
                slotSize, slotSize);
            RenderOutputCell(outputRect);

            if (anySlotSelected == false)
                SelectedSlot = new Vector2(-1, -1);
        }

        public override void ItemDroppedIntoContainer(ItemStack itemStack)
        {
            UpdateOutput();
        }

        public override void ItemRemovedFromContainer(ItemStack itemStack)
        {
            UpdateOutput();
        }

        void RemoveOneOfEach()
        {
            List<ItemStack> indicesToRemove =  new List<ItemStack>();
            for (int i = 0; i < ItemsList.Count; i++)
            {
                if(ItemsList[i].RemoveFromStack() == ItemStackState.Empty)
                    indicesToRemove.Add(ItemsList[i]);
            }

            for (int i = 0; i < indicesToRemove.Count; i++)
            {
                ItemsList.Remove(indicesToRemove[i]);
            }
        }

        protected void RenderOutputCell(Rect rect)
        {
            var rectIcon = new Rect(rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8);

            GUI.Image(ContainerRenderer.ContainerSlot, rect);

            var stack = OutputStack;
            if (stack != null)
            {
                stack = (ItemStack)stack.Clone();
                if (GUI.PressButton(stack.Item.Icon, rectIcon, SlotStyle))
                {
                    if (ContainerRenderer.SelectedStack == null)
                    {
                        stack.PreviousParent = this;
                        ContainerRenderer.SelectedStack = stack;
                        RemoveOneOfEach();
                        UpdateOutput();
                    }
                }

                GUI.Label(stack.StackSize.ToString(), rect, ContainerRenderer.SlotNumberStyle);
            }
        }
    }
}
