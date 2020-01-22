using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using VoxelNet.Assets;

namespace VoxelNet.Blocks
{
    public class Block
    {
        public virtual string Key { get; set; } = "NULL";
        public virtual int ID { get; set; } = -1;
        public virtual GetBlockColor BlockColor { get; set; } = (x, y, z) => Color4.White;

        public Face TopFace { get; set; }
        public Face BottomFace { get; set; }
        public Face LeftFace { get; set; }
        public Face RightFace { get; set; }
        public Face FrontFace { get; set; }
        public Face BackFace { get; set; }

        public class Face
        {
            public Face(Rect uvs, Rect mask)
            {
                UVCoordinates = uvs;
                Mask = mask;

                if (Mask.X == -1 || Mask.Y == -1)
                {
                    Mask = new Rect(-1, -1, -1, -1);
                }
            }

            public Rect UVCoordinates { get; }
            public Rect Mask { get; }
        }
    }
}
