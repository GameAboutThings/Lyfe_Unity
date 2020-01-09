using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta_MapGenerator;

public class TerrainChunk : ChunkTemplate
{
    TerrainData terrainData;
    PlanetData planetData;
    TERRAIN.ETerrain terrain;

    Vector2 size;

    HexTileMapGenerator_V2 mapGenerator;

    public TerrainChunk(TERRAIN.ETerrain _terrain, Vector3 _center, Vector2 _size, HexTileMapGenerator_V2 _mapGenerator)
    {
        terrain = _terrain;
        terrainData = TerrainData.GetTerrainData(_terrain);
        center = _center;
        size = _size;
        mapGenerator = _mapGenerator;
    }

    public TerrainData GetTerrainData()
    {
        return terrainData;
    }

    public TERRAIN.ETerrain GetTerrain()
    {
        return terrain;
    }
}
