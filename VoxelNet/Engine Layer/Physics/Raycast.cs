using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using VoxelNet.Assets;

namespace VoxelNet.Physics
{
    public struct RayVoxelOut
    {
        public short BlockID;
        public Vector3 BlockPosition;
        public Vector2 ChunkPosition;

        public Vector3 PlacementPosition;
        public Vector2 PlacementChunk;

        public Vector3 HitNormal;
    }

    public static class Raycast
    {
        private const float stepSize = 0.1f;

        public static bool CastVoxel(Vector3 position, Vector3 direction, float distance, out RayVoxelOut output)
        {
            RayVoxelOut op = new RayVoxelOut();
            Vector3 curPos = position;
            Vector3 lastPos = new Vector3();
            float distTravelled = 0;

            World world = World.GetInstance();

            while (distTravelled < distance)
            {
                var chunkPos = curPos.ToChunkPosition();
                var pos = curPos.ToChunkSpaceFloored();

                if(world.TryGetChunkAtPosition((int)chunkPos.X, (int)chunkPos.Z, out Chunk chunk))
                {
                    var possibleBlock = BlockDatabase.GetBlock(chunk.GetBlockID((int)(pos.X), (int)(pos.Y), (int)(pos.Z)));

                    if (possibleBlock?.CollisionShape != null)
                    {
                        var blockPos = (chunkPos * Chunk.WIDTH) + pos;
                        if (possibleBlock.CollisionShape.IntersectsForcedOffset(blockPos, curPos))
                        {
                            op.BlockID = (short) possibleBlock.ID;
                            op.BlockPosition = new Vector3((int)(pos.X), (int)(pos.Y),(int)(pos.Z));
                            op.ChunkPosition = new Vector2((int) chunkPos.X, (int) chunkPos.Z);

                            var placeChunk = lastPos.ToChunkPosition();
                            var placePos = lastPos.ToChunkSpaceFloored();

                            op.PlacementPosition = new Vector3((int)(placePos.X),(int)(placePos.Y), (int)(placePos.Z));
                            op.PlacementChunk = new Vector2((int) placeChunk.X, (int) placeChunk.Z);

                            output = op;
                            return true;
                        }
                    }
                }

                lastPos = curPos;
                curPos += direction * stepSize;
                distTravelled += stepSize;
            }

            output = default;
            return false;
        }
    }
}
