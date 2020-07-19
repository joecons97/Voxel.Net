using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using VoxelNet.Rendering;

namespace VoxelNet
{
    public class GUIStyle : ICloneable
    {
        public GUIStyleOption Normal { get; set; } = new GUIStyleOption();
        public GUIStyleOption Hover { get; set; } = new GUIStyleOption();
        public GUIStyleOption Active { get; set; } = new GUIStyleOption();
        public Font Font { get; set; }
        public float FontSize { get; set; }
        public float SlicedBorderSize { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }

        public Vector2 AlignmentOffset { get; set; } = new Vector2(0,0);

        public object Clone()
        {
            return new GUIStyle()
            {
                Normal = Normal,
                Hover = Hover,
                Active = Active,
                Font = Font,
                FontSize = FontSize,
                VerticalAlignment = VerticalAlignment,
                HorizontalAlignment = HorizontalAlignment,
                AlignmentOffset = AlignmentOffset,
                SlicedBorderSize = SlicedBorderSize
            };
        }
    }

    public enum VerticalAlignment
    {
        Top,
        Middle,
        Bottom
    }
    public enum HorizontalAlignment
    {
        Left,
        Middle,
        Right
    }

    public class GUIStyleOption
    {
        public Texture Background;
        public Color4 TextColor = Color4.White;

    }
}
