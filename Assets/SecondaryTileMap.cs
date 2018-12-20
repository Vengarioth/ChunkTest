using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

namespace ChunkedTileMapExample
{
    public class SecondaryTileMap<T> : IDisposable
        where T : struct
    {
        public NativeArray<T> Items => _items;
        private NativeArray<T> _items;

        public SecondaryTileMap()
        {
            _items = new NativeArray<T>(ChunkedTileMap.TILES_SIZE, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        }

        public void SetChunk(int index, T[] data)
        {
            _items.Slice(index * ChunkedTileMap.CHUNK_SIZE, ChunkedTileMap.CHUNK_SIZE).CopyFrom(data);
        }

        public void Dispose()
        {
            _items.Dispose();
        }
    }
}
