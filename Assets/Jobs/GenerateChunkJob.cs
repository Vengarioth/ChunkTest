using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Jobs
{
    public struct ChunkInformation
    {
        public int2 Position;
        public int Tile;
    }

    public struct GenerateChunkJob : IJobParallelFor
    {
        [ReadOnly]
        public int TilesCount;
        [ReadOnly]
        public NativeArray<int2> Positions;
        [WriteOnly]
        public NativeArray<ChunkInformation> CreatedChunks;

        public void Execute(int index)
        {
            var position = Positions[index];

            var value = SampleNoise(position, 0.01f, 5000f);
            var tileIndex = Mathf.FloorToInt(value * TilesCount);

            var chunk = new ChunkInformation
            {
                Position = position,
                Tile = tileIndex,
            };

            CreatedChunks[index] = chunk;
        }

        private static float SampleNoise(int2 position, float scale, float offset)
        {
            var x = (position.x * scale) + offset;
            var y = (position.y * scale) + offset;

            return Mathf.Clamp01(Mathf.PerlinNoise(x, y));
        }
    }
}
