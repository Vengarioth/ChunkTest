using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ChunkedTileMapExample
{
    public struct Chunk
    {
        // Index into Chunks for chunk to the left
        public int Left;
        // Index into Chunks for chunk to the top
        public int Top;
        // Index into Chunks for chunk to the right
        public int Right;
        // Index into Chunks for chunk to the bottom
        public int Bottom;
    }
    
    public class ChunkedTileMap : IDisposable
    {
        public const float TILE_SIZE = 1f;
        public const int CHUNK_WIDTH = 10;
        public const int CHUNK_HEIGHT = 10;
        public const int CHUNK_SIZE = CHUNK_WIDTH * CHUNK_HEIGHT;
        public const int MAX_LOADED_CHUNKS = 128;
        public const int TILES_SIZE = MAX_LOADED_CHUNKS * CHUNK_SIZE;

        public NativeArray<Chunk> Chunks => _chunks;
        private NativeArray<Chunk> _chunks;

        // Positions in chunk space
        private int2[] _positions;
        private List<int> _allocatedList;
        private Queue<int> _freeList;

        public ChunkedTileMap()
        {
            _chunks = new NativeArray<Chunk>(MAX_LOADED_CHUNKS, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            _positions = new int2[MAX_LOADED_CHUNKS];

            _allocatedList = new List<int>(MAX_LOADED_CHUNKS);
            _freeList = new Queue<int>(MAX_LOADED_CHUNKS);
            for (var i = 0; i < MAX_LOADED_CHUNKS; i++)
                _freeList.Enqueue(i);
        }

        public int2 ToChunkPosition(float2 position)
        {
            return new int2(
                Mathf.FloorToInt(position.x / CHUNK_WIDTH),
                Mathf.FloorToInt(position.y / CHUNK_HEIGHT)
            );
        }

        public int2 GetChunkPosition(int index)
        {
            return _positions[index];
        }

        public int FindChunkIndex(int2 position)
        {
            for(var i = 0; i < _allocatedList.Count; i++)
            {
                var index = _allocatedList[i];
                var other = _positions[index];
                if(other.x == position.x && other.y == position.y)
                {
                    return index;
                }
            }

            return -1;
        }

        public int[] GetLoadedChunks()
        {
            return _allocatedList.ToArray();
        }
        
        public bool IsChunkLoaded(int index)
        {
            return _allocatedList.Contains(index);
        }

        public int LoadChunk(int2 position)
        {
            if (FindChunkIndex(position) >= 0)
                throw new Exception("Chunk already loaded");

            if (_freeList.Count < 1)
                throw new Exception("No more space to allocate chunks");

            var index = _freeList.Dequeue();
            var left = -1;
            var top = -1;
            var right = -1;
            var bottom = -1;

            for(var i = 0; i < _allocatedList.Count; i++)
            {
                var id = _allocatedList[i];
                var otherPosition = _positions[id];
                var offset = otherPosition - position;

                if (offset.x == -1)
                {
                    left = id;
                    var other = _chunks[id];
                    other.Right = index;
                    _chunks[id] = other;
                }
                else if (offset.x == 1)
                {
                    right = id;
                    var other = _chunks[id];
                    other.Left = index;
                    _chunks[id] = other;
                }
                else if (offset.y == -1)
                {
                    bottom = id;
                    var other = _chunks[id];
                    other.Top = index;
                    _chunks[id] = other;
                }
                else if (offset.y == 1)
                {
                    top = id;
                    var other = _chunks[id];
                    other.Bottom = index;
                    _chunks[id] = other;
                }
            }

            _chunks[index] = new Chunk
            {
                Left = left,
                Top = top,
                Right = right,
                Bottom = bottom,
            };

            _positions[index] = position;
            _allocatedList.Add(index);

            return index;
        }

        public void UnloadChunk(int index)
        {
            if (index < 0)
                throw new Exception("Index cannot be smaller than 0");
            if (index >= MAX_LOADED_CHUNKS)
                throw new Exception("Index larger than chunks size");
            if (_freeList.Contains(index))
                throw new Exception("Chunk already unloaded");

            _freeList.Enqueue(index);
            _allocatedList.Remove(index);

            var chunk = _chunks[index];
            if (chunk.Left >= 0)
            {
                var other = _chunks[chunk.Left];
                other.Right = -1;
                _chunks[chunk.Left] = other;
            }
            if (chunk.Right >= 0)
            {
                var other = _chunks[chunk.Right];
                other.Left = -1;
                _chunks[chunk.Right] = other;
            }
            if (chunk.Top >= 0)
            {
                var other = _chunks[chunk.Top];
                other.Bottom = -1;
                _chunks[chunk.Top] = other;
            }
            if (chunk.Bottom >= 0)
            {
                var other = _chunks[chunk.Bottom];
                other.Top = -1;
                _chunks[chunk.Bottom] = other;
            }

            _chunks[index] = new Chunk
            {
                Left = -1,
                Top = -1,
                Right = -1,
                Bottom = -1,
            };
        }

        public void Dispose()
        {
            _chunks.Dispose();
        }
    }
}
