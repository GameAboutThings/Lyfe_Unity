using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta_MapGenerator;

public class MapChunk : ChunkTemplate
{
    Map map;
    Vector3[,] heightMap;
    List<GameObject> tiles;

    HexTileMapGenerator_V2 mapGenerator;
    private ComputeShader cs_CalculateTileHeights;
    private ComputeShader cs_CalculateTileSmoothnesses;

    int tileCountX = 50;
    int tileCountY = 50;
    int tileCountZ = 50;

    private float spacingX = 1f;
    private float spacingZ = 1f;
    private float granularity = .2f;
    private Vector2 seedOffset;

    PlanetData planetData;

    Vector3 actualCenter;

    public MapChunk(Map _map, Vector3 _center, int _tileCountX, int _tileCountY, int _tileCountZ)
    {
        map = _map;
        center = _center;
        tileCountX = _tileCountX;
        tileCountY = _tileCountY;
        tileCountZ = _tileCountZ;

        planetData = _map.GetPlanetData();
        mapGenerator = _map.GetMapGenerator();
        seedOffset = mapGenerator.GetSeed().GetWorldOffset();

        cs_CalculateTileHeights = Resources.Load<ComputeShader>("Shaders/cs_CalculateTileHeights");
        cs_CalculateTileSmoothnesses = Resources.Load<ComputeShader>("Shaders/cs_CalculateTileSmoothnesses");
    }

