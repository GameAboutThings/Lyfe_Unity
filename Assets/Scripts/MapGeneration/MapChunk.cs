using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta_MapGenerator;

public class MapChunk : ChunkTemplate
{
    private GameObject hexTilePrefab;

    HexTileMapGenerator_V2 mapGenerator;

    Vector3[,] heightMap;
    TERRAIN.ETerrain terrain; // remove
    TerrainData terrainData; // remove
    PlanetData planetData;

    List<GameObject> tiles;

    int tileCountX = 50;
    int tileCountY = 50;
    int tileCountZ = 50;

    private float spacingX = 1f;
    private float spacingZ = 1f;
    private float granularity = .2f;

    private Vector2 seedOffset;

    public void InitChunk(TERRAIN.ETerrain _terrain, Vector3 _center, int _tileCountX, int _tileCountY, int _tileCountZ, GameObject _hexTilePrefab, HexTileMapGenerator_V2 _mapGenerator)
    {
        terrain = _terrain;
        terrainData = TerrainData.GetTerrainData(terrain);
        center = _center;
        tileCountX = _tileCountX;
        tileCountY = _tileCountY;
        tileCountZ = _tileCountZ;
        hexTilePrefab = _hexTilePrefab;
        mapGenerator = _mapGenerator;
        seedOffset = UTIL.GetSeedVectorFromString(mapGenerator.GetSeed());
        planetData = _mapGenerator.GetPlanetData();
    }

    public void GenerateMap()
    {
        if (heightMap == null)
            heightMap = GenerateComplexHeightMap();

        if (tiles == null)
            tiles = new List<GameObject>();

        for (int x = 0; x < tileCountX; x++)
        {
            for (int z = 0; z < tileCountZ; z++)
            {
                Vector3 pos = heightMap[x, z];

                float y = pos.y;

                if (float.IsNaN(y))
                    y = 0;

                //move hex to its spot
                if (z % 2 == 0)
                {
                    pos = new Vector3(pos.x * TILES.Offset.x - (0.5f * tileCountX),
                                    y, 
                                    pos.z * TILES.Offset.z - (0.5f * tileCountZ));
                }
                else
                {
                    pos = new Vector3(pos.x * TILES.Offset.x + (TILES.Offset.x / 2f) - (0.5f * tileCountX), 
                                    y, 
                                    pos.z * TILES.Offset.z - (0.5f * tileCountZ));
                }

                GameObject tile = Instantiate(hexTilePrefab, pos, Quaternion.identity);
                tiles.Add(tile);
                SetTileInfo(tile, x, z);
            }
        }
    }

    private void SetTileInfo(GameObject _tile, int _x, int _z)
    {
        _tile.GetComponentInChildren<MeshRenderer>().material.SetColor("Color_8F328A1B", CalculateTileColor(_tile.transform.position));
        //_tile.GetComponentInChildren<MeshRenderer>().material.color = CalculateTileColor(_tile.transform.position);
        _tile.transform.parent = transform;
    }

    public void Clear()
    {
        if (tiles == null)
            return;

        while (tiles.Count != 0)
        {
            Destroy(tiles[0]);
            tiles.Remove(tiles[0]);
        }
    }

    private Color CalculateTileColor(Vector3 _tilePosition)
    {
        Color belowZero = planetData.GetColorBelowZero();
        Color zero = planetData.GetColorZero();
        Color levelOne = planetData.GetColorLevelOne();
        Color levelTwo = planetData.GetColorLevelTwo();

        float[] distances = new float[] {
            Mathf.Abs(_tilePosition.y - (-7f + mapGenerator.GetSeaLevel())),
            Mathf.Abs(_tilePosition.y - (0f + mapGenerator.GetSeaLevel())),
            Mathf.Abs(_tilePosition.y - (7f + mapGenerator.GetSeaLevel())),
            Mathf.Abs(_tilePosition.y - (12f + mapGenerator.GetSeaLevel()))
        };

        Color averageColor = new Color(
            //first component (RED)
            CalculateWeightedValue(new float[] { belowZero.r, zero.r, levelOne.r, levelTwo.r }, distances),
            //second component (GREEN)
            CalculateWeightedValue(new float[] { belowZero.g, zero.g, levelOne.g, levelTwo.g }, distances),
            //third component (BLUE)
            CalculateWeightedValue(new float[] { belowZero.b, zero.b, levelOne.b, levelTwo.b }, distances)
            );

        return averageColor;
    }

    private float CalculateTileSmoothness(Vector3 _tilePosition)
    {
        Vector2 pos2D = StaticMaths.ThreeDTo2D(_tilePosition, StaticMaths.EPlane.E_XZ);

        MapChunk[,] chunks = mapGenerator.GetChunks();
        float s00 = chunks[0, 0].terrainData.GetSmoothness();
        float s10 = chunks[1, 0].terrainData.GetSmoothness();
        float s20 = chunks[2, 0].terrainData.GetSmoothness();
        float s01 = chunks[0, 1].terrainData.GetSmoothness();
        float s11 = chunks[1, 1].terrainData.GetSmoothness();
        float s21 = chunks[2, 1].terrainData.GetSmoothness();
        float s02 = chunks[0, 2].terrainData.GetSmoothness();
        float s12 = chunks[1, 2].terrainData.GetSmoothness();
        float s22 = chunks[2, 2].terrainData.GetSmoothness();

        float smoothness = CalculateWeightedValue(new float[] { s00, s10, s20, s01, s11, s12, s20, s21, s22 }, GetDistancesToChunkCenters(pos2D));

        return  smoothness;
    }

