using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace VoxelNet
{
    public class Menu
    {
		static List<Menu> openMenus = new List<Menu>();

        public bool IsOpen { get; private set; }

        public static void RenderGUI()
		{
            for(int i = 0; i < openMenus.Count; i++)
			{
				if (openMenus[i] != null)
					openMenus[i].OnGUI();
			}
		}

        public virtual void OnGUI()
		{

        }

        public virtual void Show()
        {
            if (!openMenus.Contains(this))
            {
                openMenus.Add(this);
                IsOpen = true;
            }
        }

        public virtual void Close()
		{
            if (openMenus.Contains(this))
            {
                openMenus.Remove(this);
                IsOpen = false;
            }
        }
    }
}
