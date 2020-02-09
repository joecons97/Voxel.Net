using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet.Physics
{
    public class Rigidbody
    {
        public Entity Owner { get; }
        public float Mass { get; set; } = 1;
        public float Drag { get; set; } = 0;
        public Vector3 Velocity { get; set; }
        public Vector3 AngularVelocity { get; set; }

        private Shape[] collisionShapes;

        public Rigidbody(Entity owner, float mass, params Shape[] colShapes)
        {
            Owner = owner;
            Mass = mass;

            collisionShapes = colShapes;

            PhysicSimulation.AddRigidbody(this);
        }

        public Shape[] GetCollisionShapes()
        {
            return collisionShapes;
        }

        public void AddImpluse(Vector3 impulseForce)
        {
            Velocity += impulseForce / Mass;
        }

        public void AddForce(Vector3 force)
        {
            Velocity += force * Time.DeltaTime / Mass;
        }
    }
}
