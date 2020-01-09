using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta_MapGenerator;

public class TerrainChunk : ChunkTemplate
{
    TerrainData terrainData;
    PlanetData planetData;
    TERRAIN.ETerrain terrain;

    public TerrainData GetTerrainData()
    {
        return terrainData;
    }

    public TERRAIN.ETerrain GetTerrain()
    {
        return terrain;
    }
}