    private float CalculateTileHeight(Vector3 _tilePosition)
    {
        Vector2 pos2D = StaticMaths.ThreeDTo2D(_tilePosition, StaticMaths.EPlane.E_XZ);

        MapChunk[,] chunks = mapGenerator.GetChunks();
        float h00 = chunks[0, 0].terrainData.GetHeight();
        float h10 = chunks[1, 0].terrainData.GetHeight();
        float h20 = chunks[2, 0].terrainData.GetHeight();
        float h01 = chunks[0, 1].terrainData.GetHeight();
        float h11 = chunks[1, 1].terrainData.GetHeight();
        float h21 = chunks[2, 1].terrainData.GetHeight();
        float h02 = chunks[0, 2].terrainData.GetHeight();
        float h12 = chunks[1, 2].terrainData.GetHeight();
        float h22 = chunks[2, 2].terrainData.GetHeight();  

        float smoothness = CalculateWeightedValue(new float[] { h00, h10, h20, h01, h11, h12, h20, h21, h22 }, GetDistancesToChunkCenters(pos2D));

        return smoothness;
    }

    private float[] GetDistancesToChunkCenters(Vector2 _tilePosition)
    {
        Vector2 pos2D = _tilePosition;

        MapChunk[,] chunks = mapGenerator.GetChunks();

        Vector2 cen00 = StaticMaths.ThreeDTo2D(chunks[0, 0].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen10 = StaticMaths.ThreeDTo2D(chunks[1, 0].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen20 = StaticMaths.ThreeDTo2D(chunks[2, 0].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen01 = StaticMaths.ThreeDTo2D(chunks[0, 1].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen11 = StaticMaths.ThreeDTo2D(chunks[1, 1].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen21 = StaticMaths.ThreeDTo2D(chunks[2, 1].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen02 = StaticMaths.ThreeDTo2D(chunks[0, 2].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen12 = StaticMaths.ThreeDTo2D(chunks[1, 2].GetCenter(), StaticMaths.EPlane.E_XZ);
        Vector2 cen22 = StaticMaths.ThreeDTo2D(chunks[2, 2].GetCenter(), StaticMaths.EPlane.E_XZ);

        float d00 = StaticMaths.Distance2D(pos2D, cen00);
        float d10 = StaticMaths.Distance2D(pos2D, cen10);
        float d20 = StaticMaths.Distance2D(pos2D, cen20);
        float d01 = StaticMaths.Distance2D(pos2D, cen01);
        float d11 = StaticMaths.Distance2D(pos2D, cen11);
        float d21 = StaticMaths.Distance2D(pos2D, cen21);
        float d02 = StaticMaths.Distance2D(pos2D, cen02);
        float d12 = StaticMaths.Distance2D(pos2D, cen12);
        float d22 = StaticMaths.Distance2D(pos2D, cen22);

        return new float[] { d00, d10, d20, d01, d11, d12, d20, d21, d22 };
    }

    private float CalculateWeightedValue(float[] _values, float[] _distances)
    {
        float summands = 0;
        float sumDistances = 0;
        float diagonal = Mathf.Sqrt(Mathf.Pow(CHUNK.tileCount.x * TILES.Offset.x, 2) + Mathf.Pow(CHUNK.tileCount.y * TILES.Offset.y, 2));
        float maxDistance = (5f / 2f) * diagonal;
        int maxWeight = 50;


        for(int i = 0 ; i < _values.Length; i++)
        {
            for (int j = 1; j <= maxWeight; j++)
            {
                if (_distances[i] <= (maxDistance / (float)j))
                {
                    sumDistances += _distances[i];
                    summands++;
                }
                else
                    j = maxWeight + 2;
            }
        }

        float value = 0;

        for(int i = 0 ; i < _values.Length; i++)
        {
            for (int j = 1; j < maxWeight; j++)
            {
                if (_distances[i] <= (maxDistance / (float)j))
                {
                    value += (_values[i] * (sumDistances - _distances[i]) / sumDistances);
                }
                else
                    j = maxWeight;
            }
        }

        value /= summands;

        return value;
    }

    public Vector3 GetHeightAt(float _x, float _z)
    {
        Vector3 pos = new Vector3(_x * spacingX, 0, _z * spacingZ);
        float y = StaticMaths.Cap(
            Mathf.PerlinNoise(_x * granularity + seedOffset.x, _z * granularity + seedOffset.y) * 10f / CalculateTileSmoothness(pos)
                + CalculateTileHeight(pos), 
            -tileCountY,
            tileCountY);

        float heightStep = mapGenerator.GetDiscreteHeightStep();

        if (heightStep != -1)
        {
            y = StaticMaths.Descretize(y, heightStep);
        }

        return new Vector3(pos.x, y, pos.z);
    }

    public Vector3[,] GenerateComplexHeightMap()
    {
        Vector3 centerTemp = StaticMaths.DivideVector3D(center, TILES.Offset);

        Vector3[,] heightMap = new Vector3[tileCountX + 1, tileCountZ + 1];

        for (int z = 0; z <= tileCountZ; z++)
        {
            for (int x = 0; x <= tileCountX; x++)
            {
                heightMap[x, z] = GetHeightAt(x + centerTemp.x, z + centerTemp.z);
            }
        }

        return heightMap;
    }

    public TerrainData GetTerrainData() //remove
    {
        return terrainData;
    }

    public TERRAIN.ETerrain GetTerrain() //remove
    {
        return terrain;
    }
}
