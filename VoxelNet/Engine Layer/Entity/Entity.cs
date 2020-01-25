using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Misc;

namespace VoxelNet.Entity
{
    public class Entity
    {
        public string Name;
        public Vector3 Position;
        public Vector3 Rotation;

        public virtual void Begin()
        {
        }

        public virtual void End()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void Render()
        {
        }

        public virtual void GUI()
        {
        }

        public Vector3 GetForwardVector()
        {
            return Maths.GetForwardFromRotation(Rotation);
        }

        public Vector3 GetRightVector()
        {
            return Maths.GetRightFromRotation(Rotation);
        }

        public Vector3 GetUpVector()
        {
            return Maths.GetUpFromRotation(Rotation);
        }

        public Vector2 GetChunk()
        {
            var pos = Position.ToChunkPosition();
            return new Vector2(pos.X, pos.Z);
        }

        public Vector3 GetPositionInChunk()
        {
            return Position.ToChunkSpace();
        }
    }
}
