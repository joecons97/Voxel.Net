using System;
using System.Collections.Generic;

namespace VoxelNet
{
    public class Menu
    {
		static List<Menu> openMenus = new List<Menu>();

        public static void GUIAll()
		{
            for(int i = 0; i < openMenus.Count; i++)
			{
				if (openMenus[i] != null)
					openMenus[i].GUI();
			}
		}

        public virtual void GUI()
		{

        }

        public virtual void Show()
		{
			openMenus.Add(this);
		}

        public virtual void Close()
		{
            if (openMenus.Contains(this))
            {
                openMenus.Remove(this);
            }
		}
    }
}
