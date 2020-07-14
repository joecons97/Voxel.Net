using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelNet.Inventory_System
{
    public class InventorySlot
    {
        public short ItemID { get; }
        public short StackSize { get; set; } = 1;

        public InventorySlot(short id)
        {
            ItemID = id;
        }
    }
}
