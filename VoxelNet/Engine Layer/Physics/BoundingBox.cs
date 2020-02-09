using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Assets;
using VoxelNet.Blocks;

namespace VoxelNet.Physics
{
    public class BoundingBox : Shape
    {
        public Vector3 Min { get; }

        public Vector3 Max { get; }

        public Vector3 Size
        {
            get { return Max - Min; }
        }

        public BoundingBox(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            Min = new Vector3(minX, minY, minZ);
            Max = new Vector3(maxX, maxY, maxZ);
        }

        public override bool Intersects(Shape shape, Rigidbody body)
        {
            return IntersectsForcedOffset(shape, body, Vector3.Zero);
        }

        public override bool IntersectsForcedOffset(Shape shape, Rigidbody body, Vector3 offset)
        {
            if (shape is BoundingBox box)
            {
                return (offset.X + Min.X < body.Owner.Position.X + box.Max.X &&
                        offset.X + Max.X > body.Owner.Position.X + box.Min.X &&
                        offset.Y + Min.Y < body.Owner.Position.Y + box.Max.Y &&
                        offset.Y + Max.Y > body.Owner.Position.Y + box.Min.Y &&
                        offset.Z + Min.Z < body.Owner.Position.Z + box.Max.Z &&
                        offset.Z + Max.Z > body.Owner.Position.Z + box.Min.Z);
            }

            return false;
        }

        public override bool IntersectsWorld(Rigidbody body)
        {
            return IntersectsWorldDirectional(body, body.Velocity);
        }

        public override bool IntersectsWorldDirectional(Rigidbody body, Vector3 direction)
        {
            //Bottom
            bool BottomBackLeft()
            {
                var pos = body.Owner.Position + new Vector3(Min.X, Min.Y, Min.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return DoPos(chunkPosition, posInChunk);
            }
            bool BottomBackRight()
            {
                var pos = body.Owner.Position + new Vector3(Max.X, Min.Y, Min.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return DoPos(chunkPosition, posInChunk);
            }
            bool BottomFrontLeft()
            {
                var pos = body.Owner.Position + new Vector3(Min.X, Min.Y, Max.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return DoPos(chunkPosition, posInChunk);
            }
            bool BottomFrontRight()
            {
                var pos = body.Owner.Position + new Vector3(Max.X, Min.Y, Max.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return DoPos(chunkPosition, posInChunk);
            }

            //Top
            bool TopBackLeft()
            {
                var pos = body.Owner.Position + new Vector3(Min.X, Max.Y, Min.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return DoPos(chunkPosition, posInChunk);
            }
            bool TopBackRight()
            {
                var pos = body.Owner.Position + new Vector3(Max.X, Max.Y, Min.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return DoPos(chunkPosition, posInChunk);
            }
            bool TopFrontLeft()
            {
                var pos = body.Owner.Position + new Vector3(Min.X, Max.Y, Max.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return DoPos(chunkPosition, posInChunk);
            }
            bool TopFrontRight()
            {
                var pos = body.Owner.Position + new Vector3(Max.X, Max.Y, Max.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return DoPos(chunkPosition, posInChunk);
            }

            bool DoPos(Vector3 chunkPosition, Vector3 posInChunk)
            {
                if (World.GetInstance().TryGetChunkAtPosition((int)chunkPosition.X, (int)chunkPosition.Z, out Chunk chunk))
                {
                    short id = chunk.GetBlockID((int)(posInChunk.X), (int)(posInChunk.Y), (int)(posInChunk.Z));
                    if (id == 0)
                        return false;

                    Block block = BlockDatabase.GetBlock(id);
                    if (block == null)
                        return false;

                    if (block.CollisionShape != null)
                    {
                        var blockPos = (chunkPosition * Chunk.WIDTH) + posInChunk;
                        return block.CollisionShape.IntersectsForcedOffset(this, body, blockPos - direction);
                    }
                    return false;
                }

                return false;
            }

            if (!(direction.X == 0 && direction.Z == 0 && direction.Y > 0))
            {
                if (BottomBackLeft())
                    return true;
                if (BottomBackRight())
                    return true;
                if (BottomFrontLeft())
                    return true;
                if (BottomFrontRight())
                    return true;
            }

            if (direction.Y >= 0)
            {
                direction.Y = -direction.Y;

                if (TopBackLeft())
                    return true;
                if (TopBackRight())
                    return true;
                if (TopFrontLeft())
                    return true;
                if (TopFrontRight())
                    return true;
            }


            return false;
        }
    }
}
