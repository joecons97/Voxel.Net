using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet.Physics
{
    public abstract class Shape
    {
        public virtual bool Intersects(Shape shape, Rigidbody body)
        {
            return false;
        }

        public virtual bool IntersectsForcedOffset(Shape shape, Rigidbody body, Vector3 offset)
        {
            return false;
        }

        public virtual bool IntersectsWorld(Rigidbody body)
        {
            return false;
        }
        public virtual bool IntersectsWorldDirectional(Rigidbody body, Vector3 direction)
        {
            return false;
        }
    }
}
