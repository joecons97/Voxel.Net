using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet.Physics
{
    public static class PhysicSimulation
    {
        public static Vector3 Gravity = new Vector3(0, -0.49f, 0);
        private static List<Rigidbody> rigidbodies = new List<Rigidbody>();

        public static void AddRigidbody(Rigidbody body)
        {
            rigidbodies.Add(body);
        }

        public static void RemoveRigidbody(Rigidbody body)
        {
            rigidbodies.Remove(body);
        }

        public static void Simulate(float deltaTime)
        {
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                var body = rigidbodies[i];
                var normVelocity = body.Velocity.Normalized();
                for (int c = 0; c < body.GetCollisionShapes().Length; c++)
                {
                    body.Velocity += Gravity;

                    if (body.Velocity.X != 0)
                    {
                        if (body.GetCollisionShapes()[c].IntersectsWorldDirectional(body, new Vector3(normVelocity.X / 10, .5f, 0)))
                        {
                            body.Velocity = new Vector3(0, body.Velocity.Y, body.Velocity.Z);
                        }
                    }

                    if (body.Velocity.Z != 0)
                    {
                        if (body.GetCollisionShapes()[c].IntersectsWorldDirectional(body, new Vector3(0, .5f, normVelocity.Z / 10)))
                        {
                            body.Velocity = new Vector3(body.Velocity.X, body.Velocity.Y, 0);
                        }
                    }

                    if (body.Velocity.Y > 0)
                    {
                        if (body.GetCollisionShapes()[c].IntersectsWorldDirectional(body, new Vector3(0, .1f, 0)))
                        {
                            body.Velocity = new Vector3(body.Velocity.X, 0, body.Velocity.Z);
                        }
                    }
                    else if (body.Velocity.Y < 0)
                    {
                        if (body.GetCollisionShapes()[c].IntersectsWorldDirectional(body, new Vector3(0, -.125f, 0)))
                        {
                            body.Velocity = new Vector3(body.Velocity.X, 0, body.Velocity.Z);
                        }
                    }
                }

                body.Owner.Position += body.Velocity * deltaTime;
            }
        }
    }
}
