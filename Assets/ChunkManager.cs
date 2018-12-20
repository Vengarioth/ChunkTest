using ChunkedTileMapExample;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct TileNavigationData
{
    public int Flags;
}

public class ChunkManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap _tilemap;
    [SerializeField]
    private Tile[] _tiles;

    [SerializeField]
    private Transform[] _entitiesToWatch;
    private int[] _entityChunks;

    private ChunkedTileMap _chunkMap;
    private SecondaryTileMap<TileNavigationData> _tileNavigation;

    private LivenessManager _liveness;
    private List<int2> _activeChunks;

    private void Start()
    {
        _chunkMap = new ChunkedTileMap();
        _tileNavigation = new SecondaryTileMap<TileNavigationData>();
        _liveness = new LivenessManager();
        _activeChunks = new List<int2>();

        for(var i = 0; i < _entitiesToWatch.Length; i++)
        {
            _liveness.AddLiveness(i, _chunkMap.ToChunkPosition((Vector2)_entitiesToWatch[i].position));
        }

        InitializeChunks();
    }

    private void Update()
    {
        UpdateChunks();
    }

    private void OnDrawGizmos()
    {
        if (_chunkMap == null)
            return;

        foreach (var position in _chunkMap.GetLoadedChunks().Select(i => _chunkMap.GetChunkPosition(i)))
        {
            var center = new Vector3(
                (position.x * ChunkedTileMap.CHUNK_WIDTH) + (ChunkedTileMap.CHUNK_WIDTH / 2),
                (position.y * ChunkedTileMap.CHUNK_HEIGHT) + (ChunkedTileMap.CHUNK_HEIGHT / 2),
                0f
            );
            var extends = new Vector3(
                ChunkedTileMap.CHUNK_WIDTH,
                ChunkedTileMap.CHUNK_HEIGHT,
                1f
            );

            if (_liveness.IsPivot(position))
            {
                var c = Color.cyan;
                c.a = 0.25f;
                Gizmos.color = c;
                Gizmos.DrawCube(center, extends);
            }
            else
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(center, extends);
            }

        }
    }

    private void OnDestroy()
    {
        _chunkMap.Dispose();
        _tileNavigation.Dispose();
    }

    private void InitializeChunks()
    {
        _liveness.Update();
        _activeChunks.AddRange(_liveness.GetLiveChunks());

        for(var i = 0; i < _activeChunks.Count; i++)
        {
            var chunkPosition = _activeChunks[i];
            GenerateChunkData(chunkPosition);
            var navigationData = new TileNavigationData[ChunkedTileMap.CHUNK_SIZE];
            var index = _chunkMap.LoadChunk(chunkPosition);
            _tileNavigation.SetChunk(index, navigationData);
        }
    }
    
    private void UpdateChunks()
    {
        for (var i = 0; i < _entitiesToWatch.Length; i++)
        {
            _liveness.UpdateLiveness(i, _chunkMap.ToChunkPosition((Vector2)_entitiesToWatch[i].position));
        }

        _liveness.Update();

        for (var i = 0; i < _activeChunks.Count; i++)
        {
            var chunkPosition = _activeChunks[i];
            if (_liveness.IsAlive(chunkPosition))
                continue;

            var index = _chunkMap.FindChunkIndex(chunkPosition);
            _chunkMap.UnloadChunk(index);
            UnloadChunkData(chunkPosition);
        }

        var newChunks = _liveness.GetLiveChunks();

        for (var i = 0; i < newChunks.Length; i++)
        {
            var chunkPosition = newChunks[i];
            if (_activeChunks.Contains(chunkPosition))
                continue;

            var navigationData = new TileNavigationData[ChunkedTileMap.CHUNK_SIZE];
            var index = _chunkMap.LoadChunk(chunkPosition);
            _tileNavigation.SetChunk(index, navigationData);
            GenerateChunkData(chunkPosition);
        }

        _activeChunks.Clear();
        _activeChunks.AddRange(newChunks);
    }

    private void UnloadChunkData(int2 position)
    {
        var bounds = new BoundsInt(
            position.x * ChunkedTileMap.CHUNK_WIDTH,
            position.y * ChunkedTileMap.CHUNK_HEIGHT,
            1,
            ChunkedTileMap.CHUNK_WIDTH,
            ChunkedTileMap.CHUNK_HEIGHT,
            1
        );

        _tilemap.SetTilesBlock(bounds, new TileBase[ChunkedTileMap.CHUNK_SIZE]);
    }

    private void GenerateChunkData(int2 position)
    {
        var bounds = new BoundsInt(
            position.x * ChunkedTileMap.CHUNK_WIDTH,
            position.y * ChunkedTileMap.CHUNK_HEIGHT,
            1,
            ChunkedTileMap.CHUNK_WIDTH,
            ChunkedTileMap.CHUNK_HEIGHT,
            1
        );

        var tiles = new TileBase[ChunkedTileMap.CHUNK_SIZE];

        for(var i = 0; i < tiles.Length; i++)
        {
            var x = i % ChunkedTileMap.CHUNK_WIDTH;
            var y = i / ChunkedTileMap.CHUNK_HEIGHT;

            var noiseX = 5000f + (bounds.xMin + x) * 0.01f;
            var noiseY = 5000f + (bounds.yMin + y) * 0.01f;
            var value = Mathf.PerlinNoise(noiseX, noiseY);
            var tileIndex = Mathf.FloorToInt(value * _tiles.Length);
            tileIndex = Math.Min(Math.Max(0, tileIndex), _tiles.Length - 1);

            var tile = _tiles[tileIndex];

            tiles[i] = tile;
        }

        _tilemap.SetTilesBlock(bounds, tiles);
    }
}
