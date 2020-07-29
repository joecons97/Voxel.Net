using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Rendering;

namespace VoxelNet.Containers
{
    public class PlayerInventory : Container
    {
        Vector2 size = new Vector2(9,4);
        public override Vector2 ContainerSize => size;

        public int SelectedItemIndex = 0;
        public static Texture SelectedSlotTexture { get; private set; }

        public PlayerInventory()
        {
            SelectedSlotTexture = AssetDatabase.GetAsset<Texture>("Resources/Textures/GUI/inventory_slot_selected.png");
        }

        public void RenderToolBar()
        {
            if (IsOpen)
                return;

            float winWidth = Program.Settings.WindowWidth;
            float winHeight = Program.Settings.WindowHeight;
            float slotSize = ContainerRenderer.SLOT_SIZE;
            Vector2 size = new Vector2(ContainerSize.X, 1) * slotSize * 2;
            int y = 0;

            Rect parentRect = new Rect((winWidth / 2f) - (size.X / 4f) - slotSize / 2f, (winHeight - (size.Y / 4f) - slotSize / 2f) - 32,
                (size.X / 2f + 35), (size.Y / 2f) + 8);

            GUI.Image(ContainerRenderer.ContainerBackground, parentRect, 5);

            for (int x = 0; x < ContainerSize.X; x++)
            {
                var rect = new Rect(x * (slotSize + 2) + parentRect.X + 10, y * (slotSize + 2) + parentRect.Y + 4,
                    slotSize, slotSize);

                RenderCell(x, y, rect);

                if (x == SelectedItemIndex)
                {
                    int offset = 8;
                    rect.X -= offset/2f;
                    rect.Width += offset;

                    rect.Y -= offset/2f;
                    rect.Height += offset;
                    GUI.Image(SelectedSlotTexture, rect);
                }

            }
        }

        public override void RenderGUI()
        {
            float winWidth = Program.Settings.WindowWidth;
            float winHeight = Program.Settings.WindowHeight;
            float slotSize = ContainerRenderer.SLOT_SIZE;
            Vector2 size = ContainerSize * slotSize * 2;

            Rect parentRect = new Rect((winWidth / 2f) - (size.X / 4f) - slotSize / 2f, (winHeight / 2f) - (size.Y / 4f) - slotSize / 2f,
                (size.X / 2f + 35), (size.Y / 2f + 30) + 12);

            GUI.Image(ContainerRenderer.ContainerBackground, parentRect, 5);

            bool anySlotSelected = false;
            for (int x = 0; x < ContainerSize.X; x++)
            {
                for (int y = (int)ContainerSize.Y - 1; y >= 0; y--)
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
            if (anySlotSelected == false)
                SelectedSlot = new Vector2(-1, -1);
        }
    }
}