    public void GenerateMap()
    {
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

                if (x == tileCountX / 2 && z == tileCountZ / 2)
                    actualCenter = new Vector3(pos.x, 0, pos.z);

                GameObject tile = map.SetTile(pos);
                tiles.Add(tile);
                SetTileInfo(tile, x, z);
            }
        }
    }

    private void SetTileInfo(GameObject _tile, int _x, int _z)
    {
        _tile.GetComponentInChildren<MeshRenderer>().material.SetColor("Color_8F328A1B", CalculateTileColor(_tile.transform.position));
        //_tile.GetComponentInChildren<MeshRenderer>().material.color = CalculateTileColor(_tile.transform.position);     
    }

    public void Clear()
    {
        if (tiles == null)
            return;

        while (tiles.Count != 0)
        {
            mapGenerator.DestroyGameObject(tiles[0]);
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

    private float CalculateWeightedValue(float[] _values, float[] _distances)
    {
        float summands = 0;
        float sumDistances = 0;
        float maxDistance = 3 * (map.GetMapGenerator().GetPlanetData().GetTerrainSize().x + map.GetMapGenerator().GetPlanetData().GetTerrainSize().y) / 2f;
        int maxWeight = TERRAIN.maxWeight;

        for (int i = 0; i < _values.Length; i++)
        {

            for (int j = 1; j <= maxWeight; j++)
            {            
                if (_distances[i] <= (maxDistance / (float)j))
                {
                    sumDistances += _distances[i];
                    summands++;
                }
                else
                    continue;
            }
        }

        float value = 0;

        for (int i = 0; i < _values.Length; i++)
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

    private Vector3 GetHeightAt(Vector2 _pos, float[] _heights, float[] smoothnesses, int _x, int _z)
    {
        float y = StaticMaths.Cap(
            Mathf.PerlinNoise(_pos.x * granularity + seedOffset.x, _pos.y * granularity + seedOffset.y) * 10f / smoothnesses[_z * tileCountX + _x]
                + _heights[_z * tileCountX + _x],
            -tileCountY,
            tileCountY);

        float heightStep = mapGenerator.GetDiscreteHeightStep();

        if (heightStep != -1)
        {
            y = StaticMaths.Discretize(y, heightStep);
        }

        return new Vector3(_pos.x, y, _pos.y);
    }

    private float[] CalculateAllHeights(Vector2[] _tilePositions, TerrainChunk.TerrainChunkData[] _chunkData)
    {
        int kernel = cs_CalculateTileHeights.FindKernel("CSMain");

        float[] heights = new float[_tilePositions.Length];

        ComputeBuffer positionBuffer = new ComputeBuffer(_tilePositions.Length, sizeof(float) * 2);
        positionBuffer.SetData(_tilePositions);

        ComputeBuffer terrainBuffer = new ComputeBuffer(_chunkData.Length, sizeof(float) * 4);
        terrainBuffer.SetData(_chunkData);

        ComputeBuffer heightBuffer = new ComputeBuffer(_tilePositions.Length, sizeof(float));
        heightBuffer.SetData(heights);

        cs_CalculateTileHeights.SetBuffer(kernel, "tilePositions", positionBuffer);
        cs_CalculateTileHeights.SetBuffer(kernel, "chunks", terrainBuffer);
        cs_CalculateTileHeights.SetBuffer(kernel, "output_heights", heightBuffer);
        cs_CalculateTileHeights.SetFloat("terrainDimension", mapGenerator.GetTerrainDimension());
        cs_CalculateTileHeights.SetFloat("maxWeight", TERRAIN.maxWeight);
        cs_CalculateTileHeights.SetFloat("maxDistance", GetMaxDistance());
        cs_CalculateTileHeights.SetVector("tileCounts", new Vector2(tileCountX, tileCountZ));

        //Why in the name of all the fucks does it work with +10 but not without?
        cs_CalculateTileHeights.Dispatch(kernel, (tileCountX + 4) / 8, (tileCountZ + 4) / 8, 1);

        heightBuffer.GetData(heights);

        heightBuffer.Dispose();
        positionBuffer.Dispose();
        terrainBuffer.Dispose();

        return heights;
    }

    private float[] CalculateAllSmoothnesses(Vector2[] _tilePositions, TerrainChunk.TerrainChunkData[] _chunkData)
    {
        int kernel = cs_CalculateTileSmoothnesses.FindKernel("CSMain");

        float[] smoothnesses = new float[_tilePositions.Length];

        ComputeBuffer positionBuffer = new ComputeBuffer(_tilePositions.Length, sizeof(float) * 2);
        positionBuffer.SetData(_tilePositions);

        ComputeBuffer terrainBuffer = new ComputeBuffer(_chunkData.Length, sizeof(float) * 4);
        terrainBuffer.SetData(_chunkData);

        ComputeBuffer smoothnessBuffer = new ComputeBuffer(_tilePositions.Length, sizeof(float));
        smoothnessBuffer.SetData(smoothnesses);

        cs_CalculateTileSmoothnesses.SetBuffer(kernel, "tilePositions", positionBuffer);
        cs_CalculateTileSmoothnesses.SetBuffer(kernel, "chunks", terrainBuffer);
        cs_CalculateTileSmoothnesses.SetBuffer(kernel, "output_smoothnesses", smoothnessBuffer);
        cs_CalculateTileSmoothnesses.SetFloat("terrainDimension", mapGenerator.GetTerrainDimension());
        cs_CalculateTileSmoothnesses.SetFloat("maxWeight", TERRAIN.maxWeight);
        cs_CalculateTileSmoothnesses.SetFloat("maxDistance", GetMaxDistance());
        cs_CalculateTileSmoothnesses.SetVector("tileCounts", new Vector2(tileCountX, tileCountZ));

        //Why in the name of all the fucks does it work with +10 but not without?
        cs_CalculateTileSmoothnesses.Dispatch(kernel, (tileCountX + 7) / 8, (tileCountZ + 7) / 8, 1);

        smoothnessBuffer.GetData(smoothnesses);

        smoothnessBuffer.Dispose();
        positionBuffer.Dispose();
        terrainBuffer.Dispose();

        return smoothnesses;
    }

    private Vector2[] GetAllTilePositions(Vector3 _center)
    {
        Vector2[] pos = new Vector2[tileCountX * tileCountZ];

        for (int x = 0; x < tileCountX; x++)
        {
            for (int z = 0; z < tileCountZ; z++)
            {
                pos[z * tileCountX + x] = new Vector2(_center.x + x * spacingX, _center.z + z * spacingZ);
            }
        }

        return pos;
    }

    private TerrainChunk.TerrainChunkData[] GetAllChunkData()
    {
        TerrainChunk[,] chunks = mapGenerator.GetTerrainChunks();
        int dim1 = chunks.GetLength(0);
        int dim2 = chunks.GetLength(1);
        TerrainChunk.TerrainChunkData[] data = new TerrainChunk.TerrainChunkData[dim1 * dim2];

        for (int x = 0; x < dim1; x++)
        {
            for (int y = 0; y < dim2; y++)
            {
                data[y * dim1 + x] = chunks[x, y].GetData();
            }
        }

        return data;
    }

    private Vector3[,] GenerateComplexHeightMap()
    {
        Vector3 centerTemp = StaticMaths.DivideVector3D(center, TILES.Offset);

        Vector3[,] heightMap = new Vector3[tileCountX, tileCountZ];
        Vector2[] tilePositions = GetAllTilePositions(centerTemp);
        TerrainChunk.TerrainChunkData[] chunkData = GetAllChunkData();

        float[] heights = CalculateAllHeights(tilePositions, chunkData);
        float[] smoothnesses = CalculateAllSmoothnesses(tilePositions, chunkData);

        for (int x = 0; x < tileCountX; x++)
        {
            for (int z = 0; z < tileCountZ; z++)
            {
                heightMap[x, z] = GetHeightAt(tilePositions[z * tileCountX + x], heights, smoothnesses, x, z);
            }
        }

        return heightMap;
    }

    public Vector3 GetActualCenter()
    {
        return actualCenter;
    }

    public void SetCenter(Vector3 _position)
    {
        center = _position;
    }

    private float GetMaxDistance()
    {
        return 3.5f * (map.GetMapGenerator().GetPlanetData().GetTerrainSize().x + map.GetMapGenerator().GetPlanetData().GetTerrainSize().y) / 2f;
    }
}
