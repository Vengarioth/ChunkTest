# ChunkTest

This repository contains an idea how to implement chunks using the Unity Job and ECS tech stack.

## Idea

The system handles a fixed number of chunks in memory and loads/unloads before or after gameplay simulation as needed. Like with entities removing a chunk during simulation is not allowed. There is an example `LivenessManager` which performs those actions in the project.

The `Map` holds a primary `NativeArray<ChunkData>` of chunk information and as many secondary `NativeArray<T>` as needed to store per-tile data. A Tile is very much like an Entity and per-tile data are very much like components.

The `ChunkData` contains an index into the per-tile arrays pointing to the first tile that belongs to the chunk, the following `CHUNK_WIDTH * CHUNK_HEIGHT` tiles also belong to that chunk. `ChunkData` also has an index to the chunks to the left, top, right and bottom of it in the `NativeArray<ChunkData>` (or -1 if there is no chunk). This effectively forms a _2D linked list_ where a System can perform lookups into the 4 cardinal directions by chasing the indices in `ChunkData`. At worst a system should have to look into the data of 4 different chunks, if the entity is positioned near a corner.

An Entity should always have an updated index on which chunk it is before dispatching ECS systems, so that jobs can use that index as a starting point for queries into the tile data. If the entity chunk index is set as a `SharedComponentData`, the ECS should group entities with the same value next to each other, so that chunks remain hot in memory [source](https://forum.unity.com/threads/can-sharedcomponentdata-be-used-for-an-efficient-spacial-chunk-system.601183/#post-4019524).

## Code Outline

```csharp
public class Map
{
    // Chunk Information
    public NativeArray<ChunkData> Chunks;

    // Per-tile collision data
    public NativeArray<CollisionData> CollisionData;

    // Per-tile custom data
    public NativeArray<MyOtherTileData> OtherTileData;
}

public struct ChunkData
{
    // Index into per-tile data arrays
    public int TileIndex;

    // Index into chunk to the left in ChunkData
    public int Left;

    // Index into chunk to the top in ChunkData
    public int Top;

    // Index into chunk to the right in ChunkData
    public int Right;

    // Index into chunk to the bottom in ChunkData
    public int Bottom;
}

public struct CollisionData
{
    /*
     * Bits:
     * 0 - Collides Left
     * 1 - Collides Top
     * 2 - Collides Right
     * 3 - Collides Bottom
     */
    public byte CollisionMask;
}

public struct MyOtherTileData
{
    public float MyOtherTileValue;
}
```

## Resources

* [RustConf 2018 - Closing Keynote - Using Rust For Game Development by Catherine West](https://www.youtube.com/watch?v=aKLntZcp27M)

## Licence

MIT, except for some art assets by [kenney.nl](https://kenney.nl/)  (see licence in folders)
