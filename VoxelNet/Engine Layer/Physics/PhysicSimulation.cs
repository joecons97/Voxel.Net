﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace VoxelNet.Physics
{
    public static class PhysicSimulation
    {
        public static float FixedTimeStep = 0.02f;
        public static Vector3 Gravity = new Vector3(0, -25, 0);
        private static List<Rigidbody> rigidbodies = new List<Rigidbody>();

        public static void AddRigidbody(Rigidbody body)
        {
            rigidbodies.Add(body);
        }

        public static void RemoveRigidbody(Rigidbody body)
        {
            rigidbodies.Remove(body);
        }

        public static void Simulate()
        {
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                var body = rigidbodies[i];
                if (!body.IsActive) continue;
                var normVelocity = body.Velocity.Normalized();
                for (int c = 0; c < body.GetCollisionShapes().Length; c++)
                {
                    body.Velocity += Gravity * FixedTimeStep;

                    if (body.Velocity.X != 0)
                    {
                        if (body.GetCollisionShapes()[c].IntersectsWorldDirectional(body, new Vector3(normVelocity.X / 10, .25f, 0)))
                        {
                            body.Velocity = new Vector3(0, body.Velocity.Y, body.Velocity.Z);
                        }
                    }

                    if (body.Velocity.Z != 0)
                    {
                        if (body.GetCollisionShapes()[c].IntersectsWorldDirectional(body, new Vector3(0, .25f, normVelocity.Z / 10)))
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
                        if (body.GetCollisionShapes()[c].IntersectsWorldDirectional(body, new Vector3(0, -.1f, 0)))
                        {
                            body.Velocity = new Vector3(body.Velocity.X, 0, body.Velocity.Z);
                            body.Owner.Position = new Vector3(body.Owner.Position.X, (float)Math.Round(body.Owner.Position.Y), body.Owner.Position.Z);
                        }
                    }
                }

                body.Velocity *= 1 / (1 + body.Drag * FixedTimeStep);//*= MathHelper.Clamp(1f - body.Drag * deltaTime, 0, 1);

                body.Owner.Position += body.Velocity * FixedTimeStep;
            }
        }
    }
}
