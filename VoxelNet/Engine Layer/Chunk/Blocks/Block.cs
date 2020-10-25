using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using VoxelNet.Assets;
using VoxelNet.Physics;

namespace VoxelNet.Blocks
{
    public class Block
    {
        public virtual string Key { get; set; } = "";
        public int ID { get; set; } = -1;
        public virtual GetBlockColor BlockColor { get; set; } = (x, y, z) => Color4.White;
        public virtual sbyte Opacity { get; set; } = 15;
        public virtual bool IsTransparent { get; set; } = false; 
        public virtual bool TransparencyCullsSelf { get; set; } = false;
        public virtual Shape CollisionShape { get; set; } = new BoundingBox(0,1,0,1,0,1);

        public Face TopFace { get; set; } = new Face(new Rect(0,0,1,1), new Rect(0, 0, 0, 0));
        public Face BottomFace { get; set; } = new Face(new Rect(0, 0, 1, 1), new Rect(0, 0, 0, 0));
        public Face LeftFace { get; set; } = new Face(new Rect(0, 0, 1, 1), new Rect(0, 0, 0, 0));
        public Face RightFace { get; set; } = new Face(new Rect(0, 0, 1, 1), new Rect(0, 0, 0, 0));
        public Face FrontFace { get; set; } = new Face(new Rect(0, 0, 1, 1), new Rect(0, 0, 0, 0));
        public Face BackFace { get; set; } = new Face(new Rect(0, 0, 1, 1), new Rect(0, 0, 0, 0));

        public Block()
        {
            BlockDatabase.RegisterBlock(this);
        }

        public virtual void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
        }

        public virtual void OnPlace(Vector3 WorldPosition, Vector2 ChunkPosition)
        {

        }

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
